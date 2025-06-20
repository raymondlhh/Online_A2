using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PlayerHealth : MonoBehaviourPunCallbacks
{
    [Header("Health Related Stuff")]
    public float startHealth = 100;
    
    [Header("UI Elements")]
    [SerializeField] private Image TPHealthBar;    // Third person healthbar
    [SerializeField] private Image FPHealthBar;    // First person healthbar
    [SerializeField] private GameObject deadPanel;
    [SerializeField] private GameObject gameOverPanel;

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

    [PunRPC]
    public void Revive()
    {
        IsDowned = false;
        if (isLocalPlayer)
        {
            if (deadPanel != null) deadPanel.SetActive(false);
            
            movementController.CanMove = true;
            movementController.CanLook = true;
            playerShoot.enabled = true;
            foreach (var skill in skillDetails)
            {
                skill.enabled = true;
            }
        }
        RegainHealth();
    }

    void Die()
    {
        if (isLocalPlayer)
        {
            // Tell all clients that this player is now in the downed state.
            photonView.RPC("GoDown", RpcTarget.All);
        }
    }

    [PunRPC]
    private void GoDown()
    {
        IsDowned = true;

        if (isLocalPlayer)
        {
            if (animator != null)
                animator.SetBool("IsDead", true);
            
            // This player is the one who died. Run the local "downed" sequence.
            StartCoroutine(EnterDownedState());
        }

        // After the state is updated, the Master Client checks if the game is over.
        if (PhotonNetwork.IsMasterClient)
        {
            CheckForGameOver();
        }
    }

    private void CheckForGameOver()
    {
        // This check only runs on the Master Client for authority.
        if (!PhotonNetwork.IsMasterClient) return;

        PlayerHealth[] allPlayers = FindObjectsOfType<PlayerHealth>();

        // If a player is still loading in, the game isn't over.
        if (allPlayers.Length < PhotonNetwork.CurrentRoom.PlayerCount)
        {
            return;
        }

        foreach (var player in allPlayers)
        {
            // If we find even one player who isn't downed, the game continues.
            if (!player.IsDowned)
            {
                return;
            }
        }

        // If the loop completes, all players are downed. Game Over.
        // Use a room property to signal game over to all clients.
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["GameOver"] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
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

        // Deactivate the revive area associated with this player
        if (photonView.IsMine)
        {
            GameObject reviveArea = GameManager.Instance.reviveAreas[photonView.Owner.ActorNumber - 1];
            if (reviveArea != null)
            {
                reviveArea.SetActive(false);
            }
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

        // 3. Teleport to designated spawner & activate the corresponding ReviveArea
        if (GameManager.Instance != null)
        {
            Transform spawnPoint = GameManager.Instance.playerSpawners[photonView.Owner.ActorNumber - 1];
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            GameObject reviveArea = GameManager.Instance.reviveAreas[photonView.Owner.ActorNumber - 1];
            if (reviveArea != null)
            {
                reviveArea.SetActive(true);
                ReviveTrigger reviveTrigger = reviveArea.GetComponent<ReviveTrigger>();
                if (reviveTrigger != null)
                {
                    reviveTrigger.downedPlayer = this;
                }
            }
        }

        // 4. Show Dead Panel UI, but only if the game isn't already over.
        if (gameOverPanel != null && !gameOverPanel.activeInHierarchy)
        {
            if (deadPanel != null) deadPanel.SetActive(true);
        }

        yield return null; // End the coroutine, we now wait for a Revive RPC
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
