using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using static CollisionState;

/// <summary>
/// Player Controller manages the player's basic movement (without bracer abilities) using state machines.
/// </summary>
public class PlayerController : MonoBehaviour
{
    private MovementConfig _movementConfig;
    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private CollisionDetector _collisionDetector;
    private PlayerInput _input;
    private Dictionary<MovementStateType, MovementState> _states;
    private MovementState _currentState, _previousState;

    public MovementState CurrentMovementState => _currentState;
    public MovementState PreviousMovementState => _previousState;
    public Rigidbody2D Rigidbody => _rigidbody;
    public MovementConfig MovementConfig => _movementConfig;
    public CollisionDetector CollisionDetector => _collisionDetector;
    public PlayerInput Input => _input;
    public Vector2 ExternalVelocity { get; set; } = Vector2.zero;
    public Vector2 AppliedExternalVelocity { get; set; } = Vector2.zero;
    public event Action OnGroundStepEvent, OnJumpFromPlatformEvent;
    public int LookingDirection { get; set; } = 1;
    public float GrabCooldown { get; set; }
    public float DragModifier { get; set; } = 1f;
    public float DragResetStep { get; set; }

    public void Awake()
    {
        _movementConfig = Resources.Load<MovementConfig>("Configs/MovementConfig");
        _input = GetComponent<PlayerInput>();
        _animator = GetComponentInChildren<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _collisionDetector = GetComponent<CollisionDetector>();
        _states = new Dictionary<MovementStateType, MovementState> {
                { MovementStateType.IDLE, new IdleState(this) },
                { MovementStateType.RUNNING, new RunningState(this) },
                { MovementStateType.JUMPING, new JumpState(this) },
                { MovementStateType.FALLING, new FallingState(this) },
                { MovementStateType.LEVITATING, new LevitatingState(this) },
                { MovementStateType.SLIDINGSLOPE, new SlidingSlopeState(this) },
                { MovementStateType.SLIDINGWALL, new SlidingWallState(this) },
                { MovementStateType.GRABANGLE, new GrabAngleState(this) },
                { MovementStateType.CLIMBINGANGLE, new ClimbingAngleState(this) },
                { MovementStateType.SLIDING, new SlidingState(this) }
            };
        SetMovementState(MovementStateType.IDLE);
    }

    private void Start()
    {
        _input.actions["Jump"].performed += ctx => _currentState.ApplyJump();
    }

    private void OnGUI()
    {
        if (_movementConfig.DebugMode)
        {
            DrawDebugInfo();
        }
    }

    private void FixedUpdate()
    {
        HandleLookingDirection();
        HandleDragModifier();
        _collisionDetector.CheckCollisions();
        _currentState.UpdateExternalVelocity();
        _currentState.UpdateState();
        _currentState.ApplyStateTransition();
        HandleCooldowns();
    }

    private void OnDisable()
    {
        _input.actions["Jump"].performed -= ctx => _currentState.ApplyJump();
    }

    // Switch the current movement state to a new one
    public void SetMovementState(MovementStateType stateType)
    {
        if (_states.TryGetValue(stateType, out MovementState newState))
        {
            if (_currentState != null)
            {
                _currentState.Leave();
                // _animator.SetBool(_currentState.GetType().Name, false);
                Debug.Log($"State changed from {_currentState.GetType().Name} to {newState.GetType().Name}");
                _previousState = _currentState;
            }
            _currentState = newState;
            // _animator.SetBool(_currentState.GetType().Name, true);
            _currentState.Enter();

        }
        else
        {
            Debug.LogError($"Movement state {stateType} not found in dictionary.");
        }
    }

    // Handle the ground step event
    public void OnGroundStep()
    {
        OnGroundStepEvent?.Invoke();
        DragModifier = 1f;
    }

    // Handle the ground step event
    public void OnJumpFromPlatform()
    {
        OnJumpFromPlatformEvent?.Invoke();
    }

    private void HandleCooldowns()
    {
        if (GrabCooldown > 0f && _currentState is not GrabAngleState)
        {
            GrabCooldown -= Time.fixedDeltaTime;
        }
    }

    private void HandleLookingDirection()
    {
        Vector2 inputVector = _input.actions["Move"].ReadValue<Vector2>();
        if (inputVector.x > 0) LookingDirection = 1;
        else if (inputVector.x < 0) LookingDirection = -1;
    }

    // Draw debug information on the screen
    private void DrawDebugInfo()
    {
        if (_collisionDetector == null) return;

        string CollisionDebugText = $"Grounded: {_collisionDetector.CollisionStates[GROUNDED]}\n" +
                            $"Ground Distance: {_collisionDetector.GroundDistance}\n" +
                            $"Is On Moving Platform: {_collisionDetector.CollisionStates[ON_MOVING_PLATFORM]}\n" +
                            $"Is On One Way Platform: {_collisionDetector.CollisionStates[ON_ONE_WAY_PLATFORM]}\n" +
                            $"Touching Wall: {_collisionDetector.CollisionStates[TOUCHING_WALL]}\n" +
                            $"Wall collision: {_collisionDetector.WallCollision.Side}, {_collisionDetector.WallCollision.Distance}\n" +
                            $"On Slope: {_collisionDetector.CollisionStates[ON_SLOPE]}\n" +
                            $"On Sliding Slope: {_collisionDetector.CollisionStates[ON_SLIDING_SLOPE]}\n" +
                            $"Slope Normal: {_collisionDetector.SlopeNormal}\n" +
                            $"Slope Angle: {_collisionDetector.SlopeAngle}\n" +
                            $"Linear Velocity: {_rigidbody.linearVelocity}\n" +
                            $"External Velocity: {ExternalVelocity}\n" +
                            $"Magnitude: {_rigidbody.linearVelocity.magnitude}\n" +
                            $"Rigidbody type: {_rigidbody.bodyType}\n" +
                            $"Drag Modifier: {DragModifier}\n";


        Vector3 screenPosition = new Vector3(10f, Screen.height - 10f, 0f);
    }

    private void HandleDragModifier()
    {
        if (DragModifier != 1f)
        {
            DragModifier = Mathf.MoveTowards(DragModifier, 1f, Time.fixedDeltaTime * DragResetStep);
        }
        else
        {
            DragResetStep = _movementConfig.DragResetStep;
        }
    }
}