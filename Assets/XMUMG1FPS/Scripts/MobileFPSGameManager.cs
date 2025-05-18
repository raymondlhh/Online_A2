using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MobileFPSGameManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if(playerPrefab != null)
            {
                PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                Debug.LogWarning("PlayerPrefab or SpawnPoint not assigned.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
