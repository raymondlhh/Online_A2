using UnityEngine;
using Photon.Pun;
using System.Collections;
using TMPro;

public class PlayerConnector : MonoBehaviourPunCallbacks
{
    public enum ConnectionResult { Success, Failed }

    [Header("Settings")]
    public Transform connectionSlot; 
    public float connectRange = 15f;
    
    [Header("UI")]
    public GameObject connectedUI;
    public TextMeshProUGUI connectedDurationText;
    
    [Header("Victim Connection UI")]
    public GameObject victimConnectedUI;
    public TextMeshProUGUI victimConnectedDurationText;
    
    private Camera fpsCamera;
    private PlayerMovement movementController;
    private PlayerSkillDetails[] skillDetails;

    private bool isAttached = false;
    private Transform attachTarget; 
    private Coroutine connectionCoroutine;
    private Coroutine victimConnectionCoroutine;
    private PhotonView connectedPlayerView;
    private Victim connectedVictim;
    public PlayerMovement ConnectedPlayerMovement { get; private set; }

    void Start()
    {
        if (photonView.IsMine)
        {
            fpsCamera = GetComponentInChildren<Camera>(); 
        }
        movementController = GetComponent<PlayerMovement>();
        skillDetails = GetComponentsInChildren<PlayerSkillDetails>(true); 
    }

    void Update()
    {
        if (photonView.IsMine && isAttached && attachTarget != null)
        {
            transform.position = attachTarget.position;
        }
    }

    public ConnectionResult TryConnect(float duration)
    {
        if (fpsCamera == null) return ConnectionResult.Failed;

        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, connectRange))
        {
            // Check for Victim first
            Victim victim = hit.collider.GetComponentInParent<Victim>();
            if (victim != null)
            {
                // The check for the victim being saved has been removed.
                // A player can now connect to any victim.
                
                // Connect victim to player
                connectedVictim = victim;
                ConnectedPlayerMovement = null; // Ensure we're not tracking a player
                
                // Show victim connection UI and start duration display
                if (photonView.IsMine)
                {
                    if (victimConnectedUI != null) victimConnectedUI.SetActive(true);
                    if (victimConnectionCoroutine != null) StopCoroutine(victimConnectionCoroutine);
                    victimConnectionCoroutine = StartCoroutine(VictimConnectionDurationDisplay(duration));
                }
                
                victim.GetComponent<PhotonView>().RPC("GetConnectedToPlayer", RpcTarget.All, photonView.ViewID, duration);
                return ConnectionResult.Success;
            }

            // Check for Player (existing logic)
            PhotonView targetView = hit.collider.GetComponentInParent<PhotonView>();
            if (targetView != null && !targetView.IsMine)
            {
                PlayerHealth targetHealth = targetView.GetComponent<PlayerHealth>();
                if (targetHealth != null && targetHealth.IsDowned)
                {
                    return ConnectionResult.Failed; // Don't connect to downed players
                }

                connectedPlayerView = targetView;
                ConnectedPlayerMovement = targetView.GetComponent<PlayerMovement>(); // Store the component
                connectedVictim = null; // Ensure we're not tracking a victim
                targetView.RPC("GetConnected", RpcTarget.All, photonView.ViewID, duration);
                return ConnectionResult.Success; // Success
            }
        }
        return ConnectionResult.Failed; // Failed to connect
    }

    [PunRPC]
    public void GetConnected(int connectorViewID, float duration)
    {
        PhotonView connectorView = PhotonView.Find(connectorViewID);
        if (connectorView == null) return;
        
        PlayerConnector connector = connectorView.GetComponent<PlayerConnector>();
        if (connector == null || connector.connectionSlot == null) return;

        if (connectionCoroutine != null) StopCoroutine(connectionCoroutine);
        connectionCoroutine = StartCoroutine(ConnectionLifetime(connector.connectionSlot, duration));
    }

    public void CancelConnection()
    {
        if (connectedPlayerView != null)
        {
            // Reset the connected player's physics state before detaching
            if (ConnectedPlayerMovement != null)
            {
                connectedPlayerView.RPC("SetKinematicState", RpcTarget.All, false);
            }

            connectedPlayerView.RPC("ForceDetach", RpcTarget.All);
            connectedPlayerView = null;
            ConnectedPlayerMovement = null;
        }
        
        if (connectedVictim != null)
        {
            connectedVictim.GetComponent<PhotonView>().RPC("ForceDetachFromPlayer", RpcTarget.All);
            
            // Hide victim connection UI
            if (photonView.IsMine)
            {
                if (victimConnectedUI != null) victimConnectedUI.SetActive(false);
                if (victimConnectionCoroutine != null)
                {
                    StopCoroutine(victimConnectionCoroutine);
                    victimConnectionCoroutine = null;
                }
            }
            
            connectedVictim = null;
        }
    }

    [PunRPC]
    public void ForceDetach()
    {
        if (isAttached)
        {
            if (connectionCoroutine != null)
            {
                StopCoroutine(connectionCoroutine);
            }
            Detach();
        }
    }

    private void Detach()
    {
        isAttached = false;
        attachTarget = null;
        
        if (photonView.IsMine)
        {
            movementController.CanMove = true;
            foreach (var skill in skillDetails)
            {
                skill.enabled = true;
            }

            if (connectedUI != null) connectedUI.SetActive(false);
        }
        connectionCoroutine = null;
    }

    [PunRPC]
    public void RPC_ForceReleaseVictim()
    {
        // This is called by a victim that has entered a save zone.
        // It runs on the Player who was connected to the victim.
        if (photonView.IsMine)
        {
            // Hide victim connection UI
            if (victimConnectedUI != null) victimConnectedUI.SetActive(false);
            if (victimConnectionCoroutine != null)
            {
                StopCoroutine(victimConnectionCoroutine);
                victimConnectionCoroutine = null;
            }
            
            connectedVictim = null;
    
            // Find the connect skill and cancel its active UI state.
            foreach (var skill in skillDetails)
            {
                if (skill.isConnectMovementSkill)
                {
                    skill.CancelConnectSkillUI();
                    break;
                }
            }
        }
    }

    private IEnumerator ConnectionLifetime(Transform targetSlot, float duration)
    {
        isAttached = true;
        attachTarget = targetSlot;
        
        if (photonView.IsMine)
        {
            movementController.CanMove = false;
            foreach (var skill in skillDetails)
            {
                skill.enabled = false;
            }

            if (connectedUI != null) connectedUI.SetActive(true);
        }

        float remainingDuration = duration;
        while (remainingDuration > 0f)
        {
            if (photonView.IsMine && connectedDurationText != null)
            {
                connectedDurationText.text = $"YOU ARE BEING CONNECTED: {Mathf.CeilToInt(remainingDuration)}s";
            }
            yield return new WaitForSeconds(1f);
            remainingDuration -= 1f;
        }

        Detach();
    }

    private IEnumerator VictimConnectionDurationDisplay(float duration)
    {
        float remainingDuration = duration;
        while (remainingDuration > 0f && connectedVictim != null)
        {
            if (victimConnectedDurationText != null)
            {
                victimConnectedDurationText.text = $"VICTIM IS BEING CONNECTED: {Mathf.CeilToInt(remainingDuration)}s";
            }
            yield return new WaitForSeconds(1f);
            remainingDuration -= 1f;
        }

        // Hide UI when connection ends
        if (photonView.IsMine)
        {
            if (victimConnectedUI != null) victimConnectedUI.SetActive(false);
        }
        victimConnectionCoroutine = null;
    }
} 