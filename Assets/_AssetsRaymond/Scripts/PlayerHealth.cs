using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPunCallbacks
{
    [Header("Health Related Stuff")]
    public float startHealth = 100;
    
    [Header("UI Elements")]
    [SerializeField] private Image TPHealthBar;    // Third person healthbar
    [SerializeField] private Image FPHealthBar;    // First person healthbar

    private float health;
    private Animator animator;
    private bool isLocalPlayer;

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
        UpdateHealthBars();
    }

    [PunRPC]
    public void TakeDamage(float _damage, PhotonMessageInfo info)
    {
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

    [PunRPC]
    public void RegainHealth()
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

    void Die()
    {
        if (isLocalPlayer)
        {
            if (animator != null)
                animator.SetBool("IsDead", true);
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        GameObject reSpawnText = GameObject.Find("RespawnText");

        float respawnTime = 8.0f;
        while (respawnTime > 0.0f)
        {
            yield return new WaitForSeconds(1.0f);
            respawnTime -= 1.0f;

            var movement = GetComponent<PlayerMovementController>();
            if (movement != null) movement.enabled = false;

            if (reSpawnText != null)
                reSpawnText.GetComponent<Text>().text = "You are killed. Respawning at: " + respawnTime.ToString(".00");
        }

        if (animator != null)
            animator.SetBool("IsDead", false);

        if (reSpawnText != null)
            reSpawnText.GetComponent<Text>().text = "";

        int randomPoint = Random.Range(-20, 20);
        transform.position = new Vector3(randomPoint, 0, randomPoint);

        var movement2 = GetComponent<PlayerMovementController>();
        if (movement2 != null) movement2.enabled = true;

        RegainHealth();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
