using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PlayerShoot : MonoBehaviourPunCallbacks
{
    public Camera FPS_Camera;
    public GameObject hitEffectPrefab;

    [Header("Animator Settings")]
    public Animator FPAnimator;
    public Animator TPAnimator; 
    

    [Header("Weapon Settings")]
    public float damage = 10f;
    public int maxAmmo = 30;
    public int currentAmmo;
    public float reloadTime = 2f;
    public bool isReloading = false;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;
    public Text ammoText;

    // Add weapon references
    [Header("Weapons - First Person")]
    public GameObject Rifle_FP;
    public GameObject Shotgun_FP;
    public GameObject SMG_FP;

    [Header("Weapons - Third Person")]
    public GameObject Rifle_TP;
    public GameObject Shotgun_TP;
    public GameObject SMG_TP;

    private int currentWeaponIndex = 0; // 0: Rifle, 1: Shotgun, 2: SMG
    private GameObject[] weaponsFP;
    private GameObject[] weaponsTP;

    private PlayerHealth playerHealth;

    private Weapon currentWeaponData;

    private bool isChanging = false;  // Add this field
    public float weaponSwitchTime = 0.5f;  // Add this to control switch animation duration

    // Start is called before the first frame update
    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        playerHealth = GetComponent<PlayerHealth>();

        // Initialize weapons array
        weaponsFP = new GameObject[] { Rifle_FP, Shotgun_FP, SMG_FP };
        weaponsTP = new GameObject[] { Rifle_TP, Shotgun_TP, SMG_TP };
        SelectWeapon(currentWeaponIndex);

        if (photonView.IsMine)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["WeaponIndex"] = currentWeaponIndex;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        if (photonView.Owner.CustomProperties.ContainsKey("WeaponIndex"))
        {
            currentWeaponIndex = (int)photonView.Owner.CustomProperties["WeaponIndex"];
            ApplyWeaponVisual(currentWeaponIndex);
        }
        else
        {
            ApplyWeaponVisual(currentWeaponIndex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        // Weapon switching by key
        if (Input.GetKeyDown(KeyCode.Alpha1)) { SetWeaponIndex(0); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { SetWeaponIndex(1); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { SetWeaponIndex(2); }

        // Mouse wheel switching
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SetWeaponIndex((currentWeaponIndex + 2) % 3); // previous
        }
        else if (scroll < 0f)
        {
            SetWeaponIndex((currentWeaponIndex + 1) % 3); // next
        }

        // Handle continuous shooting with left mouse button held down
        if (Input.GetMouseButton(0) && !isReloading)
        {
            if (currentAmmo > 0 && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
            else if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
            }
            // Set IsGunAttacking on TPAnimator while button is held
            if (TPAnimator != null)
            {
                TPAnimator.SetBool("IsGunAttacking", true);
            }
        }
        else
        {
            // Set IsGunAttacking to false when button is released
            if (TPAnimator != null)
            {
                TPAnimator.SetBool("IsGunAttacking", false);
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
            // Debug.Log(_hit.collider.gameObject.name);

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
        if (FPAnimator != null)
            FPAnimator.SetBool("IsReloading", true);

        // If you want to sync to PlayerMovementController's animator too:
        var movement = GetComponent<PlayerMovementController>();
        if (movement != null)
        {
            var movementAnimatorField = typeof(PlayerMovementController).GetField("animator", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (movementAnimatorField != null)
            {
                var movementAnimator = movementAnimatorField.GetValue(movement) as Animator;
                if (movementAnimator != null)
                    movementAnimator.SetBool("IsReloading", true);
            }
        }

        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        isReloading = false;

        if (FPAnimator != null)
            FPAnimator.SetBool("IsReloading", false);

        // Fix: Access PlayerMovementController's animator via reflection, as in reload start
        if (movement != null)
        {
            var movementAnimatorField = typeof(PlayerMovementController).GetField("animator", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (movementAnimatorField != null)
            {
                var movementAnimator = movementAnimatorField.GetValue(movement) as Animator;
                if (movementAnimator != null)
                    movementAnimator.SetBool("IsReloading", false);
            }
        }

        if (photonView.IsMine)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["WeaponIndex"] = currentWeaponIndex;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }
    }

    [PunRPC]
    public void CreateHitEffect(Vector3 position)
    {
        GameObject hitEffectGameobject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameobject, 0.5f);
    }

    void SelectWeapon(int index)
    {
        for (int i = 0; i < weaponsFP.Length; i++)
        {
            if (weaponsFP[i] != null)
                weaponsFP[i].SetActive(i == index);
            if (weaponsTP[i] != null)
                weaponsTP[i].SetActive(i == index);
        }

        if (photonView.IsMine)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["WeaponIndex"] = index;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == photonView.Owner && changedProps.ContainsKey("WeaponIndex"))
        {
            int newIndex = (int)changedProps["WeaponIndex"];
            currentWeaponIndex = newIndex;
            ApplyWeaponVisual(currentWeaponIndex);
        }
    }

    // Modify SetWeaponIndex to handle the animation
    void SetWeaponIndex(int index)
    {
        if (isChanging)
        {
            Debug.Log($"Cannot switch weapon - already changing weapons");
            return;
        }

        Debug.Log($"Starting weapon switch to index: {index}");
        currentWeaponIndex = index;
        StartCoroutine(SwitchWeaponAnimation(index));

        if (photonView.IsMine)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["WeaponIndex"] = index;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }

    // Add this new coroutine
    IEnumerator SwitchWeaponAnimation(int index)
    {
        try
        {
            Debug.Log("Starting weapon switch animation");
            isChanging = true;
            if (FPAnimator != null)
            {
                FPAnimator.SetBool("IsChanging", true);
            }

            yield return new WaitForSeconds(weaponSwitchTime);

            Debug.Log("Applying weapon visual changes");
            ApplyWeaponVisual(index);
        }
        finally
        {
            // Ensure these are always set back to false, even if an error occurs
            Debug.Log("Finishing weapon switch animation");
            if (FPAnimator != null)
            {
                FPAnimator.SetBool("IsChanging", false);
            }
            isChanging = false;
        }
    }

    // Modify ApplyWeaponVisual to not instantly switch weapons
    void ApplyWeaponVisual(int index)
    {
        // Always update visuals for the local player after switching
        for (int i = 0; i < weaponsFP.Length; i++)
        {
            if (weaponsFP[i] != null)
                weaponsFP[i].SetActive(i == index);
            if (weaponsTP[i] != null)
                weaponsTP[i].SetActive(i == index);
        }

        // Update weapon data
        if (weaponsFP[index] != null)
        {
            var currentWeaponData = weaponsFP[index].GetComponent<Weapon>();
            if (currentWeaponData != null)
            {
                fireRate = currentWeaponData.fireRate;
                damage = currentWeaponData.damage;
                maxAmmo = currentWeaponData.maxAmmo;
                UpdateAmmoUI();
                Debug.Log($"Switched weapon: fireRate={fireRate}, damage={damage}, maxAmmo={maxAmmo}");
            }
        }
    }
}
