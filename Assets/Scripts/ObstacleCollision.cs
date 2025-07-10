using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    private PauseMenu pauseMenu;
    private ObstacleSpawner obstacleSpawner;
    
    void Start()
    {
        // Find the PauseMenu and ObstacleSpawner in the scene
        pauseMenu = FindObjectOfType<PauseMenu>();
        obstacleSpawner = FindObjectOfType<ObstacleSpawner>();
        
        if (pauseMenu == null)
        {
            Debug.LogError("PauseMenu not found in scene!");
        }
        
        if (obstacleSpawner == null)
        {
            Debug.LogError("ObstacleSpawner not found in scene!");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            GameOver();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the colliding object has the "Player" tag
        if (collision.gameObject.CompareTag("Player"))
        {
            GameOver();
        }
    }

    void GameOver()
    {
        // Stop spawning new obstacles
        if (obstacleSpawner != null)
        {
            obstacleSpawner.enabled = false;
        }
        
        // Show the pause menu (acts as game over menu)
        if (pauseMenu != null)
        {
            pauseMenu.ShowGameOverMenu();
        }
        
        Debug.Log("Game Over! Player hit obstacle.");
    }
}