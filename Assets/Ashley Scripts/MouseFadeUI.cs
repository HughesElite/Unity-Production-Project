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
    [Tooltip("Disable button interactions when UI is faded")]
    public bool disableButtonsWhenFaded = true; // NEW: Option to disable buttons

    [Header("First Click Behavior")]
    public bool waitForFirstClick = true; // Stay visible until first button click
    [Tooltip("Time to wait after button click before fade behavior starts")]
    public float delayAfterButtonClick = 1f; // NEW: Delay after button click

    [Header("Inactivity Settings")]
    public bool fadeOnInactivity = true; // Fade after mouse stops moving
    [Tooltip("Time of no mouse movement before UI fades")]
    public float inactivityFadeDelay = 2f; // RENAMED: Clearer name for mouse inactivity

    private bool isMouseInArea = false;
    private bool isUIVisible = true;
    private bool hasBeenClicked = false;
    private bool isFadedFromInactivity = false;
    private bool fadeSystemActive = false; // NEW: Track if fade system is active
    private Coroutine fadeCoroutine;
    private Coroutine inactivityCoroutine;
    private Coroutine buttonClickDelayCoroutine; // NEW: For button click delay
    private Button[] allButtons;
    private Vector3 lastMousePosition;

    private void Start()
    {
        if (uiCanvasGroup == null)
            uiCanvasGroup = GetComponent<CanvasGroup>();

        // Always start visible and interactable
        uiCanvasGroup.alpha = visibleAlpha;
        uiCanvasGroup.interactable = true;
        uiCanvasGroup.blocksRaycasts = true;
        isUIVisible = true;
        hasBeenClicked = false;
        isFadedFromInactivity = false;
        fadeSystemActive = false;
        lastMousePosition = Input.mousePosition;

        // Find all buttons in this UI group and add listeners
        if (waitForFirstClick)
        {
            SetupButtonListeners();
        }
        else
        {
            // If not waiting for first click, activate fade system immediately
            fadeSystemActive = true;
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
            Debug.Log($"First button clicked - UI fade behavior will start in {delayAfterButtonClick} seconds");

            // Remove the click listeners since we don't need them anymore
            foreach (Button button in allButtons)
            {
                button.onClick.RemoveListener(OnAnyButtonClicked);
            }

            // Start delay before activating fade system
            if (buttonClickDelayCoroutine != null)
                StopCoroutine(buttonClickDelayCoroutine);

            buttonClickDelayCoroutine = StartCoroutine(ButtonClickDelayTimer());
        }
    }

    // NEW: Coroutine to handle delay after button click
    private IEnumerator ButtonClickDelayTimer()
    {
        yield return new WaitForSecondsRealtime(delayAfterButtonClick);

        // Activate the fade system
        fadeSystemActive = true;
        Debug.Log("UI fade behavior now active");

        // Immediately check mouse position and start inactivity timer
        CheckMousePosition();
        if (fadeOnInactivity)
        {
            RestartInactivityTimer();
        }
    }

    private void Update()
    {
        // Only proceed if fade system is active
        if (!fadeSystemActive)
            return;

        // Check for mouse movement
        Vector3 currentMousePosition = Input.mousePosition;
        bool mouseHasMoved = Vector3.Distance(currentMousePosition, lastMousePosition) > 1f; // Small threshold to ignore tiny movements

        if (mouseHasMoved)
        {
            lastMousePosition = currentMousePosition;

            // If UI was faded from inactivity, show it again when mouse moves
            if (isFadedFromInactivity)
            {
                isFadedFromInactivity = false;
                ShowUI();
            }

            // Restart inactivity timer
            if (fadeOnInactivity)
            {
                RestartInactivityTimer();
            }
        }

        // Normal mouse area checking (unless faded from inactivity)
        if (!isFadedFromInactivity)
        {
            CheckMousePosition();
        }
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

        // Enable button interactions when showing UI
        if (disableButtonsWhenFaded)
        {
            uiCanvasGroup.interactable = true;
            uiCanvasGroup.blocksRaycasts = true;
        }
    }

    private void HideUI()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToAlpha(hiddenAlpha));
        isUIVisible = false;

        // Disable button interactions when hiding UI
        if (disableButtonsWhenFaded)
        {
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }
    }

    private void RestartInactivityTimer()
    {
        // Stop existing inactivity timer
        if (inactivityCoroutine != null)
            StopCoroutine(inactivityCoroutine);

        // Start new inactivity timer
        inactivityCoroutine = StartCoroutine(InactivityTimer());
    }

    private IEnumerator InactivityTimer()
    {
        yield return new WaitForSecondsRealtime(inactivityFadeDelay); // UPDATED: Use new variable name

        // After delay, fade UI due to inactivity
        if (isUIVisible)
        {
            isFadedFromInactivity = true;
            HideUI();
            Debug.Log($"UI faded due to {inactivityFadeDelay} seconds of mouse inactivity");
        }
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
        isFadedFromInactivity = false;
        fadeSystemActive = false;
        ShowUI(); // Show UI again (this will also enable interactions if needed)

        // Stop all timers
        if (inactivityCoroutine != null)
        {
            StopCoroutine(inactivityCoroutine);
            inactivityCoroutine = null;
        }

        if (buttonClickDelayCoroutine != null)
        {
            StopCoroutine(buttonClickDelayCoroutine);
            buttonClickDelayCoroutine = null;
        }

        if (waitForFirstClick)
        {
            SetupButtonListeners();
        }
        else
        {
            fadeSystemActive = true;
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