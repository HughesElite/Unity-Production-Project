using UnityEngine;

public class SimpleCameraSwitcher : MonoBehaviour
{
    [Header("Camera Setup")]
    public Camera[] cameras; // Allows for as many cameras as needed

    [Header("Settings")]
    public bool startWithFirstCamera = true; // Start with cameras [0] enabled

    private int currentCameraIndex = 0;

    void Start()
    {
        // Validates the cameras array
        if (cameras == null || cameras.Length == 0)
        {
            Debug.LogError("SimpleCameraSwitcher: No cameras assigned! Please assign cameras in the inspector.");
            return;
        }

        // Removes any null cameras from the array
        System.Array.Resize(ref cameras, cameras.Length);
        cameras = System.Array.FindAll(cameras, camera => camera != null);

        if (cameras.Length == 0)
        {
            Debug.LogError("SimpleCameraSwitcher: All assigned cameras are null!");
            return;
        }

        // Sets the initial camera
        if (startWithFirstCamera)
        {
            currentCameraIndex = 0;
        }

        // Disables all cameras first
        DisableAllCameras();

        // Enables the starting camera
        if (currentCameraIndex < cameras.Length)
        {
            cameras[currentCameraIndex].enabled = true;
            Debug.Log($"Started with {GetCurrentCameraName()}");
        }
    }

    void Update()
    {
        // Check for C key press as this allows to switch between cameras also
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCamera();
        }
    }

    // Made this public so buttons can call this method
    public void SwitchCamera()
    {
        // Safety check
        if (cameras == null || cameras.Length == 0)
        {
            Debug.LogWarning("SimpleCameraSwitcher: No cameras to switch between!");
            return;
        }

        // If only one camera, no switching needed
        if (cameras.Length == 1)
        {
            Debug.Log("SimpleCameraSwitcher: Only one camera available, no switching needed.");
            return;
        }

        // Disables current camera
        if (currentCameraIndex < cameras.Length && cameras[currentCameraIndex] != null)
        {
            cameras[currentCameraIndex].enabled = false;
        }

        // Move to next camera (loop back to start)
        currentCameraIndex++;
        if (currentCameraIndex >= cameras.Length)
        {
            currentCameraIndex = 0;
        }

        // Enables new camera
        if (cameras[currentCameraIndex] != null)
        {
            cameras[currentCameraIndex].enabled = true;
            Debug.Log($"Switched to {GetCurrentCameraName()}");
        }
        else
        {
            Debug.LogWarning($"SimpleCameraSwitcher: Camera at index {currentCameraIndex} is null!");
            // Try to find next valid camera
            FindNextValidCamera();
        }
    }

    // Switch to a specific camera by index
    public void SwitchToCamera(int index)
    {
        if (cameras == null || index < 0 || index >= cameras.Length)
        {
            Debug.LogWarning($"SimpleCameraSwitcher: Invalid camera index {index}");
            return;
        }

        if (cameras[index] == null)
        {
            Debug.LogWarning($"SimpleCameraSwitcher: Camera at index {index} is null!");
            return;
        }

        // Disables current camera
        if (currentCameraIndex < cameras.Length && cameras[currentCameraIndex] != null)
        {
            cameras[currentCameraIndex].enabled = false;
        }

        // Switch to specified camera
        currentCameraIndex = index;
        cameras[currentCameraIndex].enabled = true;
        Debug.Log($"Switched to {GetCurrentCameraName()}");
    }

    private void DisableAllCameras()
    {
        foreach (Camera cam in cameras)
        {
            if (cam != null)
            {
                cam.enabled = false;
            }
        }
    }

    private void FindNextValidCamera()
    {
        int startIndex = currentCameraIndex;

        do
        {
            currentCameraIndex++;
            if (currentCameraIndex >= cameras.Length)
            {
                currentCameraIndex = 0;
            }

            if (cameras[currentCameraIndex] != null)
            {
                cameras[currentCameraIndex].enabled = true;
                Debug.Log($"Found valid camera: {GetCurrentCameraName()}");
                return;
            }

        } while (currentCameraIndex != startIndex);

        Debug.LogError("SimpleCameraSwitcher: No valid cameras found!");
    }

    // Get current camera info (used for UI updates)
    public int GetCurrentCameraIndex()
    {
        return currentCameraIndex;
    }

    public int GetCurrentCamera()
    {
        return currentCameraIndex + 1; // 1-based for user display
    }

    public string GetCurrentCameraName()
    {
        if (cameras != null && currentCameraIndex < cameras.Length && cameras[currentCameraIndex] != null)
        {
            string cameraName = cameras[currentCameraIndex].name;
            return $"{cameraName} (Camera {currentCameraIndex + 1}/{cameras.Length})";
        }
        return "Unknown Camera";
    }

    public Camera GetCurrentCameraObject()
    {
        if (cameras != null && currentCameraIndex < cameras.Length)
        {
            return cameras[currentCameraIndex];
        }
        return null;
    }

    public int GetTotalCameras()
    {
        return cameras != null ? cameras.Length : 0;
    }
}