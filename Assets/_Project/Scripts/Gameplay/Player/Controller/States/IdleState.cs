using UnityEngine;
using static CollisionState;

public class IdleState : MovementState
{
    public IdleState(PlayerController PlayerController) : base(PlayerController) { }

    public override void Enter()
    {
        if (CollisionState(ON_MOVING_PLATFORM))
        {
            _movingPlatform = _controller.CollisionDetector.GroundHit.collider.GetComponent<MovingPlatform>();
        }
        else
        {
            ResetExternalVelocity();
        }
        
        if (CollisionState(ON_ONE_WAY_PLATFORM))
        {
            _oneWayPlatform = _controller.CollisionDetector.GroundHit.collider.GetComponent<OneWayPlatform>();
        }
        
        if ((_controller.PreviousMovementState is FallingState || _controller.PreviousMovementState is RunningState) && CollisionState(ON_MOVING_PLATFORM))
        {
            _rigidbody.linearVelocity = _controller.ExternalVelocity;
        }
        else
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (CollisionState(ON_ONE_WAY_PLATFORM) && _controller.Input.actions["Move"].ReadValue<Vector2>().y < 0)
        {
            _oneWayPlatform.SwitchCollider(false);
        }

        if (_controller.AppliedExternalVelocity != Vector2.zero)
        {
            _rigidbody.linearVelocity -= _controller.AppliedExternalVelocity;
            _controller.AppliedExternalVelocity = Vector2.zero;
        }
        _rigidbody.linearVelocity += new Vector2(_controller.ExternalVelocity.x,
                                                _controller.ExternalVelocity.y < 0 ? _controller.ExternalVelocity.y : 0);
        _controller.AppliedExternalVelocity = _controller.ExternalVelocity;
    }

    public override bool ApplyStateTransition()
    {
        if (_isPendingJump)
        {
            _controller.OnJumpFromPlatform();
            _controller.SetMovementState(MovementStateType.JUMPING);
            _isPendingJump = false;
            ResetExternalVelocity();
            return true;
        }

        if (!CollisionState(GROUNDED) && _controller.ExternalVelocity == Vector2.zero)
        {
            _controller.SetMovementState(MovementStateType.FALLING);
            ResetExternalVelocity();
            return true;
        }

        if ((_movementInput.x != 0)
        &&
        !(_movementInput.x < 0 && _controller.CollisionDetector.WallCollision.Side == WallCollisionSide.LEFT)
        &&
        (!(_movementInput.x > 0 && _controller.CollisionDetector.WallCollision.Side == WallCollisionSide.RIGHT)))
        {
            _controller.SetMovementState(MovementStateType.RUNNING);
            return true;
        }
        if (CollisionState(ON_SLIDING_SLOPE))
        {
            _controller.SetMovementState(MovementStateType.SLIDINGSLOPE);
            return true;
        }
        return false;
    }

    public override void ApplyJump()
    {
        _isPendingJump = true;
    }

    public override void Leave()
    {
        _oneWayPlatform = null;
    }
}