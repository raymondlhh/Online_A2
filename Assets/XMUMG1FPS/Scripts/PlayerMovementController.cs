using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float jumpForce = 5f;
    private float currentSpeed;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private bool isRunning = false;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 90f;
    private float verticalRotation = 0f;
    public Camera playerCamera;
    private bool isCursorLocked = true;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        currentSpeed = walkSpeed;

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // If no camera is assigned, try to find it
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                Debug.LogError("No camera found! Please assign a camera to the player.");
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        // Handle cursor lock toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCursorLocked = !isCursorLocked;
            Cursor.lockState = isCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isCursorLocked;
        }

        if (isCursorLocked)
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotate the player (horizontal rotation)
            transform.Rotate(Vector3.up * mouseX);

            // Rotate the camera (vertical rotation)
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        // Get movement input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Handle running with shift key
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Calculate movement direction
        moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        moveDirection = moveDirection.normalized;

        // Update animator for hands (first-person)
        animator.SetFloat("Horizontal", horizontalInput);
        animator.SetFloat("Vertical", verticalInput);
        animator.SetBool("IsRunning", isRunning);

        // Handle jumping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        // Apply movement
        Vector3 movement = moveDirection * currentSpeed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
    }
}
