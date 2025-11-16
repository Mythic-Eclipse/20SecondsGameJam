using UnityEngine;
using static CollisionState;

public class SlidingSlopeState : MovementState
{
    public SlidingSlopeState(PlayerController PlayerController) : base(PlayerController) { }

    public override void Enter()
    {
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    public override void UpdateState()
    {
        base.UpdateState();
        ApplySlopeSliding();
    }

    public override bool ApplyStateTransition()
    {
        if (_isPendingJump)
        {
            _controller.SetMovementState(MovementStateType.JUMPING);
            _isPendingJump = false;
            return true;
        }
        if (!CollisionState(ON_SLIDING_SLOPE) && !CollisionState(GROUNDED))
        {
            _controller.SetMovementState(MovementStateType.FALLING);
            return true;
        }
        if (!CollisionState(ON_SLIDING_SLOPE) && CollisionState(GROUNDED))
        {
            _controller.SetMovementState(MovementStateType.RUNNING);
            return true;
        }
        return false;
    }

    public override void ApplyJump()
    {
        _isPendingJump = true;
    }

    private void ApplySlopeSliding()
    {
        Vector2 slopeNormal = _controller.CollisionDetector.SlopeNormal;
        Vector2 slopeDirection = Vector2.Perpendicular(slopeNormal) * -Mathf.Sign(slopeNormal.x);

        float gravity = _controller.MovementConfig.Gravity;
        float acceleration = gravity * Mathf.Abs(slopeNormal.x);

        if (_controller.Rigidbody.linearVelocity.magnitude < _controller.MovementConfig.MaxSlopeSlidingSpeed ||
            Mathf.Sign(_controller.Rigidbody.linearVelocityX) != Mathf.Sign(slopeDirection.x))
        {
            _controller.Rigidbody.linearVelocity += slopeDirection * acceleration * Time.fixedDeltaTime;
        }
    }
}