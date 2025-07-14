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
    
    [Header("Duck Settings")]
    public float duckDuration = 1f;
    public float duckCooldown = 0.5f;
    public float duckSpeedMultiplier = 1.5f; // Speed boost while ducking
    
    [Header("Ground Check")]
    public LayerMask groundLayer;
    
    // Private variables
    private Rigidbody rb;
    private Transform cameraTransform;
    private CapsuleCollider playerCollider;
    // private float verticalRotation = 0f;
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
    
    // Duck variables
    private bool isDucking = false;
    private bool canDuck = true;
    private float originalColliderHeight;
    private Vector3 originalColliderCenter;
    private float originalCameraHeight;
    private float duckColliderHeight;
    private Vector3 duckColliderCenter;
    private float duckCameraHeight;
    
    // Public properties for other scripts to access
    public bool IsGrounded => isGrounded;
    public bool IsDucking => isDucking;
    public float CurrentMoveSpeed => moveSpeed;
    
    void Start()
    {
        InitializeComponents();
        SetupMovementParameters();
        SetupDuckingParameters();
    }
    
    void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerMovement: Rigidbody component not found!");
            return;
        }
        rb.freezeRotation = true;
        
        cameraTransform = Camera.main.transform;
        if (cameraTransform == null)
        {
            Debug.LogError("PlayerMovement: Main Camera not found!");
            return;
        }
        
        playerCollider = GetComponent<CapsuleCollider>();
        if (playerCollider == null)
        {
            Debug.LogError("PlayerMovement: CapsuleCollider component not found! Please add a CapsuleCollider to your character.");
            return;
        }
        
        playerHeight = playerCollider.height * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.2f;
        
        originalMoveSpeed = moveSpeed;
        
        Debug.Log("PlayerMovement: All components initialized successfully!");
    }
    
    void SetupMovementParameters()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void SetupDuckingParameters()
    {
        // Store original collider values
        originalColliderHeight = playerCollider.height;
        originalColliderCenter = playerCollider.center;
        originalCameraHeight = cameraTransform.localPosition.y;
        
        // Calculate duck values (reduce height by half)
        duckColliderHeight = originalColliderHeight * 0.5f;
        duckColliderCenter = new Vector3(originalColliderCenter.x, originalColliderCenter.y - (originalColliderHeight - duckColliderHeight) * 0.5f, originalColliderCenter.z);
        duckCameraHeight = originalCameraHeight - (originalColliderHeight - duckColliderHeight) * 0.5f;
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
        
        if (Input.GetButtonDown("Jump") && isGrounded && !isDucking)
        {
            Jump();
        }
        
        // Duck input (Down arrow key)
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log($"Down arrow pressed! isGrounded: {isGrounded}, canDuck: {canDuck}, isDucking: {isDucking}");
            if (isGrounded && canDuck && !isDucking)
            {
                Debug.Log("Starting duck...");
                StartDuck();
            }
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
        
        // Apply speed multiplier when ducking
        float currentSpeed = isDucking ? moveSpeed * duckSpeedMultiplier : moveSpeed;
        Vector3 targetVelocity = movement * currentSpeed;
        
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
    
    void StartDuck()
    {
        Debug.Log("StartDuck() called");
        
        if (!canDuck || isDucking)
        {
            Debug.Log($"Cannot duck: canDuck={canDuck}, isDucking={isDucking}");
            return;
        }
        
        if (playerCollider == null)
        {
            Debug.LogError("PlayerCollider is null! Cannot duck.");
            return;
        }
        
        Debug.Log("Ducking started!");
        isDucking = true;
        canDuck = false;
        
        // Modify collider for ducking
        playerCollider.height = duckColliderHeight;
        playerCollider.center = duckColliderCenter;
        
        // Lower camera
        Vector3 cameraPos = cameraTransform.localPosition;
        cameraPos.y = duckCameraHeight;
        cameraTransform.localPosition = cameraPos;
        
        // Update raycast distance for ground check
        raycastDistance = (duckColliderHeight / 2) + 0.2f;
        
        // Start duck coroutine
        StartCoroutine(DuckCoroutine());
    }
    
    void EndDuck()
    {
        if (!isDucking) return;
        
        // Check if there's enough space to stand up
        Vector3 rayOrigin = transform.position + Vector3.up * (duckColliderHeight / 2);
        float standUpCheckDistance = originalColliderHeight - duckColliderHeight + 0.1f;
        
        if (Physics.Raycast(rayOrigin, Vector3.up, standUpCheckDistance, groundLayer))
        {
            // Not enough space to stand up, extend duck duration
            StartCoroutine(ExtendDuckUntilClear());
            return;
        }
        
        isDucking = false;
        
        // Restore original collider
        playerCollider.height = originalColliderHeight;
        playerCollider.center = originalColliderCenter;
        
        // Restore camera position
        Vector3 cameraPos = cameraTransform.localPosition;
        cameraPos.y = originalCameraHeight;
        cameraTransform.localPosition = cameraPos;
        
        // Restore raycast distance
        raycastDistance = (originalColliderHeight / 2) + 0.2f;
        
        // Start cooldown
        StartCoroutine(DuckCooldownCoroutine());
    }
    
    IEnumerator DuckCoroutine()
    {
        yield return new WaitForSeconds(duckDuration);
        EndDuck();
    }
    
    IEnumerator ExtendDuckUntilClear()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            
            Vector3 rayOrigin = transform.position + Vector3.up * (duckColliderHeight / 2);
            float standUpCheckDistance = originalColliderHeight - duckColliderHeight + 0.1f;
            
            if (!Physics.Raycast(rayOrigin, Vector3.up, standUpCheckDistance, groundLayer))
            {
                EndDuck();
                break;
            }
        }
    }
    
    IEnumerator DuckCooldownCoroutine()
    {
        yield return new WaitForSeconds(duckCooldown);
        canDuck = true;
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
    
    public void ForceDuck()
    {
        if (isGrounded && !isDucking)
        {
            StartDuck();
        }
    }
    
    public void ForceStandUp()
    {
        if (isDucking)
        {
            StopAllCoroutines();
            EndDuck();
        }
    }
    
    // Coroutine to handle temporary speed boost
    IEnumerator SpeedBoostCoroutine(float boostAmount, float duration)
    {
        moveSpeed += boostAmount;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalMoveSpeed;
    }
}