using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FreeRoamCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float fastMoveSpeed = 30f;
    public float mouseSensitivity = 2f;

    [Header("Movement Style")]
    public bool useSmoothing = true;
    public float smoothTime = 0.1f;

    [Header("Controls")]
    public KeyCode fastMoveKey = KeyCode.LeftShift;
    public KeyCode upKey = KeyCode.E;
    public KeyCode downKey = KeyCode.Q;
    public KeyCode exitKey = KeyCode.Tab; // Changed from Escape to avoid conflict

    [Header("UI Button (Optional)")]
    public Button toggleButton;
    public TextMeshProUGUI buttonText;
    public string enabledText = "Exit Free Cam";
    public string disabledText = "Free Cam Mode";

    private Camera cam;
    private float pitch = 0f;
    private float yaw = 0f;
    private Vector3 currentVelocity = Vector3.zero;
    private bool isActive = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        // Set up button if provided
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleFreeRoam);

        // Initialize rotation
        Vector3 rot = transform.eulerAngles;
        pitch = rot.x;
        yaw = rot.y;

        // Force start disabled and ensure cursor is unlocked
        isActive = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateButtonText();

        Debug.Log("FreeRoamCamera: Started DISABLED - click button to activate");
    }

    void Update()
    {
        // ONLY process input if free roam is explicitly active
        if (!isActive) return;

        // Check for exit key
        if (Input.GetKeyDown(exitKey))
        {
            SetFreeRoamActive(false);
            return;
        }

        // Handle movement and mouse look
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float upDown = 0f;

        if (Input.GetKey(upKey)) upDown = 1f;
        if (Input.GetKey(downKey)) upDown = -1f;

        // Calculate movement direction
        Vector3 moveDir = transform.right * horizontal +
                         transform.forward * vertical +
                         transform.up * upDown;

        // Normalize to prevent faster diagonal movement
        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        // Apply speed
        float currentSpeed = Input.GetKey(fastMoveKey) ? fastMoveSpeed : moveSpeed;

        if (useSmoothing)
        {
            // Smooth movement
            Vector3 targetVelocity = moveDir * currentSpeed;
            Vector3 smoothVelocity = Vector3.SmoothDamp(
                currentVelocity,
                targetVelocity,
                ref currentVelocity,
                smoothTime
            );
            transform.position += smoothVelocity * Time.deltaTime;
        }
        else
        {
            // Direct movement
            Vector3 movement = moveDir * currentSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }

    // Main toggle method - call this from your button
    public void ToggleFreeRoam()
    {
        SetFreeRoamActive(!isActive);
    }

    // Enable/disable free roam mode
    public void SetFreeRoamActive(bool active)
    {
        isActive = active;

        if (isActive)
        {
            // Enable free roam movement (but leave camera component alone)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Reset rotation tracking
            Vector3 rot = transform.eulerAngles;
            pitch = rot.x;
            yaw = rot.y;

            Debug.Log("Free Roam Camera Enabled - WASD to move, Mouse to look, Shift for speed, ESC to exit");
        }
        else
        {
            // Disable free roam movement (but leave camera component alone)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            currentVelocity = Vector3.zero;

            Debug.Log("Free Roam Camera Disabled");
        }

        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = isActive ? enabledText : disabledText;
        }
    }

    // Public methods for external control
    public bool IsActive() => isActive;

    public void EnableFreeRoam() => SetFreeRoamActive(true);

    public void DisableFreeRoam() => SetFreeRoamActive(false);
}