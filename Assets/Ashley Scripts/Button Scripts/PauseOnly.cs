using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseOnlyButton : MonoBehaviour
{
    [Header("Text References (Optional)")]
    public Text buttonText;
    public TextMeshProUGUI buttonTMPText;

    [Header("Pause Settings")]
    public bool updateButtonText = false;
    public string pausedText = "Game Paused";

    private GameController gameController;

    private void Start()
    {
        gameController = GameController.Instance;
    }

    public void PauseGame()
    {
        if (gameController != null)
        {
            gameController.PauseGame();
        }
        else
        {
            Time.timeScale = 0f;
        }

        if (updateButtonText)
        {
            UpdateButtonText();
        }
    }

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
    }

    private void UpdateButtonText()
    {
        if (buttonText != null)
            buttonText.text = pausedText;
        if (buttonTMPText != null)
            buttonTMPText.text = pausedText;
    }

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
}