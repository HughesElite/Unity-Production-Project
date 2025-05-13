using UnityEngine;

public class ResetSceneButton : MonoBehaviour
{
    // Reference to your GameControl script
    private GameControl gameControl;

    private void Start()
    {
        // Find the GameControl in the scene
        gameControl = FindObjectOfType<GameControl>();

        if (gameControl == null)
        {
            Debug.LogWarning("No GameControl found in scene. Reset button won't work.");
        }
    }

    // This function will be called when the button is clicked
    public void ResetScene()
    {
        if (gameControl != null)
        {
            gameControl.ResetScene();
        }
    }
}