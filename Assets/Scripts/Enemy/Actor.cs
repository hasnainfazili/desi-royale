using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Actor : MonoBehaviour
{
    private static readonly int Throw = Animator.StringToHash("Throw Object");

    //State Machine
    StateMachine _stateMachine;
    
    //References
    private NavMeshAgent  _agent;
    private Animator _animator;
    public LineOfSight LineOfSight { get; private set; }

    [SerializeField] private ActorType currentActor;
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        LineOfSight = GetComponent<LineOfSight>();
        
        _stateMachine = new StateMachine();

        var patrolState = new PatrolState(this, _animator);
        var hostileState = new HostileState(this,_animator);
        var throwState = new ThrowState(this, _animator);
        
        At(hostileState, patrolState, new FuncPredicate(() => playerInFOV));
        At(throwState,hostileState, new FuncPredicate(() => PlayerDetected && playerInFOV));
        
        
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
    
    private static readonly int MoveSpeed = Animator.StringToHash("Speed");

    public void Patrol()
    {
        if (playerInFOV)
        {
            StartCoroutine(PlayerDetection());
        }
        else
        {
            _agent.isStopped = false;
            StartCoroutine(LineOfSight.PlayerIsVisible());
            _animator.SetFloat(MoveSpeed, _agent.velocity.magnitude);
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            {
                patrolTimer += Time.deltaTime;
                if (patrolTimer >= patrolDelay)
                {
                    _agent.SetDestination(GetPatrolPosition(transform.position, patrolRadius, -1));
                    patrolTimer = 0f;
                }
            }
        }

    }

    Vector3 GetPatrolPosition(Vector3 origin, float distance, int layerMask){

        Vector3 patrolDirection = Random.insideUnitSphere * distance;
        patrolDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(patrolDirection, out navHit, distance, layerMask);
        return navHit.position;
    }

    #endregion
    private Transform _target;
    public bool PlayerDetected { get; private set; }
    [SerializeField] private float detectionTime;

    
    private IEnumerator PlayerDetection()
    {

        while (playerInFOV && !PlayerDetected)
        {
            _agent.isStopped = true;
            detectionTime -= Time.deltaTime;
            yield return new WaitUntil(() => detectionTime <= 0);
            PlayerDetected = true;
            playerInFOV = false;
            detectionTime = 2f;
            StopAllCoroutines();
        }

        yield return null;
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

}
