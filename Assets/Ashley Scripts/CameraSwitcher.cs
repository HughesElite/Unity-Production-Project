using UnityEngine;

public class SimpleCameraSwitcher : MonoBehaviour
{
    [Header("Camera Setup")]
    public Camera[] cameras; 

    [Header("Settings")]
    public bool startWithFirstCamera = true; 

    private int currentCameraIndex = 0;

    void Start()
    {
        // Validates the cameras array
        if (cameras == null || cameras.Length == 0)
        {
            return;
        }

        // Removes any null cameras from the array
        System.Array.Resize(ref cameras, cameras.Length);
        cameras = System.Array.FindAll(cameras, camera => camera != null);

        if (cameras.Length == 0)
        {
            return;
        }

       
        if (startWithFirstCamera)
        {
            currentCameraIndex = 0;
        }

        
        DisableAllCameras();

        if (currentCameraIndex < cameras.Length)
        {
            cameras[currentCameraIndex].enabled = true;
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
        
        if (cameras == null || cameras.Length == 0)
        {
            return;
        }

        
        if (cameras.Length == 1)
        {
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
        }
        else
        {
          
            FindNextValidCamera();
        }
    }

    // Switchs to a specific camera by index
    public void SwitchToCamera(int index)
    {
        if (cameras == null || index < 0 || index >= cameras.Length)
        {
            return;
        }

        if (cameras[index] == null)
        {
            return;
        }

       
        if (currentCameraIndex < cameras.Length && cameras[currentCameraIndex] != null)
        {
            cameras[currentCameraIndex].enabled = false;
        }

       
        currentCameraIndex = index;
        cameras[currentCameraIndex].enabled = true;
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
                return;
            }

        } while (currentCameraIndex != startIndex);
    }

    // Get current camera info (used for UI updates)
    public int GetCurrentCameraIndex()
    {
        return currentCameraIndex;
    }

    public int GetCurrentCamera()
    {
        return currentCameraIndex + 1; 
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