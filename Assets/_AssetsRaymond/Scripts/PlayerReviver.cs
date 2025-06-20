using UnityEngine;
using Photon.Pun;

public class PlayerReviver : MonoBehaviour
{
    private PhotonView photonView;
    private PlayerHealth revivablePlayer;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        // Only the local player can initiate a revive.
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            TryRevive();
        }
    }

    private void TryRevive()
    {
        if (revivablePlayer != null)
        {
            // We have a target. Call the Revive RPC on them.
            revivablePlayer.photonView.RPC("Revive", RpcTarget.All);

            // Clear the target to prevent accidental multi-revives.
            revivablePlayer = null;
        }
    }

    // This method is called by ReviveTrigger when we enter/exit a revive zone.
    public void SetRevivablePlayer(PlayerHealth target)
    {
        revivablePlayer = target;
    }
} 