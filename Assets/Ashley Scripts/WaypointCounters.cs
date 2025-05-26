using UnityEngine;

public class WaypointCounter : MonoBehaviour
{
    [Header("Settings")]
    public string waypointTag = "Waypoint";
    public string npcTag = "NPC";
    public bool countOnStart = true;
    public bool countOnEnable = false;

    private void Start()
    {
        if (countOnStart)
        {
            CountWaypoints();
        }
    }

    private void OnEnable()
    {
        if (countOnEnable)
        {
            CountWaypoints();
        }
    }

    public void CountWaypoints()
    {
        GameObject[] waypoints = GameObject.FindGameObjectsWithTag(waypointTag);
        GameObject[] npcs = GameObject.FindGameObjectsWithTag(npcTag);

        Debug.Log($"Total waypoints: {waypoints.Length} | Total NPCs: {npcs.Length}");

    }
}