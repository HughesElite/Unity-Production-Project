using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveRandomly : MonoBehaviour
{
    public Transform[] targetObjects;
    private Vector3 startPosition;
    private NavMeshAgent agent;
    public float waitTimeAtTarget = 2f;
    public float stopDistance = 2f;
    public float WaitTimeAtStart = 5f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        StartCoroutine(MoveToRandomTargetsAndBack());
    }

    private IEnumerator MoveToRandomTargetsAndBack()
    {
        // Visit 5 random targets
        for (int i = 0; i < 5; i++)
        {
            Transform randomTarget = targetObjects[Random.Range(0, targetObjects.Length)];
            agent.SetDestination(randomTarget.position);

            // Wait until NPC reaches target
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