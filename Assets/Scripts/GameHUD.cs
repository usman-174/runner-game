using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;

public class GameHUD : MonoBehaviour
{
    [Header("HUD Elements")]
    public GameObject hudPanel;           // Main HUD container
    public TextMeshProUGUI levelText;     // Level number display
    public TextMeshProUGUI scoreText;     // Score display (optional)
    public TextMeshProUGUI distanceText;  // Distance traveled (optional)

    public static GameHUD Instance; // singleton

    [Header("Game Info")]
    public int currentLevel = 1;          // Current level number
    public int score = 0;                 // Player score
    public float distance = 0f;           // Distance traveled

    [Header("References")]
    public Transform player;              // Reference to player for distance calculation

    private Vector3 startPosition;        // Player's starting position
    private bool isHUDVisible = true;     // Track HUD visibility

    void Awake()
    {
        // Singleton pattern: only keep one GameHUD
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        // Store the player's starting position
        if (player != null)
        {
            startPosition = player.position;
        }

        // Initialize HUD
        UpdateHUD();

        // Only show HUD if game is running
        if (Time.timeScale > 0)
        {
            ShowHUD();
        }
        else
        {
            HideHUD();
        }
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.FindWithTag("Player")?.transform;

        if (player != null)
            startPosition = player.position;

        // Auto-update level number based on scene name
        if (scene.name == "Level1")
            SetLevel(1);
        else if (scene.name == "Level2")
            SetLevel(2);
        else if (scene.name == "Level3")
            SetLevel(3);

        UpdateHUD();
    }

    void Update()
    {
        CheckGameState();

        if (isHUDVisible && Time.timeScale > 0)
        {
            UpdateDistance();
            UpdateHUD();
        }
    }

    void CheckGameState()
    {
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
            distance = Mathf.Max(0, player.position.z - startPosition.z);
        }
    }

    void UpdateHUD()
    {
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        if (distanceText != null)
        {
            distanceText.text = $"Distance: {distance:F0}m";
        }
    }

    public void ShowHUD()
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
            isHUDVisible = true;
        }
    }

    public void HideHUD()
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
            isHUDVisible = false;
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

    // ✅ New method for loading next level based on current level
    public void LoadNextLevel()
    {
        if (currentLevel == 1)
        {
            Debug.Log("Loading Level 2...");
            SceneManager.LoadScene("Level2");
        }
        else if (currentLevel == 2)
        {
            Debug.Log("Loading Level 3...");
            SceneManager.LoadScene("Level3");
        }
        else
        {
            Debug.Log("No further levels configured.");
        }
    }
}
