using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerReviver : MonoBehaviour
{
    [Header("Revive Settings")]
    [SerializeField] private float reviveRange = 3f;
    [SerializeField] private LayerMask playerLayerMask = -1;
    [SerializeField] private KeyCode reviveKey = KeyCode.F;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject revivePromptUI;
    [SerializeField] private TextMeshProUGUI revivePromptText;
    
    private PhotonView photonView;
    private Camera playerCamera;
    private PlayerHealth revivablePlayer;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        
        // Get the player camera
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        // Only the local player can initiate a revive
        if (!photonView.IsMine) return;

        // Check for revivable players using raycast
        CheckForRevivablePlayers();

        // Handle revive input
        if (Input.GetKeyDown(reviveKey) && revivablePlayer != null)
        {
            TryRevive();
        }

        // Show/hide UI
        if (revivePromptUI != null)
        {
            revivePromptUI.SetActive(revivablePlayer != null);
        }
    }

    void CheckForRevivablePlayers()
    {
        revivablePlayer = null;
        
        if (playerCamera == null) return;

        // Cast a ray from the camera
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, reviveRange, playerLayerMask))
        {
            // Check if the hit object is a player
            PlayerHealth hitPlayer = hit.collider.GetComponent<PlayerHealth>();
            if (hitPlayer != null && hitPlayer.IsDowned && !hitPlayer.photonView.IsMine)
            {
                revivablePlayer = hitPlayer;
                
                // Update UI text
                if (revivePromptText != null)
                {
                    revivePromptText.text = $"Press F to Revive {hitPlayer.photonView.Owner.NickName}";
                }
            }
        }
    }

    void TryRevive()
    {
        if (revivablePlayer != null)
        {
            // Call the Revive RPC on the downed player
            revivablePlayer.photonView.RPC("Revive", RpcTarget.All);
            
            // Clear the target to prevent accidental multi-revives
            revivablePlayer = null;
            
            Debug.Log($"Revived player {revivablePlayer.photonView.Owner.NickName}");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw revive range in scene view
        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Vector3 rayStart = playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
            Gizmos.DrawRay(rayStart, playerCamera.transform.forward * reviveRange);
        }
    }
} 