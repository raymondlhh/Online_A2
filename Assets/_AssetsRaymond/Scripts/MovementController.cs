using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MovementController : MonoBehaviour
{
    [SerializeField]
    private float speed = 8f;

    [SerializeField]
    private float lookSensitivity = 3f;

    [SerializeField]
    GameObject fpsCamera;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float CameraUpAndDownRotation = 0f;
    private float CurrentCameraUpAndDownRotation = 0f;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GetComponent<PhotonView>().IsMine)
            return;

        // calculate movement velocity as a 3D vector
        float _xMovement = Input.GetAxis("Horizontal");
        float _zMovement = Input.GetAxis("Vertical");

        Vector3 _movementHorizontal = transform.right * _xMovement;
        Vector3 _movementVertical = transform.forward * _zMovement;

        //final movement velocity
        Vector3 _movementVelocity = (_movementHorizontal + _movementVertical).normalized * speed;

        //apply movement
        Move(_movementVelocity);

        //calculate rotation as a 3D vector for turning around
        float _yRotation = Input.GetAxis("Mouse X");
        Vector3 _rotationVector = new Vector3(0, _yRotation, 0) * lookSensitivity;

        //apply rotation
        Rotate(_rotationVector);

        //calculate look up and down camera rotation
        float _cameraUpAndDownRotation = Input.GetAxis("Mouse Y") * lookSensitivity;

        //Apply rotation 
        RotateCamera(_cameraUpAndDownRotation);
    }

    //run per physics iteration
    private void FixedUpdate()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }

        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));

        if(fpsCamera != null)
        {
            CurrentCameraUpAndDownRotation -= CameraUpAndDownRotation;
            CurrentCameraUpAndDownRotation = Mathf.Clamp(CurrentCameraUpAndDownRotation, -85, 85);

            fpsCamera.transform.localEulerAngles = new Vector3(CurrentCameraUpAndDownRotation, 0f, 0f);
        }
    }

    private void Move(Vector3 movementVelocity)
    {
        velocity = movementVelocity;
    }

    void Rotate(Vector3 rotationVector)
    {
        rotation = rotationVector;
    }

    void RotateCamera(float cameraUpAndDownRotation)
    {
        CameraUpAndDownRotation = cameraUpAndDownRotation;
    }
}
