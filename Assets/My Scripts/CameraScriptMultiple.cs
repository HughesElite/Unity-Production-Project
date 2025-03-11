using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;


    void Start()
    {
        // Ensure only camera1 is active at the start
        camera1.enabled = true;
        camera2.enabled = false;
  
    }

    void Update()
    {
        // Press 'C' to toggle cameras
        if (Input.GetKeyDown(KeyCode.C))
        {
            camera1.enabled = !camera1.enabled;
            camera2.enabled = !camera2.enabled;
         
        }
    }
}
