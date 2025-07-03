using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveRandomly : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public string waypointTag = "Waypoint";

    [Header("Waypoint Exclusions")]
    [Tooltip("Drag waypoints here to exclude them from the random cycle")]
    public Transform[] excludedWaypoints;

    [Header("Movement Settings")]
    public float waitTimeAtTarget = 2f;
    public float stopDistance = 2f;
    public float WaitTimeAtStart = 5f;

    [Header("Debug Info")]
    [Tooltip("Shows which waypoints will be visited (read-only)")]
    public Transform[] activeWaypoints; // Shows in Inspector for debugging

    private Vector3 startPosition;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Make sure both stopping distances match
        agent.stoppingDistance = stopDistance;

        startPosition = transform.position;

        // Build the active waypoint list
        BuildActiveWaypointList();

        StartCoroutine(MoveToAllWaypointsRandomly());
    }

    private void BuildActiveWaypointList()
    {
        // Find all waypoints with the specified tag
        GameObject[] allWaypointObjects = GameObject.FindGameObjectsWithTag(waypointTag);

        // Convert to Transform array
        Transform[] allWaypoints = new Transform[allWaypointObjects.Length];
        for (int i = 0; i < allWaypointObjects.Length; i++)
        {
            allWaypoints[i] = allWaypointObjects[i].transform;
        }

        // Create list of waypoints that are NOT excluded
        List<Transform> filteredWaypoints = new List<Transform>();

        foreach (Transform waypoint in allWaypoints)
        {
            bool isExcluded = false;

            // Check if this waypoint is in the exclusion list
            if (excludedWaypoints != null)
            {
                foreach (Transform excludedWaypoint in excludedWaypoints)
                {
                    if (waypoint == excludedWaypoint)
                    {
                        isExcluded = true;
                        break;
                    }
                }
            }

            // Add to active list if not excluded
            if (!isExcluded)
            {
                filteredWaypoints.Add(waypoint);
            }
        }

        // Convert back to array for easy access
        activeWaypoints = filteredWaypoints.ToArray();

        // Debug info
        Debug.Log($"{gameObject.name}: Found {allWaypoints.Length} total waypoints");
        Debug.Log($"{gameObject.name}: Excluded {(excludedWaypoints != null ? excludedWaypoints.Length : 0)} waypoints");
        Debug.Log($"{gameObject.name}: Will visit {activeWaypoints.Length} waypoints randomly");

        if (activeWaypoints.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: No active waypoints! Either no waypoints found with tag '{waypointTag}' or all waypoints are excluded.");
        }
    }

    private IEnumerator MoveToAllWaypointsRandomly()
    {
        if (activeWaypoints == null || activeWaypoints.Length == 0)
        {
            Debug.LogError($"{gameObject.name}: No active waypoints available! Cannot start movement.");
            yield break;
        }

        // Create a shuffled list of ALL active waypoints
        List<Transform> shuffledWaypoints = new List<Transform>(activeWaypoints);
        ShuffleList(shuffledWaypoints);

        // Visit ALL active waypoints in random order
        for (int i = 0; i < shuffledWaypoints.Count; i++)
        {
            Transform currentTarget = shuffledWaypoints[i];
            Debug.Log($"{gameObject.name}: Moving to random waypoint {i + 1}/{shuffledWaypoints.Count}: {currentTarget.name}");
            agent.SetDestination(currentTarget.position);

            // Wait until agent reaches target
            while (agent.pathPending || agent.remainingDistance > stopDistance)
            {
                yield return null;
            }

            Debug.Log($"{gameObject.name}: Reached waypoint: {currentTarget.name}");
            agent.isStopped = true;
            yield return new WaitForSeconds(waitTimeAtTarget);
            agent.isStopped = false;
        }

        // Return to start position
        Debug.Log($"{gameObject.name}: Returning to start position");
        agent.SetDestination(startPosition);

        while (agent.pathPending || agent.remainingDistance > stopDistance)
        {
            yield return null;
        }

        Debug.Log($"{gameObject.name}: Reached start position");
        agent.isStopped = true;
        yield return new WaitForSeconds(WaitTimeAtStart);
        agent.isStopped = false;

        // Restart the cycle with a new random order
        Debug.Log($"{gameObject.name}: Restarting random waypoint cycle");
        StartCoroutine(MoveToAllWaypointsRandomly());
    }

    // Fisher-Yates shuffle algorithm for randomizing waypoint order
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Public method to refresh waypoint list if waypoints are added/removed at runtime
    [ContextMenu("Refresh Waypoints")]
    public void RefreshWaypoints()
    {
        BuildActiveWaypointList();
        Debug.Log($"{gameObject.name}: Waypoint list refreshed - {activeWaypoints.Length} active waypoints");
    }

    // Helper method to add a waypoint to exclusion list at runtime
    public void ExcludeWaypoint(Transform waypoint)
    {
        if (waypoint == null) return;

        // Add to exclusion list
        List<Transform> exclusionList = new List<Transform>();
        if (excludedWaypoints != null)
        {
            exclusionList.AddRange(excludedWaypoints);
        }

        if (!exclusionList.Contains(waypoint))
        {
            exclusionList.Add(waypoint);
            excludedWaypoints = exclusionList.ToArray();
            BuildActiveWaypointList();
            Debug.Log($"{gameObject.name}: Excluded waypoint {waypoint.name}");
        }
    }

    // Helper method to remove a waypoint from exclusion list at runtime
    public void IncludeWaypoint(Transform waypoint)
    {
        if (waypoint == null || excludedWaypoints == null) return;

        List<Transform> exclusionList = new List<Transform>(excludedWaypoints);
        if (exclusionList.Remove(waypoint))
        {
            excludedWaypoints = exclusionList.ToArray();
            BuildActiveWaypointList();
            Debug.Log($"{gameObject.name}: Re-included waypoint {waypoint.name}");
        }
    }
}