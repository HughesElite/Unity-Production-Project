using UnityEngine;

public class FreeRoamCameraController : MonoBehaviour
{
    [Header("References")]
    public SimpleCameraSwitcher cameraSwitcher;
    public Camera freeRoamCamera; // Direct reference instead of index

    [Header("Cursor Control")]
    public bool lockCursorInFreeRoam = true;

    private int previousCameraIndex = 0; // Remember which camera was active before free roam

    void Start()
    {
        // Ensure free roam camera starts disabled
        if (freeRoamCamera != null)
        {
            freeRoamCamera.enabled = false;
        }

        Debug.Log("FreeRoamCameraController: Free roam camera disabled at start");
    }

    void Update()
    {
        // Check if we're currently on the free roam camera
        bool isOnFreeRoamCamera = (freeRoamCamera != null && freeRoamCamera.enabled);

        // Manage cursor based on current camera
        if (isOnFreeRoamCamera && lockCursorInFreeRoam)
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

    // Call this from your "Free Cam" button
    public void SwitchToFreeRoam()
    {
        if (cameraSwitcher != null && freeRoamCamera != null)
        {
            // Remember which camera was active
            previousCameraIndex = cameraSwitcher.GetCurrentCameraIndex();

            // Disable current camera from switcher
            Camera currentCamera = cameraSwitcher.GetCurrentCameraObject();
            if (currentCamera != null)
            {
                currentCamera.enabled = false;
            }

            // Enable free roam camera
            freeRoamCamera.enabled = true;

            Debug.Log("Switched to Free Roam Camera");
        }
    }

    // Call this to exit free roam (go back to previous camera)
    public void ExitFreeRoam()
    {
        if (cameraSwitcher != null && freeRoamCamera != null)
        {
            // Disable free roam camera
            freeRoamCamera.enabled = false;

            // Go back to the camera that was active before
            cameraSwitcher.SwitchToCamera(previousCameraIndex);

            Debug.Log("Exited Free Roam Camera");
        }
    }
}