using UnityEngine;
using static CollisionState;

public class FallingState : MovementState
{
    private float _coyoteTimeCounter;
    private float _acceleration;
    private float _deceleration;

    public FallingState(PlayerController PlayerController) : base(PlayerController) { }

    private void SetInitialParameters()
    {
        if (_controller.PreviousMovementState is SlidingSlopeState)
        {
            _controller.DragModifier = _controller.MovementConfig.SlopeSlidingFallDrag;
            _controller.DragResetStep = 0.8f;
        }
    }
    
    public override void Enter()
    {
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;

        if (_controller.PreviousMovementState is RunningState
        || _controller.PreviousMovementState is IdleState)
        {
            _coyoteTimeCounter = 0.15f;
        }

        SetInitialParameters();
        ApplyDragModifier();
    }

    public override void Leave()
    {
        _coyoteTimeCounter = 0;
    }

    public override void UpdateState()
    {
        base.UpdateState();
        ApplyGravity();
        ApplyDragModifier();
        ApplyHorizontalMovement();

        if (_coyoteTimeCounter > 0)
        {
            _coyoteTimeCounter -= Time.fixedDeltaTime;
        }
    }

    public override bool ApplyStateTransition()
    {
        if (_isPendingJump)
        {
            _controller.SetMovementState(MovementStateType.JUMPING);
            _isPendingJump = false;
            return true;
        }

        if (CollisionState(ON_SLIDING_SLOPE))
        {
            _controller.SetMovementState(MovementStateType.SLIDINGSLOPE);
            return true;
        }

        if (CollisionState(GROUNDED) && _controller.Rigidbody.linearVelocityX == 0)
        {
            _controller.OnGroundStep();
            _controller.SetMovementState(MovementStateType.IDLE);
            return true;
        }

        if (CollisionState(GROUNDED) && _controller.Rigidbody.linearVelocityX != 0 && !CollisionState(TOUCHING_WALL))
        {
            _controller.OnGroundStep();
            _controller.SetMovementState(MovementStateType.RUNNING);
            return true;
        }

        if (_controller.CollisionDetector.HasFullWallContacts())
        {
            _controller.SetMovementState(MovementStateType.SLIDINGWALL);
            return true;
        }

        if ((_controller.CollisionDetector.HasMiddleCornerGrabContact() || _controller.CollisionDetector.HasCornerGrabContacts()) && _controller.GrabCooldown <= 0f && CollisionState(GRABBING_ANGLE))
        {
            _controller.SetMovementState(MovementStateType.GRABANGLE);
            return true;
        }

        return false;
    }

    public override void ApplyJump()
    {
        if (_coyoteTimeCounter > 0)
        {
            _isPendingJump = true;
        }
        else
        {
             PlayerEventBus.Instance.OnJumpAbilityRequested?.Invoke();
        }
    }

    private void ApplyDragModifier()
    {
        _acceleration = _controller.MovementConfig.InFallAcceleration * _controller.DragModifier;
        _deceleration = _controller.MovementConfig.InFallDeceleration * _controller.DragModifier;
    }

    private void ApplyHorizontalMovement()
    {
        float inputAxisX = _movementInput.x;
        float targetVelocityX = _controller.MovementConfig.MovementSpeed * inputAxisX;

        if ((Mathf.Abs(inputAxisX) > 0.1f && Mathf.Sign(inputAxisX) == Mathf.Sign(_controller.Rigidbody.linearVelocityX)) || _controller.Rigidbody.linearVelocityX == 0)
        {
            _controller.Rigidbody.linearVelocity = new Vector2(
                Mathf.MoveTowards(_controller.Rigidbody.linearVelocityX, targetVelocityX, _acceleration * Time.fixedDeltaTime),
                _controller.Rigidbody.linearVelocityY
            );
        }
        else
        {
            _controller.Rigidbody.linearVelocity = new Vector2(
                Mathf.MoveTowards(_controller.Rigidbody.linearVelocityX, 0, _deceleration * Time.fixedDeltaTime),
                _controller.Rigidbody.linearVelocityY
            );
        }
    }

    private void ApplyGravity()
    {
        if (!CollisionState(GROUNDED) && _controller.Rigidbody.linearVelocityY >= -_controller.MovementConfig.MaxFallSpeed)
        {
            _controller.Rigidbody.linearVelocityY += -_controller.MovementConfig.Gravity * Time.fixedDeltaTime;
        }
    }
}