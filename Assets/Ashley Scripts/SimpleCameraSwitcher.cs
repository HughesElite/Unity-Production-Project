using UnityEngine;

public class SimpleCameraSwitcher : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;
    public Camera camera3;

    private int currentCamera = 1;

    void Start()
    {
        // Enable camera 1, disable others
        camera1.enabled = true;
        camera2.enabled = false;
        camera3.enabled = false;
    }

    void Update()
    {
        // Check for C key press
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCamera();
        }
    }

    void SwitchCamera()
    {
        // Disable all cameras
        camera1.enabled = false;
        camera2.enabled = false;
        camera3.enabled = false;

        // Move to next camera
        currentCamera++;
        if (currentCamera > 3)
            currentCamera = 1;

        // Enable the current camera
        switch (currentCamera)
        {
            case 1:
                camera1.enabled = true;
                Debug.Log("Switched to Camera 1");
                break;
            case 2:
                camera2.enabled = true;
                Debug.Log("Switched to Camera 2");
                break;
            case 3:
                camera3.enabled = true;
                Debug.Log("Switched to Camera 3");
                break;
        }
    }
}