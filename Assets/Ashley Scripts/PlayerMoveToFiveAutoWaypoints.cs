using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveAutoWaypoints : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public string waypointTag = "Waypoint";

    [Header("Movement Settings")]
    public float waitTimeAtTarget = 2f;
    public float stopDistance = 2f;
    public float WaitTimeAtStart = 5f;

    private Transform[] targetObjects; // Auto-populated waypoints
    private Vector3 startPosition;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        FindWaypointsByTag();
        StartCoroutine(MoveToRandomTargetsAndBack());
    }

    private void FindWaypointsByTag()
    {
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag(waypointTag);

        targetObjects = new Transform[waypointObjects.Length];
        for (int i = 0; i < waypointObjects.Length; i++)
        {
            targetObjects[i] = waypointObjects[i].transform;
        }

        if (targetObjects.Length == 0)
        {
            Debug.LogWarning($"No waypoints found with tag '{waypointTag}'! Make sure your waypoint objects have the correct tag.");
        }
    }

    private IEnumerator MoveToRandomTargetsAndBack()
    {
        if (targetObjects == null || targetObjects.Length == 0)
        {
            Debug.LogError("No waypoints found! Cannot start movement.");
            yield break;
        }

        // Visit 5 random targets
        for (int i = 0; i < 5; i++)
        {
            Transform randomTarget = targetObjects[Random.Range(0, targetObjects.Length)];
            agent.SetDestination(randomTarget.position);

            // Wait until agent reaches target
            while (agent.pathPending || agent.remainingDistance > stopDistance)
            {
                yield return null;
            }

            agent.isStopped = true;
            yield return new WaitForSeconds(waitTimeAtTarget);
            agent.isStopped = false;
        }

        // Return to start position
        agent.SetDestination(startPosition);

        while (agent.pathPending || agent.remainingDistance > stopDistance)
        {
            yield return null;
        }

        agent.isStopped = true;
        yield return new WaitForSeconds(WaitTimeAtStart);
        agent.isStopped = false;

        // Restart the cycle
        StartCoroutine(MoveToRandomTargetsAndBack());
    }
}