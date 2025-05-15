using UnityEngine;

public class CameraFollowFar : MonoBehaviour
{
    public Transform npc; // Reference to the NPC's Transform
    public float distanceBehind = 10.0f; // Increased distance behind NPC
    public float heightOffset = 5.0f; // Increased height to get a better view
    public float smoothSpeed = 5.0f; // How smoothly the camera follows the NPC

    void LateUpdate()
    {
        if (npc == null) return;

        // Calculate the new camera position (farther behind and above the NPC)
        Vector3 targetPosition = npc.position + npc.forward * -distanceBehind + Vector3.up * heightOffset;

        // Smoothly interpolate camera position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Make the camera look at the NPC from above
        transform.LookAt(npc.position + Vector3.up * 1.5f); // Aim at the NPC's upper body
    }
}
