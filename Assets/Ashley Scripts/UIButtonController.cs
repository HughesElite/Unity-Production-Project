using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIButtonController : MonoBehaviour
{
    [Header("Button References")]
    public Button playPauseButton;
    public Button resetButton;
    public Button exitButton;

    [Header("Text References")]
    public Text playPauseText;
    public TextMeshProUGUI playPauseTMPText;

    [Header("Additional Buttons (for future expansion)")]
    public Button settingsButton;
    public Button mainMenuButton;
    public Button helpButton;

    private GameController gameController;

    private void Start()
    {
        // Get reference to GameController
        gameController = GameController.Instance;
        if (gameController == null)
        {
            Debug.LogError("GameController not found! Make sure GameController is in the scene.");
            return;
        }

        // Set up button listeners
        SetupButtonListeners();

        // Subscribe to game state events
        gameController.OnGamePaused.AddListener(UpdatePlayPauseButton);
        gameController.OnGameResumed.AddListener(UpdatePlayPauseButton);

        // Initial button state
        UpdatePlayPauseButton();
    }

    private void SetupButtonListeners()
    {
        // Play/Pause button
        if (playPauseButton != null)
            playPauseButton.onClick.AddListener(gameController.TogglePlayPause);

        // Reset button
        if (resetButton != null)
            resetButton.onClick.AddListener(gameController.ResetGame);

        // Exit button
        if (exitButton != null)
            exitButton.onClick.AddListener(gameController.ExitGame);

        // Future buttons (add functionality as needed)
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (helpButton != null)
            helpButton.onClick.AddListener(ShowHelp);
    }

    private void UpdatePlayPauseButton()
    {
        if (gameController == null) return;

        string buttonText = gameController.IsGamePaused() ? "Play" : "Pause";

        if (playPauseText != null)
            playPauseText.text = buttonText;

        if (playPauseTMPText != null)
            playPauseTMPText.text = buttonText;
    }

    // Future button functionality
    private void OpenSettings()
    {
        Debug.Log("Settings button clicked");
        // Add settings panel logic here
    }

    private void GoToMainMenu()
    {
        Debug.Log("Main Menu button clicked");
        // Add main menu navigation logic here
    }

    private void ShowHelp()
    {
        Debug.Log("Help button clicked");
        // Add help panel logic here
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (gameController != null)
        {
            gameController.OnGamePaused.RemoveListener(UpdatePlayPauseButton);
            gameController.OnGameResumed.RemoveListener(UpdatePlayPauseButton);
        }
    }
}