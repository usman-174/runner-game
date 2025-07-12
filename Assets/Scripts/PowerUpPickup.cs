using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{
    [Header("Visual Effects")]
    public float rotationSpeed = 50f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    
    [Header("Effects")]
    public GameObject pickupEffect; // Optional particle effect
    public AudioClip pickupSound;   // Optional sound effect
    
    private Vector3 startPosition;
    private AudioSource audioSource;

    void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        
        // Add a trigger collider if none exists
        if (GetComponent<Collider>() == null)
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 1f;
        }
    }

    void Update()
    {
        // Rotate the power-up
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActivatePowerUp();
            PlayEffects();
            Destroy(gameObject);
        }
    }

    void ActivatePowerUp()
    {
        PowerUpManager powerUpManager = PowerUpManager.Instance;
        
        if (powerUpManager == null)
        {
            Debug.LogError("PowerUpManager not found! Make sure it exists in the scene.");
            return;
        }

        // For now, we only have coin doubler. You can expand this later
        powerUpManager.ActivateCoinDoubler();
    }

    void PlayEffects()
    {
        // Play pickup effect
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, transform.rotation);
        }

        // Play pickup sound
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }
}