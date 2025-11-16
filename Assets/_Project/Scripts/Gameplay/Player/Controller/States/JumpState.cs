using UnityEngine;
using static CollisionState;

public class JumpState : MovementState
{
    private Vector2 _jumpVelocity;
    private bool _isJumpingUp;
    private float _acceleration;
    private float _deceleration;

    public JumpState(PlayerController PlayerController) : base(PlayerController) { }

    public override void Enter()
    {
        SetInitialJumpParameters();
        ApplyDragModifier();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        ApplyGravity();
        ApplyDragModifier();
        ApplyHorizontalMovement();
    }

    public override bool ApplyStateTransition()
    {
        if (_isPendingJump)
        {
            _isPendingJump = false;
            _controller.SetMovementState(MovementStateType.JUMPING);
            return true;
        }
        if (_controller.CollisionDetector.HasFullWallContacts())
        {
            _controller.SetMovementState(MovementStateType.SLIDINGWALL);
            return true;
        }
        if (_controller.CollisionDetector.HasCornerGrabContacts() && _controller.GrabCooldown <= 0f && CollisionState(GRABBING_ANGLE))
        {
            _controller.SetMovementState(MovementStateType.GRABANGLE);
            return true;
        }
        if (_controller.Rigidbody.linearVelocityY <= 0 && _isJumpingUp)
        {
            _isJumpingUp = false;
            _controller.SetMovementState(MovementStateType.FALLING);
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
        if (CollisionState(TOUCHING_WALL))
        {
            _isPendingJump = true;
            return;
        }
        PlayerEventBus.Instance.OnJumpAbilityRequested?.Invoke();
    }

    private void SetInitialJumpParameters()
    {
        _jumpVelocity = Vector2.up;
        _isJumpingUp = true;

        if (_controller.PreviousMovementState is LevitatingState)
        {
            _controller.DragModifier = _controller.MovementConfig.AirWaveDrag;
        }

        if (_controller.PreviousMovementState is GrabAngleState)
        {
            if (_movementInput.x > 0 && _controller.CollisionDetector.WallCollision.Side == WallCollisionSide.LEFT
            ||
            _movementInput.x < 0 && _controller.CollisionDetector.WallCollision.Side == WallCollisionSide.RIGHT)
            {
                _jumpVelocity = new Vector2(_movementInput.x, 1f) * _controller.MovementConfig.WallJumpForce;
            }
        }

        if (_controller.PreviousMovementState is SlidingSlopeState)
        {
            _jumpVelocity = _controller.CollisionDetector.SlopeNormal * _controller.MovementConfig.JumpForce;

            if (_controller.Rigidbody.linearVelocity.magnitude >= _controller.MovementConfig.MaxSlopeSlidingSpeed)
            {
                _jumpVelocity += new Vector2(_controller.Rigidbody.linearVelocityX / 2, 0);
            }

            _controller.DragModifier = _controller.MovementConfig.SlopeSlidingJumpDrag;
        }

        if (_controller.PreviousMovementState is SlidingWallState)
        {
            float sideModifier = 0f;
            if (_controller.CollisionDetector.WallCollision.Side == WallCollisionSide.LEFT)
            {
                sideModifier = 1.0f;
            }
            else if (_controller.CollisionDetector.WallCollision.Side == WallCollisionSide.RIGHT)
            {
                sideModifier = -1.0f;
            }
            _jumpVelocity = new Vector2(1f * sideModifier, 1f) * _controller.MovementConfig.WallJumpForce;
            _controller.LookingDirection = (int)sideModifier;
            _controller.DragModifier = _controller.MovementConfig.WallJumpDrag;
        }

        if (_jumpVelocity == Vector2.up)
        {
            _controller.Rigidbody.linearVelocityY = _controller.MovementConfig.JumpForce;
        }
        else
        {
            _controller.Rigidbody.linearVelocity = _jumpVelocity;
        }
    }

    private void ApplyDragModifier()
    {
        _acceleration = _controller.MovementConfig.InJumpAcceleration * _controller.DragModifier;
        _deceleration = _controller.MovementConfig.InJumpDeceleration * _controller.DragModifier;
    }

    private void ApplyHorizontalMovement()
    {
        float inputAxisX = _movementInput.x;
        float targetVelocityX = inputAxisX * _controller.MovementConfig.MovementSpeed;

        if (Mathf.Abs(inputAxisX) > 0.1f && Mathf.Sign(inputAxisX) == Mathf.Sign(_controller.Rigidbody.linearVelocityX) || _controller.Rigidbody.linearVelocityX == 0)
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
        if (_isJumpingUp)
        {
            bool isHoldingJump = _controller.Input.actions["Jump"].IsPressed();

            float apexFactor = Mathf.InverseLerp(0, _controller.MovementConfig.JumpForce, _controller.Rigidbody.linearVelocityY);
            float baseGravity = isHoldingJump ? _controller.MovementConfig.Gravity * 0.5f : _controller.MovementConfig.Gravity * 1.5f;
            float modifiedGravity = Mathf.Lerp(baseGravity, _controller.MovementConfig.Gravity, apexFactor);

            _controller.Rigidbody.linearVelocityY -= modifiedGravity * Time.fixedDeltaTime;
        }
    }
}