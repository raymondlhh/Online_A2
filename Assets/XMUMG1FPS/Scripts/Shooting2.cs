using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Shooting2 : MonoBehaviourPunCallbacks
{
    public Camera FPS_Camera;
    public GameObject hitEffectPrefab;

    [Header("Weapon Settings")]
    public float damage = 10f;
    public int maxAmmo = 30;
    public int currentAmmo;
    public float reloadTime = 2f;
    public bool isReloading = false;
    public Text ammoText;

    [Header("Health Related Stuff")]
    public float startHealth = 100;
    private float health;
    public Image healthBar;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        health = startHealth;
        healthBar.fillAmount = health / startHealth;
        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        // Handle shooting with left mouse button
        if (Input.GetMouseButtonDown(0) && !isReloading)
        {
            if (currentAmmo > 0)
            {
                Fire();
            }
            else
            {
                StartCoroutine(Reload());
            }
        }

        // Handle reloading with R key
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    public void Fire()
    {
        if (currentAmmo <= 0) return;

        currentAmmo--;
        UpdateAmmoUI();

        RaycastHit _hit;
        Ray ray = FPS_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if(Physics.Raycast(ray, out _hit, 100))
        {
            Debug.Log(_hit.collider.gameObject.name);

            photonView.RPC("CreateHitEffect", RpcTarget.All, _hit.point);

            if(_hit.collider.gameObject.CompareTag("Player") && !_hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                _hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, damage);
            }
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        isReloading = false;
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }
    }

    [PunRPC]
    public void TakeDamage(float _damage, PhotonMessageInfo info)
    {
        health -= _damage;
        Debug.Log(health);

        healthBar.fillAmount = health / startHealth;

        if(health <= 0f)
        {
            Die();
            Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner.NickName);
        }
    }

    [PunRPC]
    public void CreateHitEffect(Vector3 position)
    {
        GameObject hitEffectGameobject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameobject, 0.5f);
    }

    void Die()
    {
        if(photonView.IsMine)
        {
            animator.SetBool("IsDead", true);
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        GameObject reSpawnText = GameObject.Find("RespawnText");

        float respawnTime = 8.0f; //8 sec
        while(respawnTime > 0.0f)
        {
            yield return new WaitForSeconds(1.0f); //wait for 1 sec
            respawnTime -= 1.0f; //decrease respawn time to 2 sec

            transform.GetComponent<PlayerMovementController>().enabled = false;
            reSpawnText.GetComponent<Text>().text = "You are killed. Respawining at: " + respawnTime.ToString(".00");
            // will show when we are killed and show the respawn time in seconds
        }

        animator.SetBool("IsDead", false); //when respawn, we should set IsDead animation to false

        reSpawnText.GetComponent<Text>().text = ""; // respawn text empty so it does not block the view

        int randomPoint = Random.Range(-20, 20); //random position
        transform.position = new Vector3(randomPoint, 0, randomPoint);
        transform.GetComponent<PlayerMovementController>().enabled = true;

        photonView.RPC("RegainHealth", RpcTarget.AllBuffered); //call this RPC from the respawned player
        // Allbuffered = players who hoined later should have the latest update
    }

    [PunRPC]
    public void RegainHealth() // to let other players know that this player regained health be using RPC call
    {
        health = startHealth;
        healthBar.fillAmount = health / startHealth; // updated health bar
    }
}
