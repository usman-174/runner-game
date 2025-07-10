using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("HUD Elements")]
    public GameObject hudPanel;           // Main HUD container
    public TextMeshProUGUI levelText;     // Level number display
    public TextMeshProUGUI scoreText;     // Score display (optional)
    public TextMeshProUGUI distanceText;  // Distance traveled (optional)
    
    [Header("Game Info")]
    public int currentLevel = 1;          // Current level number
    public int score = 0;                 // Player score
    public float distance = 0f;           // Distance traveled
    
    [Header("References")]
    public Transform player;              // Reference to player for distance calculation
    
    private Vector3 startPosition;        // Player's starting position
    private bool isHUDVisible = true;     // Track HUD visibility
    
    void Start()
    {
        // Store the player's starting position
        if (player != null)
        {
            startPosition = player.position;
        }
        
        // Initialize HUD but don't show it yet
        UpdateHUD();
        
        // Only show HUD if game is actually running (not paused)
        if (Time.timeScale > 0)
        {
            ShowHUD();
        }
        else
        {
            HideHUD();
        }
    }
    
    void Update()
    {
        // Check if game state changed
        CheckGameState();
        
        // Only update HUD if it's visible and game is running
        if (isHUDVisible && Time.timeScale > 0)
        {
            UpdateDistance();
            UpdateHUD();
        }
    }
    
    void CheckGameState()
    {
        // Auto-hide HUD when game is paused (Time.timeScale = 0)
        // Auto-show HUD when game is running (Time.timeScale > 0)
        if (Time.timeScale == 0 && isHUDVisible)
        {
            HideHUD();
        }
        else if (Time.timeScale > 0 && !isHUDVisible)
        {
            ShowHUD();
        }
    }
    
    void UpdateDistance()
    {
        if (player != null)
        {
            // Calculate distance traveled (Z-axis for forward movement)
            distance = Mathf.Max(0, player.position.z - startPosition.z);
        }
    }
    
    void UpdateHUD()
    {
        // Update level text
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
        }
        
        // Update score text (optional)
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
        
        // Update distance text (optional)
        if (distanceText != null)
        {
            distanceText.text = $"Distance: {distance:F0}m";
        }
    }
    
    public void ShowHUD()
    {
        Debug.Log("ShowHUD called");
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
            isHUDVisible = true;
            Debug.Log("HUD is now visible");
        }
        else
        {
            Debug.LogError("hudPanel is null!");
        }
    }
    
    public void HideHUD()
    {
        Debug.Log("HideHUD called");
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
            isHUDVisible = false;
            Debug.Log("HUD is now hidden");
        }
        else
        {
            Debug.LogError("hudPanel is null!");
        }
    }
    
    public void AddScore(int points)
    {
        score += points;
        UpdateHUD();
    }
    
    public void SetLevel(int level)
    {
        currentLevel = level;
        UpdateHUD();
    }
    
    public bool IsHUDVisible()
    {
        return isHUDVisible;
    }
}