using UnityEngine;
using Photon.Pun;
using TMPro;

/// <summary>
/// Test script to help verify the revive system is working correctly.
/// Attach this to a GameObject in the scene to see debug information.
/// </summary>
public class ReviveSystemTester : MonoBehaviourPunCallbacks
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private KeyCode testKey = KeyCode.T;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI debugText;
    
    private PlayerReviver localPlayerReviver;
    private PlayerHealth localPlayerHealth;

    void Start()
    {
        if (showDebugInfo && debugText != null)
        {
            debugText.text = "Revive System Tester Active\nPress T to test";
        }
    }

    void Update()
    {
        if (!showDebugInfo) return;

        // Find local player components
        if (localPlayerReviver == null || localPlayerHealth == null)
        {
            FindLocalPlayerComponents();
        }

        // Test key
        if (Input.GetKeyDown(testKey))
        {
            TestReviveSystem();
        }

        // Update debug info
        UpdateDebugInfo();
    }

    void FindLocalPlayerComponents()
    {
        PlayerReviver[] revivers = FindObjectsOfType<PlayerReviver>();
        PlayerHealth[] healths = FindObjectsOfType<PlayerHealth>();

        foreach (var reviver in revivers)
        {
            if (reviver.photonView != null && reviver.photonView.IsMine)
            {
                localPlayerReviver = reviver;
                break;
            }
        }

        foreach (var health in healths)
        {
            if (health.photonView != null && health.photonView.IsMine)
            {
                localPlayerHealth = health;
                break;
            }
        }
    }

    void TestReviveSystem()
    {
        if (debugText != null)
        {
            string testInfo = "=== Revive System Test ===\n";
            
            if (localPlayerReviver != null)
            {
                testInfo += "✓ PlayerReviver found\n";
            }
            else
            {
                testInfo += "✗ PlayerReviver not found\n";
            }

            if (localPlayerHealth != null)
            {
                testInfo += $"✓ PlayerHealth found (IsDowned: {localPlayerHealth.IsDowned})\n";
            }
            else
            {
                testInfo += "✗ PlayerHealth not found\n";
            }

            // Check for dead players
            PlayerHealth[] allHealths = FindObjectsOfType<PlayerHealth>();
            int deadPlayers = 0;
            foreach (var health in allHealths)
            {
                if (health.IsDowned && !health.photonView.IsMine)
                {
                    deadPlayers++;
                }
            }
            testInfo += $"Dead players in scene: {deadPlayers}\n";

            debugText.text = testInfo;
        }
    }

    void UpdateDebugInfo()
    {
        if (debugText == null) return;

        string info = "Revive System Status:\n";
        
        if (localPlayerReviver != null)
        {
            info += "✓ Reviver Active\n";
        }
        else
        {
            info += "✗ Reviver Missing\n";
        }

        if (localPlayerHealth != null)
        {
            info += $"Health: {localPlayerHealth.startHealth}\n";
            info += $"IsDowned: {localPlayerHealth.IsDowned}\n";
        }

        // Count players
        PhotonView[] playerViews = FindObjectsOfType<PhotonView>();
        int totalPlayers = 0;
        int localPlayers = 0;
        foreach (var pv in playerViews)
        {
            if (pv.CompareTag("Player"))
            {
                totalPlayers++;
                if (pv.IsMine) localPlayers++;
            }
        }
        info += $"Players: {totalPlayers} (Local: {localPlayers})\n";
        info += "Press T to test";

        debugText.text = info;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (showDebugInfo && debugText != null)
        {
            debugText.text = $"Player {newPlayer.NickName} joined the room";
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (showDebugInfo && debugText != null)
        {
            debugText.text = $"Player {otherPlayer.NickName} left the room";
        }
    }
} 