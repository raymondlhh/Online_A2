using UnityEngine;
using Photon.Pun;

public class ReviveTrigger : MonoBehaviour
{
    // This will be set by the downed player's PlayerHealth script.
    public PlayerHealth downedPlayer;
    public GameObject reviveUIPanel;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is a local player who is alive.
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                PlayerReviver reviver = other.GetComponent<PlayerReviver>();
                if (reviver != null)
                {
                    // Tell the reviver script which downed player is in range.
                    reviver.SetRevivablePlayer(downedPlayer);
                }
                
                // Show the "Press F to Revive" UI panel.
                if (reviveUIPanel != null) reviveUIPanel.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object that exited is the local player.
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                PlayerReviver reviver = other.GetComponent<PlayerReviver>();
                if (reviver != null)
                {
                    // Clear the revivable player when leaving the area.
                    reviver.SetRevivablePlayer(null);
                }
                
                // Hide the "Press F to Revive" UI panel.
                if (reviveUIPanel != null) reviveUIPanel.SetActive(false);
            }
        }
    }
} 