using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseOnlyButton : MonoBehaviour
{
    [Header("Text References (Optional)")]
    public Text buttonText;
    public TextMeshProUGUI buttonTMPText;

    [Header("Pause Settings")]
    public bool updateButtonText = false; // Set to true if you want the button text to change
    public string pausedText = "Game Paused";

    [Header("Debug")]
    public bool showDebugLogs = true;

    private GameController gameController;

    private void Start()
    {
        // Get reference to GameController
        gameController = GameController.Instance;

        if (gameController == null && showDebugLogs)
        {
            Debug.LogWarning("PauseOnlyButton: GameController not found. Will use fallback pause method.");
        }
    }

    // Public method to be called by button click events
    public void PauseGame()
    {
        if (gameController != null)
        {
            // Use GameController's pause method
            gameController.PauseGame();
        }
        else
        {
            // Fallback pause method
            Time.timeScale = 0f;
        }

        if (updateButtonText)
        {
            UpdateButtonText();
        }

        if (showDebugLogs)
        {
            Debug.Log("PauseOnlyButton: Game paused");
        }
    }

    // Alternative method that checks if already paused
    public void PauseGameIfNotPaused()
    {
        bool isAlreadyPaused = false;

        if (gameController != null)
        {
            isAlreadyPaused = gameController.IsGamePaused();
        }
        else
        {
            isAlreadyPaused = Time.timeScale == 0f;
        }

        if (!isAlreadyPaused)
        {
            PauseGame();
        }
        else if (showDebugLogs)
        {
            Debug.Log("PauseOnlyButton: Game is already paused");
        }
    }

    private void UpdateButtonText()
    {
        if (buttonText != null)
            buttonText.text = pausedText;

        if (buttonTMPText != null)
            buttonTMPText.text = pausedText;
    }

    // Public method to check if game is paused
    public bool IsGamePaused()
    {
        if (gameController != null)
        {
            return gameController.IsGamePaused();
        }
        else
        {
            return Time.timeScale == 0f;
        }
    }

    // For debugging
    [ContextMenu("Pause Game Now")]
    void DebugPauseGame()
    {
        PauseGame();
    }
}