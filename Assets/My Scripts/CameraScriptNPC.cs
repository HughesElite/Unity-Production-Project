using UnityEngine;

public class CameraScriptNPC : MonoBehaviour
{
    public Transform npc;  // The NPC's Transform
    public Vector3 offset; // The offset from the NPC's position

    void Start()
    {
        // If no NPC is assigned, try to find it by tag
        if (npc == null)
            npc = GameObject.FindGameObjectWithTag("NPC").transform;

        // You can set an offset here if you want the camera to be behind the NPC, etc.
        if (offset == Vector3.zero)
            offset = new Vector3(0, 5, -10);  // Default offset
    }

    void Update()
    {
        // Update the camera's position to follow the NPC with the offset
        if (npc != null)
        {
            transform.position = npc.position + offset;
            transform.LookAt(npc); // Optional: to have the camera always face the NPC
        }
    }
}