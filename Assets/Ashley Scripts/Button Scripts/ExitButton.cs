using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void ExitGame()
    {
        
        Application.Quit();

        // If running in the Unity Editor, stop play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
