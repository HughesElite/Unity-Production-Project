using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneReset : MonoBehaviour
{
    [Header("Reset Options")]
    public bool clearMemoryBeforeReset = false; // Can help with performance
    public bool pauseAfterReset = true; // Pause the game after scene reloads

    void Start()
    {
        // Check if we should pause after a reset
        if (PlayerPrefs.GetInt("PauseAfterReset", 0) == 1)
        {
            // Clear the flag
            PlayerPrefs.SetInt("PauseAfterReset", 0);

            // Pause the game
            GameController gameController = GameController.Instance;
            if (gameController != null)
            {
                gameController.PauseGame();
            }
            else
            {
                Time.timeScale = 0f; // Fallback pause
            }

            Debug.Log("Game paused after scene reset");
        }
    }

    // Call this method from button OnClick events
    public void ResetScene()
    {
        // Set flag to pause after reset if enabled
        if (pauseAfterReset)
        {
            PlayerPrefs.SetInt("PauseAfterReset", 1);
        }

        if (clearMemoryBeforeReset)
        {
            // Force garbage collection before reset
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Faster async version (optional)
    public void ResetSceneAsync()
    {
        // Set flag to pause after reset if enabled
        if (pauseAfterReset)
        {
            PlayerPrefs.SetInt("PauseAfterReset", 1);
        }

        if (clearMemoryBeforeReset)
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
}