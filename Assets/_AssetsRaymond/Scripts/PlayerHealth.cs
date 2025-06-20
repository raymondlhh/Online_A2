using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class PlayerHealth : MonoBehaviourPunCallbacks
{
    [Header("Health Related Stuff")]
    public float startHealth = 100;
    
    [Header("UI Elements")]
    [SerializeField] private Image TPHealthBar;    // Third person healthbar
    [SerializeField] private Image FPHealthBar;    // First person healthbar
    [SerializeField] private GameObject deadPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Revive System")]
    [SerializeField] private GameObject revivePromptUI;
    [SerializeField] private TextMeshProUGUI revivePromptText;
    [SerializeField] private string deadMarkPrefabName = "DeadMark";

    private GameObject instantiatedDeadMark;
    private float health;
    private Animator animator;
    private bool isLocalPlayer;
    private bool isInvulnerable = false;
    public bool IsDowned { get; private set; } = false;

    // Component references
    private PlayerMovementController movementController;
    private PlayerShoot playerShoot;
    private PlayerSkillDetails[] skillDetails;

    void Awake()
    {
        isLocalPlayer = photonView.IsMine;
        
        // Disable FP UI elements for non-local players
        if (!isLocalPlayer && FPHealthBar != null)
        {
            Transform fpUI = FPHealthBar.transform.root.Find("FP_PlayerUI");
            if (fpUI != null)
            {
                fpUI.gameObject.SetActive(false);
            }
        }
    }

    void Start()
    {
        health = startHealth;
        animator = GetComponent<Animator>();
        movementController = GetComponent<PlayerMovementController>();
        playerShoot = GetComponent<PlayerShoot>();
        skillDetails = GetComponentsInChildren<PlayerSkillDetails>();
        UpdateHealthBars();
    }

    [PunRPC]
    public void TakeDamage(float _damage, PhotonMessageInfo info)
    {
        if (isInvulnerable)
        {
            Debug.Log($"Player {photonView.Owner.NickName} is invulnerable and took no damage.");
            return;
        }

        health -= _damage;
        health = Mathf.Max(0, health); // Prevent negative health
        Debug.Log($"Player {photonView.Owner.NickName} took damage. Health: {health}");

        UpdateHealthBars();

        if (health <= 0f)
        {
            Die();
            Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner.NickName);
        }
    }

    [PunRPC]
    void SyncHealth(float newHealth)
    {
        health = newHealth;
        UpdateHealthBars();
    }

    public void RegainHealth()
    {
        photonView.RPC("RegainHealthRPC", RpcTarget.All);
    }

    [PunRPC]
    public void RegainHealthRPC()
    {
        health = startHealth;
        UpdateHealthBars();
    }

    private void UpdateHealthBars()
    {
        float healthPercentage = health / startHealth;
        
        // Update Third Person healthbar for everyone
        if (TPHealthBar != null)
        {
            TPHealthBar.fillAmount = healthPercentage;
            Debug.Log($"Updating TP healthbar for {photonView.Owner.NickName}: {healthPercentage}");
        }
            
        // Update First Person healthbar only for local player
        if (isLocalPlayer && FPHealthBar != null)
        {
            FPHealthBar.fillAmount = healthPercentage;
            Debug.Log($"Updating FP healthbar: {healthPercentage}");
        }
    }

    public void ActivateBloodLock(float duration)
    {
        if (photonView.IsMine)
        {
            StartCoroutine(BloodLockCoroutine(duration));
        }
    }

    private IEnumerator BloodLockCoroutine(float duration)
    {
        photonView.RPC("SetInvulnerable", RpcTarget.All, true);
        yield return new WaitForSeconds(duration);
        photonView.RPC("SetInvulnerable", RpcTarget.All, false);
    }

    [PunRPC]
    private void SetInvulnerable(bool state)
    {
        isInvulnerable = state;
        Debug.Log($"Player {photonView.Owner.NickName} invulnerability set to: {state}");
    }

    void Die()
    {
        if (isLocalPlayer)
        {
            // Tell all clients that this player is now dead.
            photonView.RPC("GoDown", RpcTarget.All);
        }
    }

    [PunRPC]
    private void GoDown()
    {
        IsDowned = true;

        if (animator != null)
            animator.SetBool("IsDead", true);

        if (isLocalPlayer)
        {
            // Set custom property to dead
            var props = new ExitGames.Client.Photon.Hashtable();
            props["IsAlive"] = false;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            // This player is the one who died. Run the local "downed" sequence.
            StartCoroutine(EnterDownedState());
        }
    }

    private void CheckForGameOver()
    {
        // This check only runs on the Master Client for authority.
        if (!PhotonNetwork.IsMasterClient) return;

        // Check if all players are dead 
        bool allPlayersDead = true;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            object isAlive;
            if (player.CustomProperties.TryGetValue("IsAlive", out isAlive))
            {
                if ((bool)isAlive)
                {
                    allPlayersDead = false;
                    break;
                }
            }
            else
            {
                // Property not set yet, assume they are alive.
                allPlayersDead = false;
                break;
            }
        }

        // If all players are dead, trigger game over
        if (allPlayersDead)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["GameOver"] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // When a player's "IsAlive" status changes, check for game over.
        if (PhotonNetwork.IsMasterClient && changedProps.ContainsKey("IsAlive"))
        {
            CheckForGameOver();
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("GameOver"))
        {
            if (isLocalPlayer)
            {
                TriggerGameOver();
            }
        }
    }

    private void TriggerGameOver()
    {
        // This is called on the local client to show their own Game Over screen.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Also hide the dead panel if it's active
        if (deadPanel != null)
        {
            deadPanel.SetActive(false);
        }

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator EnterDownedState()
    {
        // 1. Disable controls
        movementController.CanMove = false;
        movementController.CanLook = false;
        playerShoot.enabled = false;
        foreach(var skill in skillDetails)
        {
            skill.enabled = false;
        }

        // 2. Teleport to spawner and stay there
        if (GameManager.Instance != null)
        {
            Transform spawnPoint = GameManager.Instance.playerSpawners[photonView.Owner.ActorNumber - 1];
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        // 3. Spawn DeadMark at spawner location
        if (photonView.IsMine && !string.IsNullOrEmpty(deadMarkPrefabName))
        {
            // Instantiate the dead mark over the network at the spawner position
            instantiatedDeadMark = PhotonNetwork.Instantiate(deadMarkPrefabName, transform.position + Vector3.up, transform.rotation);
            // Parent it to the player so it moves with them
            if (instantiatedDeadMark != null)
            {
                instantiatedDeadMark.transform.SetParent(this.transform);
            }
        }

        // 4. Show Dead Panel UI, but only if the game isn't already over.
        if (gameOverPanel != null && !gameOverPanel.activeInHierarchy)
        {
            if (deadPanel != null) deadPanel.SetActive(true);
        }

        // 5. Player stays at spawner waiting for revive
        yield return null;
    }

    [PunRPC]
    public void Revive()
    {
        IsDowned = false;

        if (animator != null)
        {
            animator.SetBool("IsDead", false);
        }

        if (photonView.IsMine)
        {
            if (instantiatedDeadMark != null)
            {
                PhotonNetwork.Destroy(instantiatedDeadMark);
            }
        }
        
        if (isLocalPlayer)
        {
            // Set custom property to alive
            var props = new ExitGames.Client.Photon.Hashtable();
            props["IsAlive"] = true;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            // Hide dead panel
            if (deadPanel != null) deadPanel.SetActive(false);
            
            // Re-enable controls
            movementController.CanMove = true;
            movementController.CanLook = true;
            playerShoot.enabled = true;
            foreach (var skill in skillDetails)
            {
                skill.enabled = true;
            }
        }
        
        // Restore health
        RegainHealth();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && transform.position.y < -30f)
        {
            if (health > 0)
            {
                health = 0;
                UpdateHealthBars();
                Die();
            }
        }
    }
}
