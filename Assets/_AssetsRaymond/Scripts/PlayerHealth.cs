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
    public Image TPHealthBar;    // Third person healthbar
    public Image FPHealthBar;    // First person healthbar

    private float health;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        health = startHealth;
        UpdateHealthBars();
        animator = GetComponent<Animator>();
    }

    [PunRPC]
    public void TakeDamage(float _damage, PhotonMessageInfo info)
    {
        health -= _damage;
        Debug.Log(health);

        UpdateHealthBars();

        if (health <= 0f)
        {
            Die();
            Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner.NickName);
        }
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
        
        // Update Third Person healthbar
        if (TPHealthBar != null)
            TPHealthBar.fillAmount = healthPercentage;
            
        // Update First Person healthbar
        if (FPHealthBar != null)
            FPHealthBar.fillAmount = healthPercentage;
    }

    void Die()
    {
        if (photonView.IsMine)
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

        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
