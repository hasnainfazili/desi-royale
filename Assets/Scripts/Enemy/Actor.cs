using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Actor : MonoBehaviour
{
    [SerializeField] private StateMachine ActorStateMachine;
    [SerializeField] private ActorType currentActor;
    private NavMeshAgent  agent;
   
    [SerializeField] float patrolTimer;
    [SerializeField] float patrolDelay;
    [SerializeField] float patrolRadius;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        //Temp Patrol Code Shift this in to a Patrol State
        Patrol();
    }
    
     void Patrol()
        {
        if(!agent.pathPending && agent.remainingDistance < 0.5f){
            patrolTimer += Time.deltaTime;
            if(patrolTimer >= patrolDelay){
                agent.SetDestination(GetPatrolPosition(transform.position, patrolRadius, -1));
                patrolTimer = 0f;
            }
        }
    }

    Vector3 GetPatrolPosition(Vector3 origin, float distance, int layerMask){
        Vector3 patrolDirection = UnityEngine.Random.insideUnitSphere * distance;
        patrolDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(patrolDirection, out navHit, distance, layerMask);

        return navHit.position;
    }
}
