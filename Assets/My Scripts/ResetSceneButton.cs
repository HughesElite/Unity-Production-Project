using UnityEngine;
using UnityEngine.SceneManagement; // Import the SceneManager namespace

public class ResetSceneButton : MonoBehaviour
{
    // This function will be called when the button is clicked
    public void ResetScene()
    {
        // Get the current scene's name and reload it
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
