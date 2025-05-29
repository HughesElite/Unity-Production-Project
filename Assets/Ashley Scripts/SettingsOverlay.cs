using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsOverlay : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup settingsOverlay;
    public Button closeButton;         // Close button inside settings panel

    [Header("Population Settings")]
    public TMP_InputField populationInputField;
    public NPCPopulationController npcController;

    [Header("Game Settings")]
    public GameController gameController;         // Game controller reference
    public Button pauseResumeButton;              // Pause/resume button
    public TextMeshProUGUI pauseButtonText;       // Pause button text

    [Header("Weather Settings (if using ExclusiveButtonGroup)")]
    public ExclusiveButtonGroup weatherButtonGroup; // Weather controls

    private bool isSettingsOpen = false;

    void Start()
    {
        // Get references if not set
        if (gameController == null)
            gameController = GameController.Instance;

        // Connect close button
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSettings);

        // Connect game controls
        SetupGameControls();

        // Ensure settings start closed
        CloseSettings();

        // Update displays
        UpdateAllDisplays();
    }

    void SetupGameControls()
    {
        if (gameController == null) return;

        // Pause/Resume button
        if (pauseResumeButton != null)
            pauseResumeButton.onClick.AddListener(gameController.TogglePlayPause);
    }

    void Update()
    {
        // Update displays periodically when settings are open
        if (isSettingsOpen)
        {
            UpdateAllDisplays();
        }
    }

    void UpdateAllDisplays()
    {
        UpdatePauseButtonText();
    }

    void UpdatePauseButtonText()
    {
        if (pauseButtonText == null || gameController == null) return;

        pauseButtonText.text = gameController.IsGamePaused() ? "Resume Game" : "Pause Game";
    }

    public void ToggleSettings()
    {
        if (isSettingsOpen)
            CloseSettings();
        else
            OpenSettings();
    }

    public void OpenSettings()
    {
        settingsOverlay.alpha = 1f;
        settingsOverlay.interactable = true;
        settingsOverlay.blocksRaycasts = true;
        isSettingsOpen = true;

        Debug.Log("Settings panel opened");
        UpdateAllDisplays();

        // Optional: Pause simulation when settings open
        if (gameController != null && !gameController.IsGamePaused())
             gameController.PauseGame();
    }

    public void CloseSettings()
    {
        settingsOverlay.alpha = 0f;
        settingsOverlay.interactable = false;
        settingsOverlay.blocksRaycasts = false;
        isSettingsOpen = false;

        Debug.Log("Settings panel closed");

        // Optional: Resume simulation when settings close
        // if (gameController != null && gameController.IsGamePaused())
        //     gameController.ResumeGame();
    }
}