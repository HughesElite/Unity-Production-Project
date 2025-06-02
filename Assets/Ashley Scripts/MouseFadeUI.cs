using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MouseFadeUI : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup uiCanvasGroup;

    [Header("Mouse Area Settings")]
    public float topLeftWidth = 300f;
    public float topLeftHeight = 200f;

    [Header("Fade Settings")]
    public float fadeDuration = 0.3f;
    public float visibleAlpha = 1f;
    public float hiddenAlpha = 0.1f;
    [Tooltip("Disable button interactions when UI is faded")]
    public bool disableButtonsWhenFaded = true;

    [Header("First Click Behavior")]
    public bool waitForFirstClick = true;
    [Tooltip("Time to wait after button click before fade behavior starts")]
    public float delayAfterButtonClick = 1f;

    [Header("Inactivity Settings")]
    public bool fadeOnInactivity = true;
    [Tooltip("Time of no mouse movement before UI fades")]
    public float inactivityFadeDelay = 2f;

    private bool isMouseInArea = false;
    private bool isUIVisible = true;
    private bool hasBeenClicked = false;
    private bool isFadedFromInactivity = false;
    private bool fadeSystemActive = false;
    private Coroutine fadeCoroutine;
    private Coroutine inactivityCoroutine;
    private Coroutine buttonClickDelayCoroutine;
    private Button[] allButtons;
    private Vector3 lastMousePosition;

    private void Start()
    {
        if (uiCanvasGroup == null)
            uiCanvasGroup = GetComponent<CanvasGroup>();

        // Initialise UI as fully visible and functional
        uiCanvasGroup.alpha = visibleAlpha;
        uiCanvasGroup.interactable = true;
        uiCanvasGroup.blocksRaycasts = true;
        isUIVisible = true;
        hasBeenClicked = false;
        isFadedFromInactivity = false;
        fadeSystemActive = false;
        lastMousePosition = Input.mousePosition;

        // Either wait for first button click or start fade system immediately
        if (waitForFirstClick)
        {
            SetupButtonListeners();
        }
        else
        {
            fadeSystemActive = true;
        }
    }

    private void SetupButtonListeners()
    {
        // Find all buttons and add click detection for first-time activation
        allButtons = GetComponentsInChildren<Button>();

        foreach (Button button in allButtons)
        {
            button.onClick.AddListener(OnAnyButtonClicked);
        }
    }

    private void OnAnyButtonClicked()
    {
        if (waitForFirstClick && !hasBeenClicked)
        {
            hasBeenClicked = true;

            // Clean up listeners - fade system will take over
            foreach (Button button in allButtons)
            {
                button.onClick.RemoveListener(OnAnyButtonClicked);
            }

            // Start delay timer before enabling fade behavior
            if (buttonClickDelayCoroutine != null)
                StopCoroutine(buttonClickDelayCoroutine);

            buttonClickDelayCoroutine = StartCoroutine(ButtonClickDelayTimer());
        }
    }

    private IEnumerator ButtonClickDelayTimer()
    {
        yield return new WaitForSecondsRealtime(delayAfterButtonClick);

        // Activate the fade system after delay
        fadeSystemActive = true;

        // Start monitoring mouse position and inactivity
        CheckMousePosition();
        if (fadeOnInactivity)
        {
            RestartInactivityTimer();
        }
    }

    private void Update()
    {
        // Only runs fade logic if system has been activated
        if (!fadeSystemActive)
            return;

        // Tracks mouse movement for inactivity detection
        Vector3 currentMousePosition = Input.mousePosition;
        bool mouseHasMoved = Vector3.Distance(currentMousePosition, lastMousePosition) > 1f;

        if (mouseHasMoved)
        {
            lastMousePosition = currentMousePosition;

            // Restores UI if it was hidden due to inactivity
            if (isFadedFromInactivity)
            {
                isFadedFromInactivity = false;
                ShowUI();
            }

            // Resets inactivity timer on mouse movement
            if (fadeOnInactivity)
            {
                RestartInactivityTimer();
            }
        }

        // Check if mouse is in trigger area (unless UI is hidden from inactivity)
        if (!isFadedFromInactivity)
        {
            CheckMousePosition();
        }
    }

    private void CheckMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;

        // Check if mouse is in top-left trigger area
        bool mouseInTopLeft = mousePos.x <= topLeftWidth &&
                             mousePos.y >= (Screen.height - topLeftHeight);

        // Trigger fade transition when mouse enters/exits area
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
        // Stop any ongoing fade and start fade to visible
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToAlpha(visibleAlpha));
        isUIVisible = true;

        // Re-enable button interactions when showing
        if (disableButtonsWhenFaded)
        {
            uiCanvasGroup.interactable = true;
            uiCanvasGroup.blocksRaycasts = true;
        }
    }

    private void HideUI()
    {
        // Stop any ongoing fade and start fade to hidden
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToAlpha(hiddenAlpha));
        isUIVisible = false;

        // Disable button interactions when hiding
        if (disableButtonsWhenFaded)
        {
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }
    }

    private void RestartInactivityTimer()
    {
        // Reset the inactivity countdown
        if (inactivityCoroutine != null)
            StopCoroutine(inactivityCoroutine);

        inactivityCoroutine = StartCoroutine(InactivityTimer());
    }

    private IEnumerator InactivityTimer()
    {
        // Wait for specified inactivity period
        yield return new WaitForSecondsRealtime(inactivityFadeDelay);

        // Hide UI if still visible after inactivity period
        if (isUIVisible)
        {
            isFadedFromInactivity = true;
            HideUI();
        }
    }

    private IEnumerator FadeToAlpha(float targetAlpha)
    {
        // Smooth fade transition between current and target alpha
        float startAlpha = uiCanvasGroup.alpha;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Works even when simulation is paused
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            uiCanvasGroup.alpha = currentAlpha;
            yield return null;
        }

        uiCanvasGroup.alpha = targetAlpha;
    }

    // Reset behavior for reuse
    public void ResetFirstClickState()
    {
        hasBeenClicked = false;
        isFadedFromInactivity = false;
        fadeSystemActive = false;
        ShowUI(); // Restore UI to visible state

        // Stop all running timers
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

        // Set up for next use
        if (waitForFirstClick)
        {
            SetupButtonListeners();
        }
        else
        {
            fadeSystemActive = true;
        }
    }

    // Show trigger area in Scene view for debugging positioning
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (Camera.main != null)
        {
            // Convert screen space trigger area to world coordinates for visualisation
            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height - topLeftHeight, Camera.main.nearClipPlane));
            Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(topLeftWidth, Screen.height, Camera.main.nearClipPlane));

            Vector3 topLeft = new Vector3(bottomLeft.x, topRight.y, bottomLeft.z);
            Vector3 bottomRight = new Vector3(topRight.x, bottomLeft.y, bottomLeft.z);

            // Draw rectangle outline
            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
        }
    }
}