using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameHUD : MonoBehaviour
{
    [Header("HUD Elements")]
    public GameObject hudPanel;           
    public TextMeshProUGUI levelText;     
    public TextMeshProUGUI scoreText;     
    public TextMeshProUGUI distanceText;  

    public static GameHUD Instance; // singleton

    [Header("Game Info")]
    public int currentLevel = 1;          
    public int score = 0;                 
    public float distance = 0f;           

    [Header("References")]
    public Transform player;              

    private Vector3 startPosition;        
    private bool isHUDVisible = true;     

    void Awake()
    {
        Debug.Log($"GameHUD Awake called in scene: {SceneManager.GetActiveScene().name}");
        
        // Singleton pattern: only keep one GameHUD
        if (Instance != null && Instance != this)
        {
            Debug.Log($"Destroying duplicate GameHUD in scene: {SceneManager.GetActiveScene().name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"GameHUD Instance created and set to DontDestroyOnLoad in scene: {SceneManager.GetActiveScene().name}");
    }

    void Start()
    {
        Debug.Log($"GameHUD Start called. Instance null? {Instance == null}");
        InitializeHUD();
    }

    void InitializeHUD()
    {
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Player found and assigned");
            }
            else
            {
                Debug.LogWarning("Player not found!");
            }
        }

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
        Debug.Log("GameHUD subscribed to sceneLoaded event");
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("GameHUD unsubscribed from sceneLoaded event");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"OnSceneLoaded called for scene: {scene.name}, GameHUD Instance null? {Instance == null}");
        
        // Re-find player in new scene
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            startPosition = player.position;
            Debug.Log($"Player found in new scene: {scene.name}");
        }
        else
        {
            Debug.LogWarning($"Player not found in scene: {scene.name}");
        }

        // Auto-update level number based on scene name
        if (scene.name == "Level1")
            SetLevel(1);
        else if (scene.name == "Level2")
            SetLevel(2);
        else if (scene.name == "Level3")
            SetLevel(3);

        // Re-find HUD elements in case they're in the new scene
        RefreshHUDElements();

        // Reset distance for new level (but keep score!)
        distance = 0f;
        
        UpdateHUD();
        
        Debug.Log($"Scene loaded: {scene.name}, Current Score: {score}, Level: {currentLevel}");
    }

    void RefreshHUDElements()
    {
        Debug.Log("Refreshing HUD elements...");
        
        // Try to find HUD elements if they're not assigned or destroyed
        if (hudPanel == null)
        {
            hudPanel = FindUIElement("HUDPanel")?.gameObject;
            if (hudPanel != null) Debug.Log("HUDPanel found and assigned");
            else Debug.LogWarning("HUDPanel not found!");
        }

        if (levelText == null)
        {
            levelText = FindUIElement("LevelText") ?? FindUIElement("Level");
            if (levelText != null) Debug.Log($"Level UI found and assigned: {levelText.name}");
            else Debug.LogWarning("LevelText/Level not found!");
        }

        if (scoreText == null)
        {
            scoreText = FindUIElement("ScoreText") ?? FindUIElement("Score");
            if (scoreText != null) Debug.Log($"Score UI found and assigned: {scoreText.name}");
            else Debug.LogWarning("ScoreText/Score not found!");
        }

        if (distanceText == null)
        {
            distanceText = FindUIElement("DistanceText") ?? FindUIElement("Distance");
            if (distanceText != null) Debug.Log($"Distance UI found and assigned: {distanceText.name}");
            else Debug.LogWarning("DistanceText/Distance not found!");
        }
    }

    // Helper method to find UI elements by name
    private TextMeshProUGUI FindUIElement(string name)
    {
        GameObject obj = GameObject.Find(name);
        return obj?.GetComponent<TextMeshProUGUI>();
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
        Debug.Log($"Score added: +{points}, Total Score: {score}, Instance null? {Instance == null}");
    }

    public void SetLevel(int level)
    {
        currentLevel = level;
        UpdateHUD();
        Debug.Log($"Level set to: {level}");
    }

    public bool IsHUDVisible()
    {
        return isHUDVisible;
    }

    public void ResetScore()
    {
        score = 0;
        UpdateHUD();
    }

    public int GetScore()
    {
        return score;
    }

    public void LoadNextLevel()
    {
        if (currentLevel == 1)
        {
            Debug.Log($"Loading Level 2... Current Score: {score}");
            SceneManager.LoadScene("Level2");
        }
        else if (currentLevel == 2)
        {
            Debug.Log($"Loading Level 3... Current Score: {score}");
            SceneManager.LoadScene("Level3");
        }
        else
        {
            Debug.Log($"Game Complete! Final Score: {score}");
        }
    }

    // Static method to check if instance exists
    public static bool HasInstance()
    {
        return Instance != null;
    }
}