using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Add this for TextMeshPro support

public class PauseMenu : MonoBehaviour
{
    public bool GameISPaused = false;
    public GameObject pauseMenuUI;
    
    [Header("UI Elements")]
    public TextMeshProUGUI gameOverText;  // Changed from Text to TextMeshProUGUI
    public Button resumeButton;           // Reference to Resume button
    
    [Header("HUD Reference")]
    public GameHUD gameHUD;              // Reference to the HUD manager
    
    private bool isGameOver = false;      // Track if this is a game over scenario
    
    // Start is called before the first frame update
    void Start()
    {
        // Make sure the pause menu is hidden when the game starts
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        // Make sure the game is running
        Time.timeScale = 1f;
        GameISPaused = false;
        isGameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Only allow pause/unpause if it's not game over
        if(Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            if(GameISPaused)
            {
                resume();
            }
            else
            {
                pause();
            }
        }
    }

    public void restart()
    {
        Debug.Log("Restart button clicked!");
        GameISPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level1");
    }
    
    public void playAgain()
    {
        Debug.Log("Play Again button clicked!");
        // Same as restart - reload the current level
        GameISPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level1");
    }
    
    public void pause()
    {
        Debug.Log("Pause function called!");
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            GameISPaused = true;
            isGameOver = false;
            
            // Hide HUD during pause
            if (gameHUD != null)
            {
                gameHUD.HideHUD();
            }
            
            // Show normal pause menu
            SetupPauseMenu();
            
            // Enable cursor for menu interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Debug.LogError("pauseMenuUI is null!");
        }
    }
    
    public void resume()
    {
        Debug.Log("Resume function called!");
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            GameISPaused = false;
            isGameOver = false;
            
            // Show HUD when resuming
            if (gameHUD != null)
            {
                gameHUD.ShowHUD();
            }
            
            // Hide cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    public void ShowGameOverMenu()
    {
        Debug.Log("ShowGameOverMenu function called!");
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            GameISPaused = true;
            isGameOver = true;
            
            // Hide HUD during game over
            if (gameHUD != null)
            {
                gameHUD.HideHUD();
            }
            
            // Show game over menu
            SetupGameOverMenu();
            
            // Enable cursor for menu interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            Debug.Log("Game Over! Menu shown.");
        }
        else
        {
            Debug.LogError("pauseMenuUI is null in ShowGameOverMenu!");
        }
    }
    
    private void SetupPauseMenu()
    {
        // Hide game over text ) dwadawd
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
        
        // Show resume button
        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(true);
        }
    }
    
    private void SetupGameOverMenu()
    {
        // Show game over text
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "GAME OVER";
        }
        
        // Hide resume button
        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(false);
        }
    }
    
    public void Quit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Start");
    }
}