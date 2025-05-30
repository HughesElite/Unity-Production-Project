using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayPauseButton : MonoBehaviour
{
    [Header("Text References")]
    public Text buttonText;
    public TextMeshProUGUI buttonTMPText;

    private Button button;
    private bool isGamePaused = true;

    private void Start()
    {
        button = GetComponent<Button>();

        // Optional: You can still add the listener programmatically
        // Comment this out if you're using the inspector OnClick instead
        if (button != null)
          button.onClick.AddListener(TogglePlayPause);

        // Start paused
        Time.timeScale = 0f;
        UpdateButtonText();
    }

    private void Update()
    {
        // Handle spacebar shortcut
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause();
        }
    }

    // Made public so it can be used in Unity's OnClick event system
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