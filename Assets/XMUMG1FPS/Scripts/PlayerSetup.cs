using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public GameObject[] FPS_Hands_ChildGameobjects;
    public GameObject[] Soldier_ChildGameobjects;

    // Start is called before the first frame update
    void Start()
    {
        if(photonView.IsMine)
        {
            // Activate soldier, deactivate FPS Hands
            foreach (GameObject gameObject in FPS_Hands_ChildGameobjects)
            {
                gameObject.SetActive(true);
            }
            foreach (GameObject gameObject in Soldier_ChildGameobjects)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            // Activate soldier, deactivate FPS Hands
            foreach (GameObject gameObject in FPS_Hands_ChildGameobjects)
            {
                gameObject.SetActive(false);
            }
            foreach (GameObject gameObject in Soldier_ChildGameobjects)
            {
                gameObject.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
