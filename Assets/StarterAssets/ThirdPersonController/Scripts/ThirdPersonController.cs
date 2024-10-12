<<<<<<< HEAD
﻿ using System;
 using System.Collections.Generic;
 using Cinemachine;
 using Unity.Netcode;
 using UnityEngine;
 using Random = UnityEngine.Random;
=======
﻿ using Unity.VisualScripting;
 using UnityEngine;
 using System.Collections;
 using System.Collections.Generic;
 using UnityEngine.Jobs;
>>>>>>> cover-controller
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : NetworkBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        [SerializeField] PlayerInput _playerInput;
#endif
        private ThrowController _throwController;
        private PlayerInteraction _playerInteraction;
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        [SerializeField] private GameObject _mainCamera;

        private const float _threshold = 0.01f;
        
        private bool _hasAnimator;

        public struct InputPayload : INetworkSerializable
        {
            public int tick;
            public Vector3 inputVector;
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref tick);
                serializer.SerializeValue(ref inputVector);
            }
        }

        public struct StatePayload : INetworkSerializable
        {
            public int tick;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 velocity;
            public Vector3 angularVelocity;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref tick);
                serializer.SerializeValue(ref position);
                serializer.SerializeValue(ref rotation);
                serializer.SerializeValue(ref velocity);
                serializer.SerializeValue(ref angularVelocity);

            }
        }
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }
        
        [SerializeField] private CinemachineVirtualCamera _playerVirtualCamera;
        [SerializeField] private AudioListener _playeraudioListener;


        private NetworkTimer timer;
        private const float k_serverTickRate = 60f;
        private const int k_bufferSize = 1024;


        private CircularBuffer<StatePayload> clientStateBuffer;
        private CircularBuffer<InputPayload> clientInputBuffer;
        
        StatePayload lastServerState;
        private StatePayload lastProcessedState;
        
        CircularBuffer<StatePayload> serverStateBuffer;
        Queue<InputPayload> serverInputQueue;
        
        private void Awake()
        {
            // get a reference to our main camera
            // if (_mainCamera == null)
            // {
            //     _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            // }
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _throwController = GetComponent<ThrowController>();
            _playerInteraction = GetComponent<PlayerInteraction>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;


            timer = new NetworkTimer(k_serverTickRate);
            clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
            clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

            serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
            serverInputQueue = new Queue<InputPayload>();


        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                _throwController.enabled = false;
                _playeraudioListener.enabled = false;
                _playerVirtualCamera.Priority = 0;
                _playerInput.enabled = false;
                _animator.enabled = false;
                _playerInteraction.enabled = false;
                return;
            }
            
            
            _throwController.enabled = true;
            _playerVirtualCamera.Priority = 100;
            _playeraudioListener.enabled = true;
            _playerInput.enabled = true;
            _playerInteraction.enabled = true;
            _animator.enabled = true;
        }

        private void Update()
        {
            timer.Update(Time.deltaTime);
            _hasAnimator = TryGetComponent(out _animator);
<<<<<<< HEAD
            if (_input.interact)
                _playerInteraction.Interact();
=======
            
            TakeCover();
>>>>>>> cover-controller
            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            if (timer.ShouldTick())
            {
                HandleClientTick();
                HandleServerTick();
            }
        }

        void HandleServerTick()
        {
            var bufferIndex = -1;
            while (serverInputQueue.Count > 0)
            {
                InputPayload inputPayload = serverInputQueue.Dequeue();

                bufferIndex = inputPayload.tick % k_bufferSize;

                StatePayload statePayload = SimulateMovement(inputPayload);
                serverStateBuffer.Add(statePayload, bufferIndex);
            }

            if (bufferIndex == -1) return;
            SendToClientRPC(serverStateBuffer.Get(bufferIndex));
        }

        StatePayload SimulateMovement(InputPayload inputPayload)
        {
            Physics.simulationMode = SimulationMode.Script;
            
            Move();
            Physics.Simulate(Time.fixedDeltaTime);
            Physics.simulationMode = SimulationMode.FixedUpdate;

            return new StatePayload()
            {
                tick = inputPayload.tick,
                position = transform.position,
                rotation = transform.rotation,
                velocity = _controller.velocity,

            };
        }
        
        [ClientRpc]
        void SendToClientRPC(StatePayload statePayload)
        {
            if (!IsOwner) return;

            lastServerState = statePayload;
        }
        void HandleClientTick()
        {
            if (!IsClient) return;
            var currentTick = timer.CurrentTick;
            var bufferIndex = currentTick % k_bufferSize;

            InputPayload payload = new InputPayload()
            {
                tick = currentTick
            };
            
            clientInputBuffer.Add(payload, bufferIndex);
            SendToServerRPC(payload);

            StatePayload statePayload = ProcessMovement(payload);
            clientStateBuffer.Add(statePayload, bufferIndex);
            
            HandleServerReconciliation();
        }

        bool ShouldReconcile()
        {
            bool isNewServerState = !lastServerState.Equals(default);
            bool isLastStateUndefinedorDifferent = lastProcessedState.Equals(default);

            return isNewServerState && isLastStateUndefinedorDifferent;
        }
        
        [SerializeField] private float reconciliationThreshold = 2f;
        void HandleServerReconciliation()
        {
            if (!ShouldReconcile()) return;

            float positionError;
            int bufferIndex;
            StatePayload rewindState = default;
            
            bufferIndex = lastServerState.tick % k_bufferSize;
            if(bufferIndex - 1 > 0) return;
            
            rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState;
            
            positionError = Vector3.Distance(rewindState.position, clientStateBuffer.Get(bufferIndex).position);

            if (positionError > reconciliationThreshold)
            {
                ReconcileState(rewindState);
            }

            lastProcessedState = lastServerState;
        }

        void ReconcileState(StatePayload rewindState)
        {
            transform.position = rewindState.position;
            transform.rotation = rewindState.rotation;
            // _controller. = rewindState.velocity;
            
            if(!rewindState.Equals(lastServerState)) return;
            
            clientStateBuffer.Add(rewindState, rewindState.tick);
            
            
            int ticksToReplay = lastServerState.tick;

            while (ticksToReplay < timer.CurrentTick)
            {
                int bufferIndex = ticksToReplay % k_bufferSize;
                
                StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
                clientStateBuffer.Add(statePayload, bufferIndex);
                ticksToReplay++;
                
            }
        }
        [ServerRpc]
        void SendToServerRPC(InputPayload inputPayload)
        {
            serverInputQueue.Enqueue(inputPayload);
        }
        StatePayload ProcessMovement(InputPayload input)
        {
            Move();

            return new StatePayload()
            {
                tick = input.tick,
                position = transform.position,
                rotation = transform.rotation,
                velocity = _controller.velocity
            };
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error-prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed / 100f, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

    
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            var tickRate = timer.MinTimeBetweenTicks / (1f / Time.deltaTime);
            // move the player
            
            _controller.Move(targetDirection.normalized * (_speed * tickRate) + new Vector3(0.0f, _verticalVelocity, 0.0f) * tickRate);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        [SerializeField] private Transform HighCover;
        [SerializeField] private Transform LowCover;
        [SerializeField] private float maxCoverDistance = 3f;
        [SerializeField] private LayerMask coverLayerMask;
        [SerializeField] private bool isInCover = false;
        // ReSharper disable Unity.PerformanceAnalysis
        void TakeCover()
        {
            if (_input.cover && isInCover)
            {
                ExitCover();
            }
            RaycastHit hit = new RaycastHit();
            if (isInCover) return;
            if (_input.cover && !isInCover)
            {
                var enterHigh = Physics.Raycast(HighCover.position, transform.forward, maxCoverDistance, coverLayerMask);
                var enterLow = Physics.Raycast(LowCover.position, transform.forward,out hit, maxCoverDistance, coverLayerMask);
                if(!enterHigh || !enterLow) return;
                var moveDirection = hit.point - transform.position;
                var moveToDirection = moveDirection.normalized * (MoveSpeed * Time.deltaTime);
                
                StartCoroutine(MoveToCover(moveToDirection, hit.point));            
                //Enter high cover 
                // Cover Animation
                // Walk Animation
                // if(isInCover && enterHigh && enterLow)
            }

            if (hit.collider != null)
            {
                //Return x wil have a value and z will have a value
                //Depending on which is returned we will lock the transform of the normal returned
                if (hit.normal.x != 0)
                {
                    transform.position = new Vector3(hit.point.x, transform.position.y, transform.position.z);
                }
                else if (hit.normal.z != 0)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, hit.point.z);
                }
            }
            //Get the X and Z lengths of the cover and clamp the player to it
            
          
        }

      

        void ExitCover()
        {
            isInCover = false;
        }
        
        private IEnumerator MoveToCover(Vector3 coverDirection, Vector3 destination)
        {
            while (!isInCover)
            {
                if (Vector3.Distance(destination, transform.position) < 1f)
                    isInCover = true;
                _controller.Move(coverDirection);
                _animator.SetFloat(_animIDSpeed, _speed);
                yield return null;
            }
            
            
            yield return new WaitForSeconds(0.5f);
        }
    }
}