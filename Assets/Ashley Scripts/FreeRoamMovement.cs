using UnityEngine;

public class FreeRoamMovement : MonoBehaviour
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
        // Reset when this camera becomes active
        Vector3 rot = transform.eulerAngles;
        pitch = rot.x;
        yaw = rot.y;
        wasJustEnabled = true;

        Debug.Log("Free Roam Movement Active - WASD to move, Mouse to look, Shift for speed");
    }

    void Update()
    {
        // Skip first frame to avoid jumps
        if (wasJustEnabled)
        {
            wasJustEnabled = false;
            return;
        }

        // Only work if this camera is active
        if (cam == null || !cam.enabled) return;

        // Handle input
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
}