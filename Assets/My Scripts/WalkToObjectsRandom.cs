using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveRandomly : MonoBehaviour
{
    public Transform[] targetObjects;  // Array of possible target locations
    private Vector3 startPosition;     // The player's starting position
    private NavMeshAgent agent;        // Reference to the NavMeshAgent component

    public float waitTimeAtTarget = 2f;      // Time to wait at each target
    public float stopDistance = 2f;          // Distance from the target to stop
    public float WaitTimeAtStart = 5f; // Time to wait at the start position before restarting

    private void Start()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        // Store the initial position of the player
        startPosition = transform.position;

        // Start the random movement cycle
        StartCoroutine(MoveToRandomTargetsAndBack());
    }

    private IEnumerator MoveToRandomTargetsAndBack()
    {
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
}
