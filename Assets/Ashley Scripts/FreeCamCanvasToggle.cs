using UnityEngine;

public class CanvasToggle : MonoBehaviour
{
    [Header("Canvas to Control")]
    public Canvas targetCanvas;

    [Header("Options")]
    public bool startHidden = true;
    public KeyCode hideKey = KeyCode.Tab; // Key to hide the canvas

    void Start()
    {
        // Set initial state
        if (targetCanvas != null && startHidden)
        {
            targetCanvas.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Check for hide key press
        if (Input.GetKeyDown(hideKey))
        {
            HideCanvas();
        }
    }

    // Call this from your button
    public void ShowCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(true);
            Debug.Log("Canvas shown");
        }
    }

    // Call this to hide the canvas
    public void HideCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(false);
            Debug.Log("Canvas hidden");
        }
    }

    // Call this to toggle the canvas on/off
    public void ToggleCanvas()
    {
        if (targetCanvas != null)
        {
            bool isActive = targetCanvas.gameObject.activeSelf;
            targetCanvas.gameObject.SetActive(!isActive);

            Debug.Log($"Canvas {(isActive ? "hidden" : "shown")}");
        }
    }
}