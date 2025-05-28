using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MouseFadeUI : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup uiCanvasGroup; // Assign your button container

    [Header("Mouse Area Settings")]
    public float topLeftWidth = 300f; // Width of the trigger area from left edge
    public float topLeftHeight = 200f; // Height of the trigger area from top edge

    [Header("Fade Settings")]
    public float fadeDuration = 0.3f; // How long fade takes
    public float visibleAlpha = 1f; // Fully visible
    public float hiddenAlpha = 0.1f; // Almost invisible when hidden

    [Header("First Click Behavior")]
    public bool waitForFirstClick = true; // Stay visible until first button click

    private bool isMouseInArea = false;
    private bool isUIVisible = true;
    private bool hasBeenClicked = false;
    private Coroutine fadeCoroutine;
    private Button[] allButtons;

    private void Start()
    {
        if (uiCanvasGroup == null)
            uiCanvasGroup = GetComponent<CanvasGroup>();

        // Always start visible
        uiCanvasGroup.alpha = visibleAlpha;
        isUIVisible = true;
        hasBeenClicked = false;

        // Find all buttons in this UI group and add listeners
        if (waitForFirstClick)
        {
            SetupButtonListeners();
        }
    }

    private void SetupButtonListeners()
    {
        // Find all buttons that are children of this GameObject
        allButtons = GetComponentsInChildren<Button>();

        foreach (Button button in allButtons)
        {
            // Add a listener to detect when any button is clicked
            button.onClick.AddListener(OnAnyButtonClicked);
        }
    }

    private void OnAnyButtonClicked()
    {
        if (waitForFirstClick && !hasBeenClicked)
        {
            hasBeenClicked = true;
            Debug.Log("First button clicked - UI fade behavior now active");

            // Remove the click listeners since we don't need them anymore
            foreach (Button button in allButtons)
            {
                button.onClick.RemoveListener(OnAnyButtonClicked);
            }

            // Immediately check mouse position after first click
            CheckMousePosition();
        }
    }

    private void Update()
    {
        // Only check mouse position after first button click (if enabled)
        if (waitForFirstClick && !hasBeenClicked)
            return;

        CheckMousePosition();
    }

    private void CheckMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;

        // Check if mouse is in the top-left area
        bool mouseInTopLeft = mousePos.x <= topLeftWidth &&
                             mousePos.y >= (Screen.height - topLeftHeight);

        // If mouse state changed, trigger fade
        if (mouseInTopLeft != isMouseInArea)
        {
            isMouseInArea = mouseInTopLeft;

            if (isMouseInArea && !isUIVisible)
            {
                ShowUI();
            }
            else if (!isMouseInArea && isUIVisible)
            {
                HideUI();
            }
        }
    }

    private void ShowUI()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToAlpha(visibleAlpha));
        isUIVisible = true;
    }

    private void HideUI()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToAlpha(hiddenAlpha));
        isUIVisible = false;
    }

    private IEnumerator FadeToAlpha(float targetAlpha)
    {
        float startAlpha = uiCanvasGroup.alpha;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time so it works when paused
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            uiCanvasGroup.alpha = currentAlpha;
            yield return null;
        }

        uiCanvasGroup.alpha = targetAlpha;
    }

    // Reset the first click state (useful if you want to reset the behavior)
    public void ResetFirstClickState()
    {
        hasBeenClicked = false;
        ShowUI(); // Show UI again

        if (waitForFirstClick)
        {
            SetupButtonListeners();
        }
    }

    // Optional: Draw the trigger area in Scene view for debugging
    private void OnDrawGizmos()
    {
        // This shows the trigger area in the Scene view
        Gizmos.color = Color.yellow;

        // Convert screen coordinates to world coordinates for visualization
        if (Camera.main != null)
        {
            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height - topLeftHeight, Camera.main.nearClipPlane));
            Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(topLeftWidth, Screen.height, Camera.main.nearClipPlane));

            // Draw a wireframe rectangle showing the trigger area
            Vector3 topLeft = new Vector3(bottomLeft.x, topRight.y, bottomLeft.z);
            Vector3 bottomRight = new Vector3(topRight.x, bottomLeft.y, bottomLeft.z);

            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
        }
    }
}