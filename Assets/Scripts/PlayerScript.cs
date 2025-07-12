using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // ✅ Added for Coroutine

public class Player : MonoBehaviour
{
    private GameHUD gameHUD;

    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;
    private Transform cameraTransform;

    private Rigidbody rb;
    public float MoveSpeed = 5f;
    private float originalMoveSpeed; // ✅ Added for storing original speed
    private float moveHorizontal;
    private float moveForward = 1f;

    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float ascendMultiplier = 2f;
    private bool isGrounded = true;
    public LayerMask groundLayer;
    private float groundCheckTimer = 0f;
    private float groundCheckDelay = 0.3f;
    private float playerHeight;
    private float raycastDistance;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cameraTransform = Camera.main.transform;

        playerHeight = GetComponent<CapsuleCollider>().height * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.2f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gameHUD = FindObjectOfType<GameHUD>();

        originalMoveSpeed = MoveSpeed; // ✅ store original speed at start
    }

    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = 1f;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

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
        Vector3 targetVelocity = movement * MoveSpeed;

        Vector3 velocity = rb.linearVelocity; // ✅ Corrected to rb.velocity
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            if (gameHUD != null)
            {
                gameHUD.AddScore(1);
            }
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Diamond"))
        {
            if (gameHUD != null)
            {
                gameHUD.AddScore(5);
            }

            Destroy(other.gameObject);

            // ✅ Start speed boost coroutine
            StartCoroutine(SpeedBoost());
        }

        if (other.CompareTag("EndPoint"))
        {
            Debug.Log("Level Complete! Calling LoadNextLevel from GameHUD...");
            if (gameHUD != null)
            {
                gameHUD.LoadNextLevel();
            }
        }
    }

    // ✅ Coroutine to handle temporary speed boost
    IEnumerator SpeedBoost()
    {
        MoveSpeed +=10f;
        yield return new WaitForSeconds(10f);
        MoveSpeed = originalMoveSpeed;
    }
}
