using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MobileFPSGameManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    [SerializeField] Transform spawner;



    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (spawner != null)
            {
                PhotonNetwork.Instantiate(playerPrefab.name, spawner.position, Quaternion.identity);
            }
            else
            {
                Debug.Log("Place Player");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
