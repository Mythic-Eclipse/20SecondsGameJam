using UnityEngine;
using static CollisionState;

public class ClimbingAngleState : MovementState
{
    private Vector2 _climbingUpVelocity;
    private Vector2 _climbingSideVelocity;
    private Collider2D _cornerCollider;
    private bool _isGrabbingMovingPlatform;
    private float _climbingTimeCounter;
    private int _wallCollisionDirection;
    private bool _isClimbingUp;


    public ClimbingAngleState(PlayerController PlayerController) : base(PlayerController) { }

    public override void Enter()
    {
        _rigidbody.linearVelocity = _isGrabbingMovingPlatform ? _controller.ExternalVelocity : Vector2.zero;
        _cornerCollider = _controller.CollisionDetector.WallCollision.Hits[0].Hit.collider;
        _isGrabbingMovingPlatform = _cornerCollider.CompareTag("MovingPlatform");
        _climbingTimeCounter = _controller.MovementConfig.WallClimbUpTime;
        _climbingUpVelocity = new Vector2(0f, _controller.MovementConfig.WallClimbUpForce);
        _climbingSideVelocity = new Vector2(_controller.MovementConfig.WallClimbSideForce, -0.8f);
        _wallCollisionDirection = _controller.CollisionDetector.WallCollision.Side == WallCollisionSide.LEFT ? -1 : 1;
    }

    public override void Leave()
    {
        _isClimbingUp = false;
    }

    public override void UpdateState()
    {
        base.UpdateState();
        HandleClimbingTimer();
        ApplyClimbingVelocity();
        ApplyGravity();
    }

    private void HandleClimbingTimer()
    {
        if (_climbingTimeCounter > 0f)
        {
            _climbingTimeCounter -= Time.fixedDeltaTime;
        }
    }

    private void ApplyClimbingVelocity()
    {
        if (!_isClimbingUp)
        {
            _controller.Rigidbody.linearVelocity = _climbingUpVelocity + _controller.ExternalVelocity;
            _isClimbingUp = true;
        }
        if (_isClimbingUp && _climbingTimeCounter <= 0f)
        {
            _controller.Rigidbody.linearVelocity = new Vector2(
                _climbingSideVelocity.x * _wallCollisionDirection,
                _climbingSideVelocity.y
            ) + _controller.ExternalVelocity;
        }
    }

    protected void ApplyGravity()
    {
        _controller.Rigidbody.linearVelocityY -= _controller.MovementConfig.Gravity * Time.fixedDeltaTime;
    }
    
    public override bool ApplyStateTransition()
    {
        if (CollisionState(GROUNDED) && _movementInput.x == 0f)
        {
            _controller.OnGroundStep();
            _controller.SetMovementState(MovementStateType.IDLE);
            ResetExternalVelocity();
            return true;
        }

        if (CollisionState(GROUNDED) && _movementInput.x != 0f)
        {
            _controller.OnGroundStep();
            _controller.SetMovementState(MovementStateType.RUNNING);
            ResetExternalVelocity();
            return true;
        }

        if (_climbingTimeCounter > 0f)
        {
            return false;
        }

        if (CollisionState(GROUNDED) && CollisionState(TOUCHING_WALL))
        {
            _controller.SetMovementState(MovementStateType.SLIDINGWALL);
            return true;
        }

        return false;
    }
}