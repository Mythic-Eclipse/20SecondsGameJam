using UnityEngine;
using System;

public abstract class MovementState
{
    protected PlayerController _controller;
    protected Rigidbody2D _rigidbody;
    protected MovingPlatform _movingPlatform;
    protected OneWayPlatform _oneWayPlatform;
    protected Vector2 _movementInput;
    protected bool _isPendingJump;

    public bool IsPendingJump {get => _isPendingJump; set => _isPendingJump = value; }

    public MovementState(PlayerController PlayerController)
    {
        _controller = PlayerController;
        _rigidbody = _controller.Rigidbody;
    }
    public virtual void UpdateExternalVelocity()
    {
        if (_movingPlatform != null) _controller.ExternalVelocity = _movingPlatform.MovingVelocity;
    }
    public virtual void Enter() {; }
    public virtual void Leave() {; }
    public virtual void UpdateState() { _movementInput = _controller.Input.actions["Move"].ReadValue<Vector2>(); }
    public virtual void ApplyJump() {; }
    public virtual bool ApplyStateTransition() { return false; }
    protected void ResetExternalVelocity()
    {
        if (_movingPlatform != null) _movingPlatform = null;
        _controller.ExternalVelocity = Vector2.zero;
        _controller.AppliedExternalVelocity = Vector2.zero;
    }
    // Shortcut for checking collision states
    protected bool CollisionState(CollisionState state)
    {
        return _controller.CollisionDetector.CollisionStates[state];
    }
}

public enum MovementStateType
{
    FALLING,
    IDLE,
    JUMPING,
    LEVITATING,
    RUNNING,
    SLIDINGSLOPE,
    SLIDINGWALL,
    STICKTOWALL,
    GRABANGLE,
    CLIMBINGANGLE,
    SLIDING
}