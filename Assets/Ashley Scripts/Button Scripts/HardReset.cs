using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    private Button button;

    public void Start()
    {
        button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(ResetScene);
    }

    public void ResetScene()
    {
        // Use the persistent manager to handle the reset
        PersistentResetManager.HardResetAndFixPlayButton();
    }
}