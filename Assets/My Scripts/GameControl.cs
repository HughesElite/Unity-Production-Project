using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour
{
    private bool isGamePaused = false; // Track whether the game is paused or not

    // Static property for scene reset state (accessible from any script)
    public static bool IsResetting { get; private set; } = false;

    // Call this method to start the game
    public void StartGame()
    {
        // Ensure the game is not paused
        Time.timeScale = 1f; // Resumes game time
        isGamePaused = false;
    }

    // Call this method to pause the game
    public void PauseGame()
    {
        if (!isGamePaused)
        {
            Time.timeScale = 0f; // Freezes game time
            isGamePaused = true;
        }
    }

    // Call this method to stop the game
    public void StopGame()
    {
        // Stop the game entirely, you could reload the scene if needed
        Time.timeScale = 0f; // Freeze game time
        // Optional: Reload the current scene (useful for restarting the game)
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // New method to reset the scene
    public void ResetScene()
    {
        // Set the static flag that we're resetting
        IsResetting = true;

        // Reset time scale in case the game was paused
        Time.timeScale = 1f;
        isGamePaused = false;

        // Reload the current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    // This should be called by InfectedCountDisplay when the scene starts
    public static void FinishReset()
    {
        IsResetting = false;
    }
}