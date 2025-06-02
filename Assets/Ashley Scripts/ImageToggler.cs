using UnityEngine;
using UnityEngine.UI;

public class ImageToggler : MonoBehaviour
{
    [Header("Objects to Toggle")]
    public Image targetImage;
    public Button targetButton;

    [Header("Toggle Options")]
    public bool startVisible = false;

    void Start()
    {
        // Sets initial visibility for both objects
        if (targetImage != null)
            targetImage.enabled = startVisible;

        if (targetButton != null)
            targetButton.gameObject.SetActive(startVisible);
    }

    // Call this method from our button's OnClick event
    public void ToggleObjects()
    {
        bool newState = !IsAnyVisible();

        if (targetImage != null)
            targetImage.enabled = newState;

        if (targetButton != null)
            targetButton.gameObject.SetActive(newState);
    }

    // Additional methods for more control
    public void ShowObjects()
    {
        if (targetImage != null)
            targetImage.enabled = true;

        if (targetButton != null)
            targetButton.gameObject.SetActive(true);
    }

    public void HideObjects()
    {
        if (targetImage != null)
            targetImage.enabled = false;

        if (targetButton != null)
            targetButton.gameObject.SetActive(false);
    }

    public bool IsAnyVisible()
    {
        bool imageVisible = targetImage != null && targetImage.enabled;
        bool buttonVisible = targetButton != null && targetButton.gameObject.activeSelf;

        return imageVisible || buttonVisible;
    }
}