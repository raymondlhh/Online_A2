using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

[RequireComponent(typeof(PhotonView))]
public class EnemyHealth : MonoBehaviour
{
    public float startHealth = 100f;
    private float health;
    private bool isDead = false;

    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private EnemyMovement enemyMovement;
    private EnemyShoot enemyShoot;
    private Collider mainCollider;
    private PhotonView photonView;

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
    }

    [PunRPC]
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        
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
