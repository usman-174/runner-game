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
        InitializeComponents();
    }

    void InitializeComponents()
    {
        gameHUD = FindObjectOfType<GameHUD>();
        playerMovement = GetComponent<PlayerMovement>();

        // Ensure we have the required components
        if (gameHUD == null)
        {
            Debug.LogWarning("GameHUD not found! Make sure GameHUD exists in the scene.");
        }

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found! Make sure it's attached to the same GameObject.");
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
        // Get base score
        int baseScore = coinScore; // Your existing coinScore value (1)

        // Apply power-up multiplier
        PowerUpManager powerUpManager = PowerUpManager.Instance;
        if (powerUpManager != null)
        {
            baseScore *= powerUpManager.GetCoinMultiplier();

            // Show visual feedback if doubler is active
            if (powerUpManager.IsCoinDoublerActive)
            {
                Debug.Log("💰 DOUBLED COIN! 2x Score!");
            }
        }

        // Add score
        if (gameHUD != null)
        {
            gameHUD.AddScore(baseScore);
        }

        // Destroy the coin
        Destroy(coinCollider.gameObject);

        Debug.Log($"Coin collected! Score: +{baseScore}");
    }
    void CollectDiamond(Collider diamondCollider)
    {
        // Add score
        if (gameHUD != null)
        {
            gameHUD.AddScore(diamondScore);
        }

        // Destroy the diamond
        Destroy(diamondCollider.gameObject);

        // Apply speed boost
        if (playerMovement != null)
        {
            playerMovement.ApplySpeedBoost(speedBoostAmount, speedBoostDuration);
        }

        // Optional: Add sound effect or particle effect here
        Debug.Log($"Diamond collected! Score: +{diamondScore}, Speed boost applied!");
    }

    void CompleteLevel()
    {
        Debug.Log("Level Complete! Calling LoadNextLevel from GameHUD...");

        if (gameHUD != null)
        {
            gameHUD.LoadNextLevel();
        }
        else
        {
            Debug.LogError("GameHUD is null! Cannot load next level.");
        }
    }

    // Public methods for other scripts to call
    public void AddScore(int points)
    {
        if (gameHUD != null)
        {
            gameHUD.AddScore(points);
        }
    }

    public void TriggerLevelComplete()
    {
        CompleteLevel();
    }

    // Optional: Method to handle custom power-ups
    public void ApplyCustomPowerUp(float speedBoost, float duration)
    {
        if (playerMovement != null)
        {
            playerMovement.ApplySpeedBoost(speedBoost, duration);
        }
    }
}