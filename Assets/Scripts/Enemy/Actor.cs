using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Actor : MonoBehaviour
{
    //State Machine
    StateMachine stateMachine;
    
    private NavMeshAgent  _agent;
    private Animator _animator;
    [SerializeField] private ActorType currentActor;
    
    //Patrolling Variables, for the time to start patrolling, delay patrolling when reaching destination and the radius of which the actor can move
    [SerializeField] float patrolTimer;
    [SerializeField] float patrolDelay;
    [SerializeField] float patrolRadius;

    public bool playerInFOV;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        stateMachine = new StateMachine();

        var patrolState = new PatrolState(this, _animator);
        var hostileState = new HostileState(this,_animator);
        
        At(hostileState, patrolState, new FuncPredicate(() => playerInFOV));
        At(patrolState, hostileState, new FuncPredicate(() => !playerInFOV));
        
        stateMachine.SetState(patrolState);
    }
   
    void At(IState to, IState from, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
    
    //Patrol Function
    public void Patrol()
    {
        //If player is in View
        if(playerInFOV)
        {
            StartCoroutine(PlayerDetected());
        }
        else
        {
            //Agent has a destination and the distance remaining is less than stopping distance ideally
            if(!_agent.pathPending && _agent.remainingDistance < 0.5f){
            //increases the patrol timer till next destination is set
            patrolTimer += Time.deltaTime;
            //if patrol timer has crossed the delay amount, set a new position for the Agent
            if(patrolTimer >= patrolDelay){
                _agent.SetDestination(GetPatrolPosition(transform.position, patrolRadius, -1));
                patrolTimer = 0f;
            }
            }
        }
        
    }


    //Get the next position for the agent on patrol
    Vector3 GetPatrolPosition(Vector3 origin, float distance, int layerMask){

        //Gets a random transform position from the center of the player to set the new Position
        Vector3 patrolDirection = UnityEngine.Random.insideUnitSphere * distance;
        patrolDirection += origin;

        NavMeshHit navHit;
        //Sends out a ray inside the navmesh agents patrol radius
        NavMesh.SamplePosition(patrolDirection, out navHit, distance, layerMask);
        //Returns the hit position.
        return navHit.position;
    }


    public IEnumerator PlayerDetected()
    {
        while(playerInFOV)
        {
            Transform target = GetComponent<LineOfSight>().InViewingRange[0];
            if(target != null ) transform.LookAt(target);
            float _PlayerInViewDuration = 0f;
            _PlayerInViewDuration += Time.deltaTime;
            if(_PlayerInViewDuration > 1f)
            {
                //Player Found, actor is now chasing after the player
                Debug.Log("Player found " + name + " is now chasing them");
            }
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
        playerInFOV = false;
        Debug.Log("Player has disappeared from Line of Sight, going back to patrol");
    }
}
