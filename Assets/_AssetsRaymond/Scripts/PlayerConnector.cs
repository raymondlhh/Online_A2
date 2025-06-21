using UnityEngine;
using Photon.Pun;
using System.Collections;
using TMPro;

public class PlayerConnector : MonoBehaviourPunCallbacks
{
    [Header("Settings")]
    public Transform connectionSlot; 
    public float connectRange = 15f;
    
    [Header("UI")]
    public GameObject connectedUI;
    public TextMeshProUGUI connectedDurationText;
    
    private Camera fpsCamera;
    private PlayerMovementController movementController;
    private PlayerSkillDetails[] skillDetails;

    private bool isAttached = false;
    private Transform attachTarget; 
    private Coroutine connectionCoroutine;
    private PhotonView connectedPlayerView;

    void Start()
    {
        if (photonView.IsMine)
        {
            fpsCamera = GetComponentInChildren<Camera>(); 
        }
        movementController = GetComponent<PlayerMovementController>();
        skillDetails = GetComponentsInChildren<PlayerSkillDetails>(true); 
    }

    void Update()
    {
        if (photonView.IsMine && isAttached && attachTarget != null)
        {
            transform.position = attachTarget.position;
        }
    }

    public bool TryConnect(float duration)
    {
        if (fpsCamera == null) return false;

        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, connectRange))
        {
            PhotonView targetView = hit.collider.GetComponentInParent<PhotonView>();
            if (targetView != null && !targetView.IsMine)
            {
                PlayerHealth targetHealth = targetView.GetComponent<PlayerHealth>();
                if (targetHealth != null && targetHealth.IsDowned)
                {
                    return false; // Don't connect to downed players
                }

                connectedPlayerView = targetView;
                targetView.RPC("GetConnected", RpcTarget.All, photonView.ViewID, duration);
                return true; // Success
            }
        }
        return false; // Failed to connect
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
            connectedPlayerView.RPC("ForceDetach", RpcTarget.All);
            connectedPlayerView = null;
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
} 