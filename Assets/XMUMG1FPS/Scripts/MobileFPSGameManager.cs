using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MobileFPSGameManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    [SerializeField] private Transform[] spawners;



    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (playerPrefab != null && spawners != null && spawners.Length > 0)
            {
                // Get sorted player list
                var players = Photon.Pun.PhotonNetwork.PlayerList;
                System.Array.Sort(players, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));
                int myIndex = System.Array.FindIndex(players, p => p.ActorNumber == Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber);

                // Clamp index to spawner count
                int spawnerIndex = Mathf.Clamp(myIndex, 0, spawners.Length - 1);

                // Spawn at the correct spawner
                Quaternion spawnRotation = Quaternion.Euler(0, 180, 0);
                PhotonNetwork.Instantiate(playerPrefab.name, spawners[spawnerIndex].position, spawnRotation);
            }
            else
            {
                Debug.Log("Assign playerPrefab and spawners in the inspector.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
