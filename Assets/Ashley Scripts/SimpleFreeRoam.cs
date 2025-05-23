using UnityEngine;

public class SimpleFreeRoam : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float fastMoveSpeed = 30f;
    public float mouseSensitivity = 2f;
    public float smoothTime = 0.1f;
    public KeyCode fastMoveKey = KeyCode.LeftShift;

    private Camera cam;
    private Vector3 currentVelocity;
    private float pitch = 0f;
    private float yaw = 0f;
    private bool wasJustEnabled = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void OnEnable()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotation when enabled
        Vector3 euler = transform.eulerAngles;
        pitch = euler.x;
        yaw = euler.y;
        wasJustEnabled = true;
    }

    void OnDisable()
    {
        // Restore cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Skip one frame after being enabled to avoid jumps
        if (wasJustEnabled)
        {
            wasJustEnabled = false;
            return;
        }

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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float upDown = 0f;

        if (Input.GetKey(KeyCode.E)) upDown = 1f;
        if (Input.GetKey(KeyCode.Q)) upDown = -1f;

        Vector3 moveDir = transform.right * horizontal +
                         transform.forward * vertical +
                         transform.up * upDown;
        moveDir.Normalize();

        float currentSpeed = Input.GetKey(fastMoveKey) ? fastMoveSpeed : moveSpeed;
        Vector3 targetVelocity = moveDir * currentSpeed;

        Vector3 smoothVelocity = Vector3.SmoothDamp(
            currentVelocity,
            targetVelocity,
            ref currentVelocity,
            smoothTime
        );

        transform.position += smoothVelocity * Time.deltaTime;
    }

    void OnGUI()
    {
        if (cam != null && cam.enabled)
        {
            int y = 10;
            int lineHeight = 20;
            GUI.Label(new Rect(10, y, 300, lineHeight), "Free Roam Mode");
            y += lineHeight;
            GUI.Label(new Rect(10, y, 300, lineHeight), "WASD - Move");
            y += lineHeight;
            GUI.Label(new Rect(10, y, 300, lineHeight), "Mouse - Look");
            y += lineHeight;
            GUI.Label(new Rect(10, y, 300, lineHeight), "Q/E - Down/Up");
            y += lineHeight;
            GUI.Label(new Rect(10, y, 300, lineHeight), "Shift - Fast Move");
        }
    }
}