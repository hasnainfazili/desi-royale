using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Actor : MonoBehaviour
{
    private static readonly int Throw = Animator.StringToHash("Throw Object");

    //State Machine
    StateMachine _stateMachine;
    
    //References
    private NavMeshAgent  _agent;
    private Animator _animator;
    [SerializeField] private ActorType currentActor;
    
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        
        //Initializing the State Machine
        _stateMachine = new StateMachine();

        //Initializing the states
        var patrolState = new PatrolState(this, _animator);
        var hostileState = new HostileState(this,_animator);
        var engagedState = new EngageState(this, _animator);
        
        At(hostileState, patrolState, new FuncPredicate(() => playerInFOV));
        At(engagedState, patrolState, new FuncPredicate(() => _playerDetected));
        Any(patrolState, new FuncPredicate(() => !playerInFOV && !_playerDetected));
        
        _stateMachine.SetState(patrolState);
    }

    #region Helper Methods for State Machine

    //Helper methods for transitioning between states
    void At(IState to, IState from, IPredicate condition) => _stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => _stateMachine.AddAnyTransition(to, condition);
    
    #endregion
    private void Update()
    {
        _stateMachine.Update();
    }

    private void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
    }

    #region Patrol

    //Patrol Function

    
    //Patrolling Variables, for the time to start patrolling, delay patrolling when reaching destination and the radius of which the actor can move
    [SerializeField] float patrolTimer;
    [SerializeField] float patrolDelay;
    [SerializeField] float patrolRadius;

    public bool playerInFOV;
    private bool _playerDetected;
    
    private static readonly int MoveSpeed = Animator.StringToHash("Speed");

    public void Patrol()
    {
        //If player is in View
        if(playerInFOV)
        {
            StartCoroutine(PlayerDetected());
        }
        else
        {
            _animator.SetFloat(MoveSpeed, _agent.velocity.magnitude);
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


    private IEnumerator PlayerDetected()
    {
        while(playerInFOV)
        {
            _agent.isStopped = true;
            var list = GetComponent<LineOfSight>().InViewingRange;
            if (list.Count > 0)
            {
                Transform target = list[0];
                if(target != null ) transform.LookAt(target);
            }

            float playerInViewDuration = 0f;
            playerInViewDuration += Time.deltaTime;
            if(playerInViewDuration > 1f)
            {
                _playerDetected = true;
                //Player Found, actor is now chasing after the player
                Debug.Log("Player found " + name + " is now chasing them");
            }
            yield return new WaitWhile(() => !_playerDetected);
        }
        
        yield return new WaitForSeconds(1f);
        playerInFOV = false;
        Debug.Log("Player has disappeared from Line of Sight, going back to patrol");
    }
    #endregion

    private Transform _target;
    private float _atkRange;

    private float _atkDelay;
    //Temporary for testing
    public void Engaged()
    {
        //Null check target
        if (_target != null) 
            _agent.SetDestination(_target.position);

        var distanceFromTarget = Vector3.Distance(transform.position, _target.position);
        if (distanceFromTarget < _atkRange)
        {
            //Attack Player
            //Aim at the player and launch the shoe.
            if (_atkDelay != 0 && isThrowAvailable)
            {
                isThrowAvailable = false;
                Vector3 lookAtDirection = new Vector3(_target.position.x, transform.position.y, _target.position.z);
                transform.LookAt(Vector3.Lerp(transform.position,_target.position, _agent.angularSpeed * Time.deltaTime));
                _animator.SetTrigger(Throw);
            }
        }
    }
    #region Throw Region

    [SerializeField] private Rigidbody currentThrowableEquipped;
    [FormerlySerializedAs("IsThrowAvailable")] [SerializeField] private bool isThrowAvailable = true;
    [FormerlySerializedAs("ThrowDelay")] [SerializeField] private float throwDelay;
    [FormerlySerializedAs("ThrowAudioClip")] [SerializeField] private AudioClip throwAudioClip;
    [FormerlySerializedAs("ThrowStrength")] [SerializeField] private float throwStrength = 10f; 
    [FormerlySerializedAs("_releaseTransform")] [SerializeField] private Transform releaseTransform;
    
    private Transform _initialParent;
    private Vector3 _initialLocalPosition;
    private Quaternion _initialRotation;

    public void InitializeThrow()
    {
        _initialParent = currentThrowableEquipped.transform.parent;
        _initialRotation = currentThrowableEquipped.transform.localRotation;
        _initialLocalPosition = currentThrowableEquipped.transform.localPosition;
        currentThrowableEquipped.freezeRotation = true;
    }
    
    private void ReleaseThrowable()
    {
        currentThrowableEquipped.velocity = Vector3.zero;
        currentThrowableEquipped.angularVelocity = Vector3.zero;
        currentThrowableEquipped.isKinematic = false;
        currentThrowableEquipped.freezeRotation = false;
        currentThrowableEquipped.transform.SetParent(null, true);
        currentThrowableEquipped.AddForce(Camera.main!.transform.forward * throwStrength, ForceMode.Impulse);

        SoundFXManager.instance.PlaySingleSoundFXClip(throwAudioClip, transform, .4f);
        StartCoroutine(ResetThrowable());
    }
    //Coroutine resets the transform, rotation, and bools of the throwable
    IEnumerator ResetThrowable()
    {
        yield return new WaitForSeconds(throwDelay);
        currentThrowableEquipped.freezeRotation = true;
        currentThrowableEquipped.isKinematic = true;
        currentThrowableEquipped.transform.SetParent(_initialParent, true);
        currentThrowableEquipped.rotation = _initialRotation.normalized;
        currentThrowableEquipped.transform.localPosition = _initialLocalPosition;
        isThrowAvailable = true;
    }
    #endregion

    public void Hostile()
    {
        // Chase Player if player is in Field of View
        if(playerInFOV && _target!=null)
            _agent.SetDestination(_target.position);
    }
}
