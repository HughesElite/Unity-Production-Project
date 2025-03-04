using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveToObjects : MonoBehaviour
{
    public Transform[] targetObjects;  // Array of target objects to visit
    private Vector3 startPosition;     // The starting position of the player
    private NavMeshAgent agent;        // Reference to the NavMeshAgent component

    public float waitTimeAtTarget = 2f;  // Time to wait at each target
    public float stopDistance = 2f;      // Distance from the target to stop

    private void Start()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        // Store the initial position of the player
        startPosition = transform.position;

        // Start the movement sequence
        StartCoroutine(MoveThroughTargetsAndBack());
    }

    private IEnumerator MoveThroughTargetsAndBack()
    {
        // Loop through all target objects
        foreach (Transform target in targetObjects)
        {
            agent.SetDestination(target.position);

            // Wait until the agent is within the stopping distance
            while (agent.pathPending || agent.remainingDistance > stopDistance)
            {
                yield return null;
            }

            // Stop the agent and wait at the target
            agent.isStopped = true;
            yield return new WaitForSeconds(waitTimeAtTarget);
            agent.isStopped = false;
        }

        // After visiting all targets, return to the start position
        agent.SetDestination(startPosition);

        // Wait until the agent reaches the starting position
        while (agent.pathPending || agent.remainingDistance > stopDistance)
        {
            yield return null;
        }

        // Restart the process
        StartCoroutine(MoveThroughTargetsAndBack());
    }
}
