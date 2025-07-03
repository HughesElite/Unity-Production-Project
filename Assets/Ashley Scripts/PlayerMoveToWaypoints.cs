using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveToObjects : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public Transform[] targetObjects; // Manually assigned waypoints

    [Header("Movement Settings")]
    [Tooltip("Manual control - adjust per NPC as needed")]
    public float stopDistance = 2f;

    [Header("Random Wait Time Ranges")]
    [Tooltip("Each NPC will use random wait times within these ranges")]
    public float minWaitTimeAtTarget = 1f;
    public float maxWaitTimeAtTarget = 5f;
    public float minWaitTimeAtStart = 1f;
    public float maxWaitTimeAtStart = 5f;

    [Header("Debug Info")]
    [Tooltip("Shows the random wait times this NPC is using (read-only)")]
    public float currentTargetWaitTime;
    public float currentStartWaitTime;

    private Vector3 startPosition;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Make sure both stopping distances match
        agent.stoppingDistance = stopDistance;

        startPosition = transform.position;

        // Generate random wait times for this specific NPC
        GenerateRandomWaitTimes();

        StartCoroutine(MoveToWaypointsAndBack());
    }

    private void GenerateRandomWaitTimes()
    {
        // Each NPC gets its own random wait times
        currentTargetWaitTime = Random.Range(minWaitTimeAtTarget, maxWaitTimeAtTarget);
        currentStartWaitTime = Random.Range(minWaitTimeAtStart, maxWaitTimeAtStart);

        Debug.Log($"{gameObject.name}: Generated wait times - Target: {currentTargetWaitTime:F1}s, Start: {currentStartWaitTime:F1}s");
    }

    private IEnumerator MoveToWaypointsAndBack()
    {
        if (targetObjects == null || targetObjects.Length == 0)
        {
            Debug.LogError($"{gameObject.name}: No waypoints assigned! Please assign waypoints in the inspector.");
            yield break;
        }

        // Visit all waypoints in order
        for (int i = 0; i < targetObjects.Length; i++)
        {
            Transform currentTarget = targetObjects[i];
            Debug.Log($"{gameObject.name}: Moving to waypoint {i + 1}/{targetObjects.Length}: {currentTarget.name}");
            agent.SetDestination(currentTarget.position);

            // Wait until agent reaches target
            while (agent.pathPending || agent.remainingDistance > stopDistance)
            {
                yield return null;
            }

            Debug.Log($"{gameObject.name}: Reached waypoint: {currentTarget.name} - waiting {currentTargetWaitTime:F1}s");
            agent.isStopped = true;
            yield return new WaitForSeconds(currentTargetWaitTime); // Use this NPC's random wait time
            agent.isStopped = false;
        }

        // Return to start position
        Debug.Log($"{gameObject.name}: Returning to start position");
        agent.SetDestination(startPosition);

        while (agent.pathPending || agent.remainingDistance > stopDistance)
        {
            yield return null;
        }

        Debug.Log($"{gameObject.name}: Reached start position - waiting {currentStartWaitTime:F1}s");
        agent.isStopped = true;
        yield return new WaitForSeconds(currentStartWaitTime); // Use this NPC's random wait time
        agent.isStopped = false;

        // Generate new random wait times for next cycle (optional - remove if you want consistent times per NPC)
        GenerateRandomWaitTimes();

        // Restart the cycle
        Debug.Log($"{gameObject.name}: Restarting waypoint cycle");
        StartCoroutine(MoveToWaypointsAndBack());
    }

    // Public method to manually regenerate wait times (useful for testing)
    [ContextMenu("Generate New Random Wait Times")]
    public void RegenerateWaitTimes()
    {
        GenerateRandomWaitTimes();
    }

    // Helper methods to get current wait times (for other scripts)
    public float GetCurrentTargetWaitTime()
    {
        return currentTargetWaitTime;
    }

    public float GetCurrentStartWaitTime()
    {
        return currentStartWaitTime;
    }
}