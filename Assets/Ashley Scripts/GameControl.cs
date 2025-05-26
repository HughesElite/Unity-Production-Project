using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameControl : MonoBehaviour
{
    private bool isGamePaused = true;
    public Button playPauseButton;
    public Text buttonText;
    public TextMeshProUGUI buttonTMPText;
    public static bool IsResetting { get; private set; } = false;

    private void Awake()
    {
        Time.timeScale = 0f;
        isGamePaused = true;
    }

    private void Start()
    {
        if (playPauseButton != null)
        {
            playPauseButton.onClick.RemoveAllListeners();
            playPauseButton.onClick.AddListener(TogglePlayPause);
        }
        UpdateButtonText();
    }

    private void Update()
    {
        // Optimized: Only check for key down event, not continuous key state
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause();
        }
    }

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
        if (IsResetting)
        {
            PauseGame();
            UpdateButtonText();
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        UpdateButtonText();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isGamePaused = true;
        UpdateButtonText();
    }

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

    private void UpdateButtonText()
    {
        string text = isGamePaused ? "Play" : "Pause";

        if (buttonText != null)
            buttonText.text = text;

        if (buttonTMPText != null)
            buttonTMPText.text = text;
    }

    public void ResetScene()
    {
        IsResetting = true;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public static void FinishReset()
    {
        IsResetting = false;
    }
}