using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLogic : MonoBehaviour
{
    [Header("Game References")]
    private GameHUD gameHUD;
    private PlayerMovement playerMovement;

    [Header("Power-up Settings")]
    public float speedBoostAmount = 10f;
    public float speedBoostDuration = 10f;

    [Header("Score Values")]
    public int coinScore = 1;
    public int diamondScore = 5;

    void Start()
    {
        Debug.Log($"PlayerLogic Start called in scene: {SceneManager.GetActiveScene().name}");
        InitializeComponents();
    }

    void InitializeComponents()
    {
        Debug.Log($"Initializing PlayerLogic components...");
        Debug.Log($"GameHUD.Instance null? {GameHUD.Instance == null}");
        Debug.Log($"GameHUD.HasInstance()? {GameHUD.HasInstance()}");
        
        // Use singleton instance instead of FindObjectOfType
        gameHUD = GameHUD.Instance;
        playerMovement = GetComponent<PlayerMovement>();

        // Ensure we have the required components
        if (gameHUD == null)
        {
            Debug.LogError("GameHUD Instance not found! Make sure GameHUD exists and is properly initialized.");
            
            // Try to find it as a fallback
            gameHUD = FindObjectOfType<GameHUD>();
            if (gameHUD != null)
            {
                Debug.Log("Found GameHUD using FindObjectOfType as fallback");
            }
        }
        else
        {
            Debug.Log("GameHUD Instance found successfully!");
        }

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found! Make sure it's attached to the same GameObject.");
        }
        else
        {
            Debug.Log("PlayerMovement component found!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        HandleCollectibleInteraction(other);
        HandleLevelProgression(other);
    }

    void HandleCollectibleInteraction(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            CollectCoin(other);
        }
        else if (other.CompareTag("Diamond"))
        {
            CollectDiamond(other);
        }
    }

    void HandleLevelProgression(Collider other)
    {
        if (other.CompareTag("EndPoint"))
        {
            CompleteLevel();
        }
    }

    void CollectCoin(Collider coinCollider)
    {
        Debug.Log($"Coin collection triggered. GameHUD.Instance null? {GameHUD.Instance == null}");
        
        // Get base score
        int baseScore = coinScore;

        // Apply power-up multiplier
        PowerUpManager powerUpManager = PowerUpManager.Instance;
        if (powerUpManager != null)
        {
            baseScore *= powerUpManager.GetCoinMultiplier();

            if (powerUpManager.IsCoinDoublerActive)
            {
                Debug.Log("💰 DOUBLED COIN! 2x Score!");
            }
        }

        // Add score using singleton instance
        if (GameHUD.Instance != null)
        {
            Debug.Log($"Adding score: {baseScore}");
            GameHUD.Instance.AddScore(baseScore);
        }
        else
        {
            Debug.LogError("GameHUD Instance is null! Cannot add score.");
            
            // Try alternative approach
            if (gameHUD != null)
            {
                Debug.Log("Using cached gameHUD reference instead");
                gameHUD.AddScore(baseScore);
            }
            else
            {
                Debug.LogError("Cached gameHUD is also null!");
            }
        }

        // Destroy the coin
        Destroy(coinCollider.gameObject);

        Debug.Log($"Coin collected! Score: +{baseScore}");
    }

    void CollectDiamond(Collider diamondCollider)
    {
        Debug.Log($"Diamond collection triggered. GameHUD.Instance null? {GameHUD.Instance == null}");
        
        // Add score using singleton instance
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.AddScore(diamondScore);
        }
        else
        {
            Debug.LogError("GameHUD Instance is null! Cannot add score.");
            
            // Try alternative approach
            if (gameHUD != null)
            {
                Debug.Log("Using cached gameHUD reference instead");
                gameHUD.AddScore(diamondScore);
            }
            else
            {
                Debug.LogError("Cached gameHUD is also null!");
            }
        }

        // Destroy the diamond
        Destroy(diamondCollider.gameObject);

        // Apply speed boost
        if (playerMovement != null)
        {
            playerMovement.ApplySpeedBoost(speedBoostAmount, speedBoostDuration);
        }

        Debug.Log($"Diamond collected! Score: +{diamondScore}, Speed boost applied!");
    }

    void CompleteLevel()
    {
        Debug.Log("Level Complete! Calling LoadNextLevel from GameHUD...");

        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.LoadNextLevel();
        }
        else
        {
            Debug.LogError("GameHUD Instance is null! Cannot load next level.");
        }
    }

    // Public methods for other scripts to call
    public void AddScore(int points)
    {
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.AddScore(points);
        }
    }

    public void TriggerLevelComplete()
    {
        CompleteLevel();
    }

    public void ApplyCustomPowerUp(float speedBoost, float duration)
    {
        if (playerMovement != null)
        {
            playerMovement.ApplySpeedBoost(speedBoost, duration);
        }
    }

    // Method to manually reinitialize if needed
    [ContextMenu("Reinitialize Components")]
    public void ReinitializeComponents()
    {
        InitializeComponents();
    }
}