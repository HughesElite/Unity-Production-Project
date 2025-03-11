using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform npc; // Reference to the NPC's Transform
    public Vector3 offset; // The offset position for the camera relative to the NPC
    public float distanceBehind = 6.0f; // The distance behind the NPC for the camera
    public float heightOffset = 3f; // The height at which the camera should be (eye level)

    void Update()
    {
        // Set the camera's position behind the NPC at eye level
        Vector3 targetPosition = npc.position + npc.forward * -distanceBehind + Vector3.up * heightOffset;
        transform.position = targetPosition;

        // Make the camera look at the NPC (this will make the camera always look at the NPC's face)
        transform.LookAt(npc.position + Vector3.up * heightOffset);
    }
}
