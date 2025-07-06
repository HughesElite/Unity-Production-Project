using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayPauseButton : MonoBehaviour
{
    [Header("Text References")]
    public Text buttonText;
    public TextMeshProUGUI buttonTMPText;

    private Button button;
    public bool isGamePaused = true;

    private void Start()
    {
        button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(TogglePlayPause);

        // Check if hard reset just happened
        if (PlayerPrefs.GetInt("HardResetHappened", 0) == 1)
        {
            // Clear the flag
            PlayerPrefs.SetInt("HardResetHappened", 0);

            // Force paused state after hard reset
            isGamePaused = true;
            Time.timeScale = 0f;
            UpdateButtonText();
        }
        else
        {
            // Normal startup - starts paused
            Time.timeScale = 0f;
            UpdateButtonText();
        }
    }

    private void Update()
    {
        // Handle spacebar shortcut
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause();
        }
    }

    // Made this public so it can be used in Unity's OnClick event system
    public void TogglePlayPause()
    {
        if (isGamePaused)
        {
            // Start game
            Time.timeScale = 1f;
            isGamePaused = false;
        }
        else
        {
            // Pause game
            Time.timeScale = 0f;
            isGamePaused = true;
        }

        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        string text = isGamePaused ? "Play" : "Pause";

        if (buttonText != null)
            buttonText.text = text;

        if (buttonTMPText != null)
            buttonTMPText.text = text;
    }
}