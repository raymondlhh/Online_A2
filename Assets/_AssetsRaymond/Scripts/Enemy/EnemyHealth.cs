using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float startHealth = 100f;
    [Header("UI")]
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject enemyUICanvas;

    private float health;
    private bool isDead = false;

    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private EnemyMovement enemyMovement;
    private EnemyShoot enemyShoot;
    private Collider mainCollider;
    private PhotonView photonView;
    private Camera mainCamera;

    void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyMovement = GetComponent<EnemyMovement>();
        enemyShoot = GetComponent<EnemyShoot>();
        mainCollider = GetComponent<Collider>();
        photonView = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        health = startHealth;
        mainCamera = Camera.main; // Find the main camera
        UpdateHealthBar(); // Set initial health bar state
    }

    void LateUpdate()
    {
        // Make the UI always face the player's camera
        if (enemyUICanvas != null && enemyUICanvas.activeSelf && mainCamera != null)
        {
            enemyUICanvas.transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    [PunRPC]
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        UpdateHealthBar(); // Update UI on all clients
        
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = health / startHealth;
        }
    }

    void Die()
    {
        isDead = true;
        
        // Hide the health bar canvas
        if(enemyUICanvas != null)
        {
            enemyUICanvas.SetActive(false);
        }
        
        // Trigger death animation
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }

        // Disable components
        if (enemyMovement != null) enemyMovement.enabled = false;
        if (navMeshAgent != null) navMeshAgent.enabled = false;
        if (enemyShoot != null) enemyShoot.enabled = false;
        if (mainCollider != null) mainCollider.enabled = false;

        // Start coroutine to destroy the object after a delay
        StartCoroutine(DestroyAfterAnimation());
    }

    IEnumerator DestroyAfterAnimation()
    {
        // Wait for the length of the death animation
        // You might need to adjust this time based on your actual animation length
        yield return new WaitForSeconds(3f);
        
        // Only the master client should destroy networked objects
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
