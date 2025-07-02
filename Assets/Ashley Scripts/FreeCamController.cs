using UnityEngine;

public class FreeRoamCameraController : MonoBehaviour
{
    [Header("References")]
    public SimpleCameraSwitcher cameraSwitcher;
    public Camera freeRoamCamera;

    [Header("Cursor Control")]
    public bool lockCursorInFreeRoam = true;

    private int previousCameraIndex = 0; // Remember which camera was active before free roam

    void Start()
    {
        if (freeRoamCamera != null)
        {
            freeRoamCamera.enabled = false;
        }
    }

    void Update()
    {
        bool isOnFreeRoamCamera = (freeRoamCamera != null && freeRoamCamera.enabled);

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

    // Call this from "Free Cam" button
    public void SwitchToFreeRoam()
    {
        if (cameraSwitcher != null && freeRoamCamera != null)
        {
            previousCameraIndex = cameraSwitcher.GetCurrentCameraIndex();

            Camera currentCamera = cameraSwitcher.GetCurrentCameraObject();
            if (currentCamera != null)
            {
                currentCamera.enabled = false;
            }

            freeRoamCamera.enabled = true;
        }
    }

    // Call this to exit free roam
    public void ExitFreeRoam()
    {
        if (cameraSwitcher != null && freeRoamCamera != null)
        {
            freeRoamCamera.enabled = false;
            cameraSwitcher.SwitchToCamera(previousCameraIndex);
        }
    }
}