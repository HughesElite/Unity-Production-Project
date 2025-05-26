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

    [Header("UI Instructions (Optional)")]
    public Canvas instructionsCanvas;
    public TextMeshProUGUI instructionsText;
    public bool showInstructions = true;

    private Camera cam;
    private float pitch = 0f;
    private float yaw = 0f;
    private Vector3 currentVelocity = Vector3.zero;
    private bool wasJustEnabled = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        // Initialize rotation based on current camera rotation
        Vector3 rot = transform.eulerAngles;
        pitch = rot.x;
        yaw = rot.y;
    }

    void OnEnable()
    {
        // Reset rotation to current camera rotation
        Vector3 rot = transform.eulerAngles;
        pitch = rot.x;
        yaw = rot.y;
        wasJustEnabled = true;

        // Update cursor and UI
        UpdateCursorState();
        UpdateInstructions();
    }

    void OnDisable()
    {
        // Always unlock cursor when script is disabled
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hide instructions
        if (instructionsCanvas != null)
            instructionsCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        // Skip one frame after being enabled to avoid jumps
        if (wasJustEnabled)
        {
            wasJustEnabled = false;
            return;
        }

        // Update cursor state based on camera status
        UpdateCursorState();

        // Only handle input if camera is enabled
        if (cam != null && cam.enabled)
        {
            HandleMouseLook();
            HandleMovement();
        }
    }

    void UpdateCursorState()
    {
        // Only lock cursor if this camera is enabled and active
        if (cam != null && cam.enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void UpdateInstructions()
    {
        if (!showInstructions) return;

        bool shouldShow = cam != null && cam.enabled;

        if (instructionsCanvas != null)
        {
            instructionsCanvas.gameObject.SetActive(shouldShow);
        }

        if (instructionsText != null && shouldShow)
        {
            instructionsText.text = $"Free Roam Camera\n" +
                                  $"WASD - Move\n" +
                                  $"Mouse - Look Around\n" +
                                  $"{upKey}/{downKey} - Up/Down\n" +
                                  $"{fastMoveKey} - Fast Move\n" +
                                  $"Movement: {(useSmoothing ? "Smooth" : "Direct")}";
        }
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

    // Public methods for external control
    public void SetMovementSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void SetMouseSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }

    public void ToggleSmoothing()
    {
        useSmoothing = !useSmoothing;
        if (!useSmoothing)
            currentVelocity = Vector3.zero; // Reset velocity when switching to direct
        UpdateInstructions();
    }

    public void ResetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
        currentVelocity = Vector3.zero;
    }

    public void ResetRotation(Vector3 newRotation)
    {
        transform.eulerAngles = newRotation;
        pitch = newRotation.x;
        yaw = newRotation.y;
    }
}