using UnityEngine;
using UnityEngine.UI;  // Import UI namespace

public class ChangeCamerView : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;
    public Camera camera3;
    public Transform npc;
    public Vector3 offset;
    public float followSpeed = 5f;
    private int activeCameraIndex = 0;

    void Start()
    {
        camera1.enabled = true;
        camera2.enabled = false;
        camera3.enabled = false;

        if (offset == Vector3.zero)
            offset = new Vector3(0, 5, -10);
    }

    void Update()
    {
        if (camera3.enabled && npc != null)
        {
            camera3.transform.position = Vector3.Lerp(camera3.transform.position, npc.position + offset, Time.deltaTime * followSpeed);
            camera3.transform.LookAt(npc);
        }
    }

    // Public method for UI button click
    public void ChangeCameraView()
    {
        activeCameraIndex = (activeCameraIndex + 1) % 3; // Cycle through cameras (0,1,2)
        SwitchToCamera(activeCameraIndex + 1);
    }

    void SwitchToCamera(int cameraIndex)
    {
        camera1.enabled = false;
        camera2.enabled = false;
        camera3.enabled = false;

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
