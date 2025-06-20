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

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.2f;
    private bool isGrounded;
    private bool canMultiJump = false;
    private bool isSlowFalling = false;
    private Coroutine slowFallCoroutine;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 90f;
    private float verticalRotation = 0f;
    public Camera playerCamera;
    private bool isCursorLocked = true;

    [Header("Animation Settings")]
    public Animator FPAnimator;  // First Person Animator
    public Animator TPAnimator;  // Third Person Animator

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        // Ground Check
        if (groundCheck != null)
        {
            // Casts a ray straight down from the groundCheck position.
            isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance);
        }

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

        // Update both FP and TP animators
        UpdateAnimators();

        // Handle jumping
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || canMultiJump))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Apply movement
        Vector3 movement = moveDirection * currentSpeed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        if (isSlowFalling)
        {
            // Override y-velocity to counteract gravity
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }
    }

    private void UpdateAnimators()
    {
        // Update First Person Animator
        if (FPAnimator != null)
        {
            // No longer setting IsRunning for FPAnimator
        }

        // Update Third Person Animator
        if (TPAnimator != null)
        {
            TPAnimator.SetFloat("Horizontal", horizontalInput);
            TPAnimator.SetFloat("Vertical", verticalInput);
            TPAnimator.SetBool("IsRunning", isRunning);
        }
    }

    public void ActivateHighJump(float duration)
    {
        StartCoroutine(HighJumpCoroutine(duration));
    }

    private IEnumerator HighJumpCoroutine(float duration)
    {
        canMultiJump = true;
        yield return new WaitForSeconds(duration);
        canMultiJump = false;
    }

    public void ActivateSlowFall(float duration)
    {
        if (slowFallCoroutine != null) StopCoroutine(slowFallCoroutine);
        slowFallCoroutine = StartCoroutine(SlowFallCoroutine(duration));
    }

    public void DeactivateSlowFall()
    {
        if (slowFallCoroutine != null)
        {
            StopCoroutine(slowFallCoroutine);
            slowFallCoroutine = null;
        }
        isSlowFalling = false;
        rb.useGravity = true; // Re-enable gravity
    }

    private IEnumerator SlowFallCoroutine(float duration)
    {
        isSlowFalling = true;
        rb.useGravity = false;
        yield return new WaitForSeconds(duration);
        DeactivateSlowFall();
    }
}
