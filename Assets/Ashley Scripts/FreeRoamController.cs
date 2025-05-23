using UnityEngine;

public class FreeRoamController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float fastMoveSpeed = 30f;
    public float mouseSensitivity = 2f;

    [Header("Controls")]
    public KeyCode fastMoveKey = KeyCode.LeftShift;

    private Camera cam;
    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Initialize rotation based on current camera rotation
        Vector3 rot = transform.eulerAngles;
        rotationX = rot.x;
        rotationY = rot.y;
    }

    void OnEnable()
    {
        // When this camera becomes active, lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reset rotation to current camera rotation
        Vector3 rot = transform.eulerAngles;
        rotationX = rot.x;
        rotationY = rot.y;
    }

    void OnDisable()
    {
        // When switching away from this camera, unlock the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Only process input if this camera is active
        if (!cam.enabled) return;

        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        // Get mouse movement
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Update rotation
        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        // Apply rotation
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    void HandleMovement()
    {
        // Get input axes
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float upDown = 0f;

        // Vertical movement
        if (Input.GetKey(KeyCode.E)) upDown = 1f;
        if (Input.GetKey(KeyCode.Q)) upDown = -1f;

        // Calculate movement direction
        Vector3 movement = new Vector3(horizontal, upDown, vertical);
        movement = transform.TransformDirection(movement);

        // Apply speed
        float currentSpeed = Input.GetKey(fastMoveKey) ? fastMoveSpeed : moveSpeed;
        movement *= currentSpeed * Time.deltaTime;

        // Move the camera
        transform.position += movement;
    }

    void OnGUI()
    {
        // Only show controls when this camera is active
        if (cam.enabled)
        {
            int y = 10;
            int lineHeight = 20;
            GUI.Label(new Rect(10, y, 400, lineHeight), "Free Roam Camera Controls:");
            y += lineHeight;
            GUI.Label(new Rect(10, y, 400, lineHeight), "WASD - Move horizontally");
            y += lineHeight;
            GUI.Label(new Rect(10, y, 400, lineHeight), "Q/E - Move down/up");
            y += lineHeight;
            GUI.Label(new Rect(10, y, 400, lineHeight), "Mouse - Look around");
            y += lineHeight;
            GUI.Label(new Rect(10, y, 400, lineHeight), "Shift - Move faster");
            y += lineHeight;
            GUI.Label(new Rect(10, y, 400, lineHeight), "C - Switch camera");
        }
    }
}