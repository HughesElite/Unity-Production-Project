using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PersistentResetManager : MonoBehaviour
{
    private static PersistentResetManager instance;

    [Header("Debug")]
    public bool debugMode = true;

    void Awake()
    {
        // Make this survive scene reloads
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            if (debugMode)
                Debug.Log("PersistentResetManager created and set to DontDestroyOnLoad");
        }
        else
        {
            if (debugMode)
                Debug.Log("PersistentResetManager already exists, destroying duplicate");
            Destroy(this.gameObject); // Prevent duplicates
        }
    }

    /// <summary>
    /// Call this method to hard reset the scene and fix the play button
    /// </summary>
    public static void HardResetAndFixPlayButton()
    {
        if (instance != null)
        {
            instance.DoHardReset();
        }
        else
        {
            Debug.LogError("PersistentResetManager instance not found!");
        }
    }

    private void DoHardReset()
    {
        if (debugMode)
            Debug.Log("Starting hard reset...");

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoadedAfterReset;

        // Reload current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private void OnSceneLoadedAfterReset(Scene scene, LoadSceneMode mode)
    {
        if (debugMode)
            Debug.Log("Scene reloaded, now fixing play button...");

        // Unsubscribe to prevent multiple calls
        SceneManager.sceneLoaded -= OnSceneLoadedAfterReset;

        // Fix the play button after everything initializes
        StartCoroutine(FixPlayButtonAfterFrame());
    }

    private IEnumerator FixPlayButtonAfterFrame()
    {
        // Wait one frame for all Start() methods to run
        yield return null;

        if (debugMode)
            Debug.Log("Looking for UIButtonController...");

        // Find the UI button controller that manages the play/pause button
        UIButtonController uiController = FindFirstObjectByType<UIButtonController>();

        if (uiController != null)
        {
            if (debugMode)
                Debug.Log("Found UIButtonController, checking GameController...");

            // Get the GameController instance
            GameController gameController = GameController.Instance;

            if (gameController != null)
            {
                if (debugMode)
                    Debug.Log($"Found GameController. Current state - isGamePaused: {gameController.IsGamePaused()}");

                // If the game is not paused, pause it
                if (!gameController.IsGamePaused())
                {
                    if (debugMode)
                        Debug.Log("Game is not paused, calling PauseGame to pause it");

                    gameController.PauseGame();
                }
                else
                {
                    if (debugMode)
                        Debug.Log("Game is already paused, no action needed");
                }
            }
            else
            {
                Debug.LogError("GameController instance not found!");
            }
        }
        else
        {
            Debug.LogError("UIButtonController not found after scene reload!");
        }
    }

    /// <summary>
    /// Get the singleton instance (useful for debugging)
    /// </summary>
    public static PersistentResetManager GetInstance()
    {
        return instance;
    }

    /// <summary>
    /// Check if the manager exists
    /// </summary>
    public static bool Exists()
    {
        return instance != null;
    }
}