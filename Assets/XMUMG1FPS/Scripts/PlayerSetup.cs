using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public GameObject[] FPS_Hands_ChildGameobjects;
    public GameObject[] Soldier_ChildGameobjects;

    // public GameObject playerUIPrefab;
    private PlayerMovementController playerMovementController;

    public Camera FPSCamera;

    private Animator animator;

    private PlayerShoot shooter;

    public TextMeshProUGUI playerNameText;

    // Start is called before the first frame update
    void Start()
    {
        shooter = GetComponent<PlayerShoot>();
        animator = GetComponent<Animator>();   
        playerMovementController = GetComponent<PlayerMovementController>();

        if(photonView.IsMine)
        {
            Debug.Log("I am the local player, showing FPS hands.");
            // Show FPS hands, hide soldier body
            foreach (GameObject go in FPS_Hands_ChildGameobjects) go.SetActive(true);
            foreach (GameObject go in Soldier_ChildGameobjects) go.SetActive(false);

            //Instantiate PlayerUI
            //GameObject playerUIGameobject = Instantiate(playerUIPrefab);
            //playerUIGameobject.transform.Find("FireButton").GetComponent<Button>().onClick.AddListener(() => shooter.Fire());

            FPSCamera.enabled = true;
            if (animator != null)
                animator.SetBool("IsSoldier", false);
        }
        else
        {
            Debug.Log("I am a remote player, showing soldier body.");
            // Hide FPS hands, show soldier body
            foreach (GameObject go in FPS_Hands_ChildGameobjects) go.SetActive(false);
            foreach (GameObject go in Soldier_ChildGameobjects) go.SetActive(true);

            playerMovementController.enabled = false;

            FPSCamera.enabled = false;
            if (animator != null)
                animator.SetBool("IsSoldier", true);
        }

        // Find the PlayerNameText in the hierarchy
        if (playerNameText == null)
        {
            Transform nameTextTransform = transform.Find("PlayerHealthAndName/Canvas/PlayerNameText");
            if (nameTextTransform == null)
            {
                Debug.LogError("PlayerNameText not found! Check the hierarchy path.");
            }
            else
            {
                playerNameText = nameTextTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        // Set the player name
        if (playerNameText != null)
        {
            playerNameText.text = photonView.Owner.NickName;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
