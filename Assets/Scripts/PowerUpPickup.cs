using UnityEngine;

public enum PowerUpType
{
    CoinDoubler,
    StealthMode
}

public class PowerUpPickup : MonoBehaviour
{
    [Header("Power-Up Type")]
    public PowerUpType powerUpType = PowerUpType.CoinDoubler;
    
    [Header("Visual Effects")]
    public float rotationSpeed = 50f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    
    [Header("Effects")]
    public GameObject pickupEffect; // Optional particle effect
    public AudioClip pickupSound;   // Optional sound effect
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private Vector3 startPosition;
    private AudioSource audioSource;

    void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        
        // Add a trigger collider if none exists
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 2f; // Larger radius for easier collection
            
            if (showDebugInfo)
            {
                Debug.Log($"Added SphereCollider to {gameObject.name}");
            }
        }
        else
        {
            col.isTrigger = true; // Ensure it's a trigger
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"PowerUp {gameObject.name} spawned at {transform.position} with type: {powerUpType}");
        }
    }

    void Update()
    {
        // Rotate the power-up
        if (rotationSpeed != 0)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
        
        // Bob up and down
        if (bobHeight > 0)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (showDebugInfo)
        {
            Debug.Log($"PowerUp trigger hit by: {other.name} with tag: {other.tag}");
        }
        
        if (other.CompareTag("Player"))
        {
            if (showDebugInfo)
            {
                Debug.Log($"Player collected {powerUpType} power-up!");
            }
            
            ActivatePowerUp();
            PlayEffects();
            
            // Small delay before destroying to ensure effects play
            Invoke(nameof(DestroyPowerUp), 0.1f);
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

        // Activate the appropriate power-up based on type
        switch (powerUpType)
        {
            case PowerUpType.CoinDoubler:
                powerUpManager.ActivateCoinDoubler();
                if (showDebugInfo)
                {
                    Debug.Log("Activated Coin Doubler!");
                }
                break;
            case PowerUpType.StealthMode:
                powerUpManager.ActivateStealthMode();
                if (showDebugInfo)
                {
                    Debug.Log("Activated Stealth Mode!");
                }
                break;
        }
    }

    void PlayEffects()
    {
        // Play pickup effect
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, transform.rotation);
            // Auto-destroy effect after 3 seconds
            Destroy(effect, 3f);
        }

        // Play pickup sound
        if (pickupSound != null && audioSource != null)
        {
            // Create a temporary audio source to play the sound even after the object is destroyed
            GameObject audioObj = new GameObject("TempAudio");
            AudioSource tempAudio = audioObj.AddComponent<AudioSource>();
            tempAudio.clip = pickupSound;
            tempAudio.volume = audioSource.volume;
            tempAudio.pitch = audioSource.pitch;
            tempAudio.Play();
            
            // Destroy the temporary audio object after the clip finishes
            Destroy(audioObj, pickupSound.length);
        }
    }
    
    void DestroyPowerUp()
    {
        Destroy(gameObject);
    }

    // Debug method to test power-up activation manually
    [ContextMenu("Test Activate PowerUp")]
    void TestActivatePowerUp()
    {
        ActivatePowerUp();
    }
}