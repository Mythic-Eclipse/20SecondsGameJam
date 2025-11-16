using UnityEngine;
using UnityEngine.InputSystem;
using static CollisionState;

public class SlidingWallState : MovementState
{
    private float _slidingSpeed;
    private float _slidingDeceleration;

    public SlidingWallState(PlayerController PlayerController) : base(PlayerController) { }

    public override void Enter()
    {
        _slidingSpeed = -_controller.MovementConfig.SlidingWallSpeed;
        _slidingDeceleration = _controller.MovementConfig.SlidingWallDeceleration;
    }

    public override void UpdateState()
    {
        base.UpdateState();
        ApplyWallSliding();
    }

    public override bool ApplyStateTransition()
    {
        if (_isPendingJump)
        {
            _controller.SetMovementState(MovementStateType.JUMPING);
            _isPendingJump = false;
            return true;
        }

        if (_controller.CollisionDetector.HasCornerGrabContacts() && _controller.GrabCooldown <= 0f
            && CollisionState(GRABBING_ANGLE))
        {
            _controller.SetMovementState(MovementStateType.GRABANGLE);
            return true;
        }

        if (!_controller.CollisionDetector.HasFullWallContacts() && !CollisionState(GROUNDED))
        {
            _controller.SetMovementState(MovementStateType.FALLING);
            return true;
        }

        if (CollisionState(GROUNDED) && _controller.Rigidbody.linearVelocityX == 0)
        {
            _controller.OnGroundStep();
            _controller.SetMovementState(MovementStateType.IDLE);
            return true;
        }

        if (CollisionState(GROUNDED) && _controller.Rigidbody.linearVelocityX != 0)
        {
            _controller.OnGroundStep();
            _controller.SetMovementState(MovementStateType.RUNNING);
            return true;
        }

        return false;
    }

    public override void ApplyJump()
    {
        _isPendingJump = true;
    }

    private void ApplyWallSliding()
    {
        if (_controller.Rigidbody.linearVelocityY > _slidingSpeed)
        {
            _controller.Rigidbody.linearVelocityY -= _slidingDeceleration;
        }

        if (_controller.Rigidbody.linearVelocityY < _slidingSpeed)
        {
            _controller.Rigidbody.linearVelocityY += _slidingDeceleration;
        }
    }
}