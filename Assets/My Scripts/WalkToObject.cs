using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveToObject : MonoBehaviour
{
    public Transform targetObject;  // The target object to move to
    private Vector3 startPosition;  // The starting position of the player
    private NavMeshAgent agent;     // Reference to the NavMeshAgent component

    public float waitTimeAtTarget = 2f;  // Time to wait at the target before returning
    public float stopDistance = 2f;      // Distance from the target to stop and wait

    private void Start()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        // Store the initial position of the player
        startPosition = transform.position;

        // Start the movement sequence
        StartCoroutine(MoveToTargetAndBack());
    }

    private IEnumerator MoveToTargetAndBack()
    {
        // Move to the target but stop at a specified distance from it
        agent.SetDestination(targetObject.position);

        // Wait until the agent is within the stopping distance from the target
        while (agent.pathPending || agent.remainingDistance > stopDistance)
        {
            yield return null;
        }

        // Once the player is close enough, stop the agent and wait
        agent.isStopped = true;

        // Wait for a few seconds at the target
        yield return new WaitForSeconds(waitTimeAtTarget);

        // Resume movement to the starting position
        agent.isStopped = false;
        agent.SetDestination(startPosition);

        // Wait until the agent reaches the starting position
        while (agent.pathPending || agent.remainingDistance > stopDistance)
        {
            yield return null;
        }

        // Repeat the process
        StartCoroutine(MoveToTargetAndBack());
    }
}
