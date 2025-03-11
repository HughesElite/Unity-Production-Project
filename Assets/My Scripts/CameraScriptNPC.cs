using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform npc;  // Reference to the NPC
    public Vector3 offset = new Vector3(0, 1.5f, -4);  // Adjust height to eye level and distance behind NPC

    void LateUpdate()
    {
        // Follow the NPC's position with the offset
        transform.position = npc.position + offset;

        // Ensure the camera looks at the NPC
        transform.LookAt(npc);
    }
}

