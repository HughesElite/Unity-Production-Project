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
        gameController = GameController.Instance;

        if (gameController == null)
        {
            return;
        }

        SetupButtonListeners();

        // Subscribe to game state events
        gameController.OnGamePaused.AddListener(UpdatePlayPauseButton);
        gameController.OnGameResumed.AddListener(UpdatePlayPauseButton);

        UpdatePlayPauseButton();
    }

    private void SetupButtonListeners()
    {
        if (playPauseButton != null)
            playPauseButton.onClick.AddListener(gameController.TogglePlayPause);

        if (resetButton != null)
            resetButton.onClick.AddListener(gameController.ResetGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(gameController.ExitGame);

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

    private void OpenSettings()
    {
        // Add settings panel logic here
    }

    private void GoToMainMenu()
    {
        // Add main menu navigation logic here
    }

    private void ShowHelp()
    {
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