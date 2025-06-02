using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    [Header("Game State")]
    public bool isGamePaused = true;
    public bool startGamePaused = true;

    [Header("Events")]
    public UnityEvent OnGamePaused;
    public UnityEvent OnGameResumed;
    public UnityEvent OnGameReset;
    public UnityEvent OnGameExit;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        // using a singleton pattern for easy access
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (startGamePaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void Update()
    {
        // Global keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }
    }

    public void TogglePlayPause()
    {
        if (isGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isGamePaused = true;
        OnGamePaused?.Invoke();
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        OnGameResumed?.Invoke();
        Debug.Log("Game Resumed");
    }

    public void ResetGame()
    {
        OnGameReset?.Invoke();
        PauseGame();

        // Small delay to ensure pause takes effect
        Invoke(nameof(LoadCurrentScene), 0.1f);
    }

    private void LoadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void ExitGame()
    {
        OnGameExit?.Invoke();
        Debug.Log("Exiting Game");

        // Quits the application (works in a built game)
        Application.Quit();

        // If running in the Unity Editor, stops play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Utility methods for other scripts
    public bool IsGamePaused() => isGamePaused;
    public float GetGameSpeed() => Time.timeScale;
    public void SetGameSpeed(float speed) => Time.timeScale = speed;
}