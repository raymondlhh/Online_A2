using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerVisibilityController : MonoBehaviourPunCallbacks
{
    public GameObject tpViewObject;
    public GameObject tpPlayerUI;
    private Coroutine visibilityCoroutine;

    public void SetPlayerVisible(bool isVisible)
    {
        if (tpViewObject != null)
        {
            tpViewObject.SetActive(isVisible);
        }
        if (tpPlayerUI != null)
        {
            tpPlayerUI.SetActive(isVisible);
        }
    }

    public void ActivateGhostCloak(float duration)
    {
        if (visibilityCoroutine != null) StopCoroutine(visibilityCoroutine);
        visibilityCoroutine = StartCoroutine(GhostCloakCoroutine(duration));
    }

    [PunRPC]
    private void TeleportRPC(Vector3 position)
    {
        transform.position = position;
    }

    public void Teleport(Vector3 position)
    {
        // Use an RPC to ensure all clients see the teleportation.
        photonView.RPC("TeleportRPC", RpcTarget.All, position);
    }

    private IEnumerator GhostCloakCoroutine(float duration)
    {
        photonView.RPC("SyncVisibility", RpcTarget.All, false);
        yield return new WaitForSeconds(duration);
        photonView.RPC("SyncVisibility", RpcTarget.All, true);
        visibilityCoroutine = null;
    }

    [PunRPC]
    private void SyncVisibility(bool isVisible)
    {
        SetPlayerVisible(isVisible);
    }
} 