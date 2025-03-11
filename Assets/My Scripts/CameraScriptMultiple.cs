using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera camera1;           // Static Camera 1
    public Camera camera2;           // Static Camera 2
    public Camera camera3;           // Camera that follows the NPC
    public Transform npc;            // Reference to the NPC's Transform
    public Vector3 offset;           // Offset from the NPC for the third camera
    public float followSpeed = 5f;   // Speed at which camera follows the NPC

    private int activeCameraIndex = 0;  // Index to track the active camera

    void Start()
    {
        // Ensure only the first camera is active at the start
        camera1.enabled = true;
        camera2.enabled = false;
        camera3.enabled = false;

        // Default offset for the camera (if not set in the Inspector)
        if (offset == Vector3.zero)
            offset = new Vector3(0, 5, -10);  // Default offset for camera3
    }

    void Update()
    {
        // Press 'C' to cycle through cameras
        if (Input.GetKeyDown(KeyCode.C))
        {
            activeCameraIndex++;
            if (activeCameraIndex > 2)
                activeCameraIndex = 0;  // Loop back to the first camera

            SwitchToCamera(activeCameraIndex + 1);  // Switch cameras (index starts at 1)
        }

        // If camera3 (the following camera) is active, update its position to follow the NPC
        if (camera3.enabled && npc != null)
        {
            // Move camera smoothly to the new position
            camera3.transform.position = Vector3.Lerp(camera3.transform.position, npc.position + offset, Time.deltaTime * followSpeed);

            // Make camera3 always look at the NPC
            camera3.transform.LookAt(npc);
        }
    }

    void SwitchToCamera(int cameraIndex)
    {
        // Deactivate all cameras
        camera1.enabled = false;
        camera2.enabled = false;
        camera3.enabled = false;

        // Activate the selected camera based on the index
        switch (cameraIndex)
        {
            case 1:
                camera1.enabled = true;
                break;
            case 2:
                camera2.enabled = true;
                break;
            case 3:
                camera3.enabled = true;
                break;
        }
    }
}
