using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject playerPrefab;

    [SerializeField]
    private Transform[] playerSpawners;

    private bool isCursorLocked = true;

    // Start is called before the first frame update
    void Start()
    {
        // // Lock cursor at start
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (playerPrefab != null && playerSpawners.Length >= PhotonNetwork.CurrentRoom.PlayerCount)
            {
                int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // ActorNumber starts at 1
                playerIndex = Mathf.Clamp(playerIndex, 0, playerSpawners.Length - 1);

                Transform spawnPoint = playerSpawners[playerIndex];
                PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                Debug.Log("Place playerPrefab or assign all spawners!");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle cursor lock with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCursorLocked = !isCursorLocked;
            Cursor.lockState = isCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isCursorLocked;
        }
    }
}
