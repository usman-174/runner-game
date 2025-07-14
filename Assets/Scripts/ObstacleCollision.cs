using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    private PauseMenu pauseMenu;
    private CentralizedSpawnManager spawnManager;
    
    void Start()
    {
        // Find the PauseMenu and CentralizedSpawnManager in the scene
        pauseMenu = FindObjectOfType<PauseMenu>();
        spawnManager = FindObjectOfType<CentralizedSpawnManager>();
        
        if (pauseMenu == null)
        {
            Debug.LogError("PauseMenu not found in scene!");
        }
        
        if (spawnManager == null)
        {
            Debug.LogError("CentralizedSpawnManager not found in scene!");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Check if stealth mode is active before triggering game over
            if (!IsPlayerInStealthMode())
            {
                GameOver();
            }
            else
            {
                Debug.Log("Player passed through obstacle while in stealth mode!");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the colliding object has the "Player" tag
        if (collision.gameObject.CompareTag("Player"))
        {
            // Check if stealth mode is active before triggering game over
            if (!IsPlayerInStealthMode())
            {
                GameOver();
            }
            else
            {
                Debug.Log("Player passed through obstacle while in stealth mode!");
            }
        }
    }

    bool IsPlayerInStealthMode()
    {
        // Check if PowerUpManager exists and stealth mode is active
        PowerUpManager powerUpManager = PowerUpManager.Instance;
        if (powerUpManager != null)
        {
            return powerUpManager.IsStealthModeActive;
        }
        
        return false; // If no PowerUpManager found, stealth is not active
    }

    void GameOver()
    {
        // Stop spawning new obstacles and coins
        if (spawnManager != null)
        {
            spawnManager.enabled = false;
        }
        
        // Show the pause menu (acts as game over menu)
        if (pauseMenu != null)
        {
            pauseMenu.ShowGameOverMenu();
        }
        
        Debug.Log("Game Over! Player hit obstacle.");
    }
}