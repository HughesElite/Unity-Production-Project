using UnityEngine;
using UnityEngine.SceneManagement; // If you want to stop/reload the scene

public class GameControl : MonoBehaviour
{
    private bool isGamePaused = false; // Track whether the game is paused or not

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
}
