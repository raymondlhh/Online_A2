using UnityEngine;
using Photon.Pun;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PhotonView))]
public class Victim : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject dangerMark;
    [SerializeField] private GameObject safeMark;

    private bool isSaved = false;
    private bool isConnected = false;
    private Coroutine connectionCoroutine;
    private Transform playerConnectionSlot;
    private PhotonView photonView;
    private int connectedPlayerViewID = 0;
    private Rigidbody rb;

    public bool IsSaved()
    {
        return isSaved;
    }

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
    }

    void Start()
    {
        // Victim should be affected by gravity by default.
        // It will become kinematic only when connected to a player.
        rb.isKinematic = false;

        // Set initial state for the marks.
        if (dangerMark != null)
        {
            dangerMark.SetActive(true);
        }
        if (safeMark != null)
        {
            safeMark.SetActive(false);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SaveZone") && !isSaved)
        {
            // Tell the Master Client a victim has entered the save zone.
            photonView.RPC(nameof(RPC_VictimEnteredSaveZone), RpcTarget.MasterClient);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SaveZone") && isSaved)
        {
            // Tell the Master Client a victim has left the save zone.
            photonView.RPC(nameof(RPC_VictimLeftSaveZone), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    private void RPC_VictimEnteredSaveZone()
    {
        if (isSaved) return;
        
        // This runs on the Master Client to update the authoritative count.
        GameManager.Instance.UpdateVictimsSavedCount(1);

        // Broadcast to all clients that this victim is saved and should detach.
        photonView.RPC(nameof(RPC_SetSavedState), RpcTarget.All, true);
    }

    [PunRPC]
    private void RPC_VictimLeftSaveZone()
    {
        if (!isSaved) return;

        // This runs on the Master Client.
        GameManager.Instance.UpdateVictimsSavedCount(-1);

        // Broadcast to all clients that this victim is no longer saved.
        photonView.RPC(nameof(RPC_SetSavedState), RpcTarget.All, false);
    }

    [PunRPC]
    private void RPC_SetSavedState(bool state)
    {
        isSaved = state;

        // Toggle the danger/safe marks based on the saved state.
        if (dangerMark != null)
        {
            dangerMark.SetActive(!isSaved);
        }
        if (safeMark != null)
        {
            safeMark.SetActive(isSaved);
        }

        // The logic for auto-detaching when entering the save zone has been removed.
        // The victim will now remain connected.
    }

    [PunRPC]
    public void GetConnectedToPlayer(int connectorViewID, float duration)
    {
        // The check preventing connection to a saved victim has been removed.
        // Players can now connect to victims inside the save zone.
        this.connectedPlayerViewID = connectorViewID;

        PhotonView connectorView = PhotonView.Find(connectorViewID);
        if (connectorView == null) return;
        
        PlayerConnector connector = connectorView.GetComponent<PlayerConnector>();
        if (connector == null || connector.connectionSlot == null) return;

        if (connectionCoroutine != null) StopCoroutine(connectionCoroutine);
        connectionCoroutine = StartCoroutine(VictimConnectionLifetime(connector.connectionSlot, duration));
    }

    [PunRPC]
    public void ForceDetachFromPlayer()
    {
        ForceDetachFromPlayer_Internal();
    }

    private void ForceDetachFromPlayer_Internal()
    {
        if (isConnected)
        {
            if (connectionCoroutine != null)
            {
                StopCoroutine(connectionCoroutine);
            }
            DetachFromPlayer();
        }
    }

    private void DetachFromPlayer()
    {
        isConnected = false;
        playerConnectionSlot = null;
        connectionCoroutine = null;
        connectedPlayerViewID = 0;

        // Victim is no longer controlled by a player, let physics take over.
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    private IEnumerator VictimConnectionLifetime(Transform targetSlot, float duration)
    {
        isConnected = true;
        playerConnectionSlot = targetSlot;

        // When connected, the victim should not be affected by physics.
        // Its movement is controlled directly.
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        float remainingDuration = duration;
        while (remainingDuration > 0f && isConnected)
        {
            if (playerConnectionSlot != null)
            {
                transform.position = playerConnectionSlot.position;
            }
            yield return new WaitForSeconds(0.1f);
            remainingDuration -= 0.1f;
        }

        DetachFromPlayer();
    }
} 