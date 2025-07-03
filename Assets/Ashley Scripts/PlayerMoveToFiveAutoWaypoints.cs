using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveToObjects : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public Transform[] targetObjects; // Manually assigned waypoints

    [Header("Movement Settings")]
    public float waitTimeAtTarget = 2f;
    public float stopDistance = 2f;
    public float WaitTimeAtStart = 5f;

    private Vector3 startPosition;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Make sure both stopping distances match
        agent.stoppingDistance = stopDistance;

        startPosition = transform.position;
        StartCoroutine(MoveToWaypointsAndBack());
    }

    private IEnumerator MoveToWaypointsAndBack()
    {
        if (targetObjects == null || targetObjects.Length == 0)
        {
            Debug.LogError("No waypoints assigned! Please assign waypoints in the inspector.");
            yield break;
        }

        // Visit all waypoints in order
        for (int i = 0; i < targetObjects.Length; i++)
        {
            Transform currentTarget = targetObjects[i];
            Debug.Log($"Moving to waypoint {i + 1}/{targetObjects.Length}: {currentTarget.name}");
            agent.SetDestination(currentTarget.position);

            // Wait until agent reaches target
            while (agent.pathPending || agent.remainingDistance > stopDistance)
            {
                yield return null;
            }

            Debug.Log($"Reached waypoint: {currentTarget.name}");
            agent.isStopped = true;
            yield return new WaitForSeconds(waitTimeAtTarget);
            agent.isStopped = false;
        }

        // Return to start position
        Debug.Log("Returning to start position");
        agent.SetDestination(startPosition);

        while (agent.pathPending || agent.remainingDistance > stopDistance)
        {
            yield return null;
        }

        Debug.Log("Reached start position");
        agent.isStopped = true;
        yield return new WaitForSeconds(WaitTimeAtStart);
        agent.isStopped = false;

        // Restart the cycle
        Debug.Log("Restarting waypoint cycle");
        StartCoroutine(MoveToWaypointsAndBack());
    }
}