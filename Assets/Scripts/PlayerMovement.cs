using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    
    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float ascendMultiplier = 2f;
    
    [Header("Ground Check")]
    public LayerMask groundLayer;
    
    // Private variables
    private Rigidbody rb;
    private Transform cameraTransform;
    private float verticalRotation = 0f;
    private float originalMoveSpeed;
    
    // Input variables
    private float moveHorizontal;
    private float moveForward = 1f;
    
    // Ground check variables
    private bool isGrounded = true;
    private float groundCheckTimer = 0f;
    private float groundCheckDelay = 0.3f;
    private float playerHeight;
    private float raycastDistance;
    
    // Public properties for other scripts to access
    public bool IsGrounded => isGrounded;
    public float CurrentMoveSpeed => moveSpeed;
    
    void Start()
    {
        InitializeComponents();
        SetupMovementParameters();
    }
    
    void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cameraTransform = Camera.main.transform;
        
        playerHeight = GetComponent<CapsuleCollider>().height * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.2f;
        
        originalMoveSpeed = moveSpeed;
    }
    
    void SetupMovementParameters()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleInput();
        CheckGrounded();
    }
    
    void HandleInput()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = 1f;
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }
    
    void CheckGrounded()
    {
        if (!isGrounded && groundCheckTimer <= 0f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, raycastDistance, groundLayer);
        }
        else
        {
            groundCheckTimer -= Time.deltaTime;
        }
    }
    
    void FixedUpdate()
    {
        MovePlayer();
        ApplyJumpPhysics();
    }
    
    void MovePlayer()
    {
        Vector3 movement = (transform.right * moveHorizontal + transform.forward * moveForward).normalized;
        Vector3 targetVelocity = movement * moveSpeed;
        
        Vector3 velocity = rb.linearVelocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.linearVelocity = velocity;
        
        if (isGrounded && moveHorizontal == 0 && moveForward == 0)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }
    
    void Jump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }
    
    void ApplyJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (ascendMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
    
    // Public methods for other scripts to call
    public void ApplySpeedBoost(float boostAmount, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(boostAmount, duration));
    }
    
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    
    public void ResetMoveSpeed()
    {
        moveSpeed = originalMoveSpeed;
    }
    
    // Coroutine to handle temporary speed boost
    IEnumerator SpeedBoostCoroutine(float boostAmount, float duration)
    {
        moveSpeed += boostAmount;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalMoveSpeed;
    }
}