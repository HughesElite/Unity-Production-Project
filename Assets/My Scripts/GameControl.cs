using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Include this if using TextMeshPro

public class GameControl : MonoBehaviour
{
    private bool isGamePaused = true; // Start paused by default

    // Reference to the play/pause button and its text
    public Button playPauseButton;

    // Reference to the button's text component (use appropriate type)
    // For regular UI Text:
    public Text buttonText;
    // OR for TextMeshPro:
    public TextMeshProUGUI buttonTMPText;

    // Static property for scene reset state
    public static bool IsResetting { get; private set; } = false;

    private void Awake()
    {
        // Initially pause the game
        Time.timeScale = 0f;
        isGamePaused = true;
    }

    private void Start()
    {
        // Set up button click event
        if (playPauseButton != null)
        {
            playPauseButton.onClick.RemoveAllListeners();
            playPauseButton.onClick.AddListener(TogglePlayPause);
        }

        // Update button text for initial state
        UpdateButtonText();
    }

    // This runs when scene loads - automatically pause after reset
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If we're resetting, pause the game after scene loads
        if (IsResetting)
        {
            PauseGame();
            UpdateButtonText();
        }
    }

    // Call this method to start the game
    public void StartGame()
    {
        // Ensure the game is not paused
        Time.timeScale = 1f; // Resumes game time
        isGamePaused = false;
        UpdateButtonText();
    }

    // Call this method to pause the game
    public void PauseGame()
    {
        Time.timeScale = 0f; // Freezes game time
        isGamePaused = true;
        UpdateButtonText();
    }

    // Toggle between play and pause states
    public void TogglePlayPause()
    {
        if (isGamePaused)
        {
            StartGame();
        }
        else
        {
            PauseGame();
        }
    }

    // Updates the button's text based on game state
    private void UpdateButtonText()
    {
        // For regular UI Text
        if (buttonText != null)
        {
            buttonText.text = isGamePaused ? "Play" : "Pause";
        }

        // For TextMeshPro Text
        if (buttonTMPText != null)
        {
            buttonTMPText.text = isGamePaused ? "Play" : "Pause";
        }
    }

    // Method to reset the scene
    public void ResetScene()
    {
        // Set the static flag that we're resetting
        IsResetting = true;

        // Reload the current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    // This should be called by InfectedCountDisplay when the scene starts
    public static void FinishReset()
    {
        IsResetting = false;
    }
}