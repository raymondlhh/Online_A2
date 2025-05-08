using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shooting : MonoBehaviour
{
    [SerializeField]
    Camera fpsCamera;

    public float fireRate = 0.3f;
    float fireTimer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        if (Input.GetButton("Fire1") && fireTimer > fireRate) //represent left click mouse in
        {
            fireTimer = 0.0f;

            RaycastHit _hit; // the target
            Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)); // is the center of the screen defined by vertical/half vertial

            if(Physics.Raycast(ray, out _hit, 100)) // denotes te ray line from the center of screen, output target, max range of 100
            { 
                Debug.Log(_hit.collider.gameObject.name); //denote the testing to print out the target name if we shoot something

                if(_hit.collider.gameObject.CompareTag("Player") && !_hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
                {
                    _hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 10f);
                }
            }
        }
    }
}
