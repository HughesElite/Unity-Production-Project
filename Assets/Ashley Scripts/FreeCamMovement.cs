using UnityEngine;

public class FreeRoamMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float fastMoveSpeed = 30f;
    public float mouseSensitivity = 0.01f;

    [Header("Movement Style")]
    public bool useSmoothing = true;
    public float smoothTime = 0.1f;

    [Header("Controls")]
    public KeyCode fastMoveKey = KeyCode.LeftShift;
    public KeyCode upKey = KeyCode.E;
    public KeyCode downKey = KeyCode.Q;
    public KeyCode exitKey = KeyCode.Tab;

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
        Vector3 rot = transform.eulerAngles;
        pitch = rot.x;
        yaw = rot.y;
    }

    void OnEnable()
    {
        Vector3 rot = transform.eulerAngles;
        pitch = rot.x;
        yaw = rot.y;
        wasJustEnabled = true;
    }

    void Update()
    {
        // Skip first frame to avoid jumps
        if (wasJustEnabled)
        {
            wasJustEnabled = false;
            return;
        }

        if (cam == null || !cam.enabled) return;

        if (Input.GetKeyDown(exitKey))
        {
            ExitFreeRoam();
            return;
        }

        HandleMouseLook();
        HandleMovement();
    }

    void ExitFreeRoam()
    {
        FreeRoamCameraController controller = FindFirstObjectByType<FreeRoamCameraController>();
        if (controller != null)
        {
            controller.ExitFreeRoam();
        }
        else
        {
            cam.enabled = false;
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
        float horizontal = 0f;
        float vertical = 0f;
        float upDown = 0f;

        // WASD movement
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical = -1f;

        if (Input.GetKey(upKey)) upDown = 1f;
        if (Input.GetKey(downKey)) upDown = -1f;

        Vector3 moveDir = transform.right * horizontal +
                         transform.forward * vertical +
                         transform.up * upDown;

        // Normalise to prevent faster diagonal movement
        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        float currentSpeed = Input.GetKey(fastMoveKey) ? fastMoveSpeed : moveSpeed;

        if (useSmoothing)
        {
            Vector3 targetVelocity = moveDir * currentSpeed;

            // Custom smoothing using unscaled time
            float smoothFactor = 1f - Mathf.Exp(-1f / smoothTime * Time.unscaledDeltaTime);
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, smoothFactor);

            transform.position += currentVelocity * Time.unscaledDeltaTime;
        }
        else
        {
            Vector3 movement = moveDir * currentSpeed * Time.unscaledDeltaTime;
            transform.position += movement;
        }
    }
}