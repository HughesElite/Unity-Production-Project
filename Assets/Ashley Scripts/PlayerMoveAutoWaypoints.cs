using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveAutoWaypoints : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public string waypointTag = "Waypoint";  // Tag to search for waypoints

    [Header("Movement Settings")]
    public float waitTimeAtTarget = 2f;      // Time to wait at each target
    public float stopDistance = 2f;          // Distance from the target to stop
    public float WaitTimeAtStart = 5f;       // Time to wait at the start position before restarting

    private Transform[] targetObjects;       // Array of waypoints (auto-populated)
    private Vector3 startPosition;           // The player's starting position
    private NavMeshAgent agent;              // Reference to the NavMeshAgent component

    private void Start()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        // Store the initial position of the player
        startPosition = transform.position;

        // Automatically find all waypoints with the specified tag
        FindWaypointsByTag();

        // Start the random movement cycle
        StartCoroutine(MoveToRandomTargetsAndBack());
    }

    private void FindWaypointsByTag()
    {
        // Find all GameObjects with the waypoint tag
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag(waypointTag);

        // Create array of transforms
        targetObjects = new Transform[waypointObjects.Length];

        for (int i = 0; i < waypointObjects.Length; i++)
        {
            targetObjects[i] = waypointObjects[i].transform;
        }

       // Debug.Log($"Found {targetObjects.Length} waypoints with tag '{waypointTag}'");

        // Warning if no waypoints found
        if (targetObjects.Length == 0)
        {
            Debug.LogWarning($"No waypoints found with tag '{waypointTag}'! Make sure your waypoint objects have the correct tag.");
        }
    }

    private IEnumerator MoveToRandomTargetsAndBack()
    {
        // Check if we have any targets
        if (targetObjects == null || targetObjects.Length == 0)
        {
            Debug.LogError("No waypoints found! Cannot start movement.");
            yield break;
        }

        // Pick a random target five times
        for (int i = 0; i < 5; i++)
        {
            Transform randomTarget = targetObjects[Random.Range(0, targetObjects.Length)];
            agent.SetDestination(randomTarget.position);

            // Wait until the agent reaches the target
            while (agent.pathPending || agent.remainingDistance > stopDistance)
            {
                yield return null;
            }

            // Stop at the target and wait
            agent.isStopped = true;
            yield return new WaitForSeconds(waitTimeAtTarget);
            agent.isStopped = false;
        }

        // Return to the start position
        agent.SetDestination(startPosition);

        // Wait until the agent reaches the start position
        while (agent.pathPending || agent.remainingDistance > stopDistance)
        {
            yield return null;
        }

        // Stop at the start position for a longer period
        agent.isStopped = true;
        yield return new WaitForSeconds(WaitTimeAtStart);
        agent.isStopped = false;

        // Restart the process
        StartCoroutine(MoveToRandomTargetsAndBack());
    }

    // Optional: Refresh waypoints at runtime (right-click script in inspector)
    [ContextMenu("Refresh Waypoints")]
    private void RefreshWaypoints()
    {
        FindWaypointsByTag();
    }
}