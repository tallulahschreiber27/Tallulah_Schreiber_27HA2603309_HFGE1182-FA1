using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimalAI : MonoBehaviour
{
    public enum NPCState { Idle, Patrol, Runaway, Dead }
    [SerializeField] private NPCState currentState = NPCState.Idle;
    
    [Header("AI PARAMETERS")]
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private float patrolRange = 4f;

    [Header("DETECTION PARAMETERS")] 
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float runawayRange = 20f;
    [SerializeField] private Transform playerTransform;

    [Header("AREA RESTRICTION")] 
    [SerializeField] private Transform areaCenter;
    [SerializeField] private float areaRadius = 30f;
    
    private Vector3 target;
    
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navMeshAgent =  GetComponent<NavMeshAgent>();
        PickNewPatrolPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == NPCState.Dead)
        {
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (Vector3.Distance(transform.position, areaCenter.position) > areaRadius)
        {
            navMeshAgent.SetDestination(areaCenter.position);
            
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            currentState = NPCState.Runaway;
        }
        
        StateHandler();
    }

    public void StateHandler()
    {
      
        switch (currentState)
        {
            case NPCState.Idle:
                navMeshAgent.isStopped = false;
                StartCoroutine(IdleFor(1f));
                break;
            case NPCState.Patrol:
                PatrolRoutine();
                break;
            case NPCState.Runaway:
                navMeshAgent.isStopped = false;
                Vector3 directionToTarget = transform.position - playerTransform.position;
                Vector3 targetPosition = transform.position + directionToTarget.normalized * runawayRange;
                navMeshAgent.SetDestination(targetPosition);
                break;
            case NPCState.Dead:
                break;
        }
    }
    
    //PATROL

    public void PatrolRoutine()
    {
        navMeshAgent.isStopped = false;

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < .5f)
        {
            PickNewPatrolPoint();
        }
    }

    public void PickNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRange;
        randomDirection += areaCenter.position;
        
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRange, NavMesh.AllAreas))
        {
            target = hit.position;
            navMeshAgent.SetDestination(target);
        }
    }

    public IEnumerator IdleFor(float time)
    {
        yield return new WaitForSeconds(time);
        currentState = NPCState.Patrol;
    }
}
