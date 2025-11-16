using UnityEngine;
using static CollisionState;

public class SlidingState : MovementState
{
    public SlidingState(PlayerController PlayerController) : base(PlayerController)
    { }

    public override void Enter()
    {
        SetInitialParameters();
        if (CollisionState(ON_MOVING_PLATFORM))
        {
            _movingPlatform = _controller.CollisionDetector.GroundHit.collider.GetComponent<MovingPlatform>();
        }

        if (CollisionState(ON_ONE_WAY_PLATFORM))
        {
            _oneWayPlatform = _controller.CollisionDetector.GroundHit.collider.GetComponent<OneWayPlatform>();
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (CollisionState(ON_ONE_WAY_PLATFORM) && _movementInput.y < 0)
        {
            _oneWayPlatform.SwitchCollider(false);
        }

        ApplyGravity();
        ApplyHorizontalMovement();
    }

    public override bool ApplyStateTransition()
    {
        if (_isPendingJump)
        {
            _controller.OnJumpFromPlatform();
            _controller.SetMovementState(MovementStateType.JUMPING);
            ResetExternalVelocity();
            _isPendingJump = false;
            return true;
        }

        if (!CollisionState(GROUNDED) && (_controller.CollisionDetector.GroundDistance > _controller.MovementConfig.KeepRunningDistance || _controller.CollisionDetector.GroundDistance == -1f))
        {
            _controller.SetMovementState(MovementStateType.FALLING);
            ResetExternalVelocity();
            return true;
        }

        if (CollisionState(TOUCHING_WALL) &&
        ((_movementInput.x > 0 && _controller.CollisionDetector.WallCollision.Side == WallCollisionSide.RIGHT) ||
        (_movementInput.x < 0 && _controller.CollisionDetector.WallCollision.Side == WallCollisionSide.LEFT)))
        {
            _controller.SetMovementState(MovementStateType.IDLE);
            return true;
        }

        if (_movementInput.x == 0 && CollisionState(GROUNDED) &&
        _controller.Rigidbody.linearVelocityX - _controller.ExternalVelocity.x == 0)
        {
            _controller.SetMovementState(MovementStateType.IDLE);
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

    private void SetInitialParameters()
    {
        if (_controller.PreviousMovementState is SlidingSlopeState)
        {
            _controller.DragModifier = _controller.MovementConfig.SlopeSlidingFallDrag;
            _controller.DragResetStep = 0.15f;
        }
    }

    private void ApplyGravity()
    {
        if (!CollisionState(GROUNDED))
        {
            _controller.Rigidbody.linearVelocityY += -_controller.MovementConfig.Gravity * Time.fixedDeltaTime;
        }
    }

    private void ApplyHorizontalMovement()
    {
        float inputAxisX = _movementInput.x;
        float acceleration = _controller.MovementConfig.SlidingAcceleration;
        float deceleration = _controller.MovementConfig.SlidingDeceleration;
        float moveSpeed = _controller.MovementConfig.MinSlidingSpeed;

        Vector2 moveDirection;

        if (CollisionState(ON_SLOPE))
        {
            moveDirection = new Vector2(_controller.CollisionDetector.SlopeNormal.y, -_controller.CollisionDetector.SlopeNormal.x).normalized * inputAxisX;
        }
        else
        {
            moveDirection = Vector2.right * inputAxisX;
        }

        if (_controller.CollisionDetector.GroundDistance > 0 && _controller.CollisionDetector.GroundDistance < _controller.MovementConfig.KeepRunningDistance)
        {
            moveDirection += Vector2.down / 2;
        }

        if (_controller.AppliedExternalVelocity != Vector2.zero)
        {
            _rigidbody.linearVelocity -= _controller.AppliedExternalVelocity;
            _controller.AppliedExternalVelocity = Vector2.zero;
        }

        if (Mathf.Abs(inputAxisX) > 0.1f)
        {
            if (Mathf.Sign(inputAxisX) != Mathf.Sign(_rigidbody.linearVelocityX))
            {
                _controller.Rigidbody.linearVelocity = Vector2.MoveTowards(
                    _controller.Rigidbody.linearVelocity,
                    new Vector2(0, 0),
                    10f * Time.fixedDeltaTime
                ) + new Vector2(_controller.ExternalVelocity.x, _controller.ExternalVelocity.y < 0 ? _controller.ExternalVelocity.y : 0);
            }
            else
            {
                Vector2 targetVelocity = moveDirection * _controller.MovementConfig.MaxSlidingSpeed;
                _controller.Rigidbody.linearVelocity = new Vector2(targetVelocity.x, targetVelocity.y) +
                                                new Vector2(_controller.ExternalVelocity.x, _controller.ExternalVelocity.y < 0 ? _controller.ExternalVelocity.y : 0);
            }

        }
        else
        {
            Vector2 targetVelocity = moveDirection * moveSpeed;
            _controller.Rigidbody.linearVelocity = Vector2.MoveTowards(
                    _controller.Rigidbody.linearVelocity,
                    new Vector2(targetVelocity.x, targetVelocity.y),
                    deceleration * Time.fixedDeltaTime
                ) + new Vector2(_controller.ExternalVelocity.x, _controller.ExternalVelocity.y < 0 ? _controller.ExternalVelocity.y : 0);
        }
            _controller.AppliedExternalVelocity = _controller.ExternalVelocity;
    }
}