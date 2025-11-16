using UnityEngine;

public class GrabAngleState : MovementState
{
    private float _grabTimer;
    private bool _isGrabbingMovingPlatform;

    public GrabAngleState(PlayerController PlayerController) : base(PlayerController) { }

    public override void Enter()
    {
        Collider2D cornerCollider = _controller.CollisionDetector.WallCollision.Hits[0].Hit.collider;
        _isGrabbingMovingPlatform = cornerCollider.CompareTag("MovingPlatform");
        if (_isGrabbingMovingPlatform)
        {
            _movingPlatform = cornerCollider.GetComponent<MovingPlatform>();
            _controller.ExternalVelocity = _movingPlatform.MovingVelocity;
        }

        if (_rigidbody.linearVelocityY > 0)
        {
            _rigidbody.linearVelocityY = 0f;
            ApplyJump();
        }
        else
        {
            _controller.Rigidbody.linearVelocity = Vector2.zero + _controller.ExternalVelocity;
            _grabTimer = _controller.MovementConfig.GrabAngleDuration;
        }

    }

    public override void Leave()
    {
        _controller.GrabCooldown = _controller.MovementConfig.GrabCooldown;
        if (_isGrabbingMovingPlatform)
        {
            _controller.ExternalVelocity = Vector2.zero;
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();
        HandleLookingDirection();
        _rigidbody.linearVelocity = _controller.ExternalVelocity;
        _controller.AppliedExternalVelocity = _controller.ExternalVelocity;
    }

    public override void ApplyJump()
    {
        _isPendingJump = true;
    }

    public override bool ApplyStateTransition()
    {
        if (_isPendingJump)
        {
            _isPendingJump = false;

            if (_controller.LookingDirection < 0 && _controller.CollisionDetector.WallCollision.Side == WallCollisionSide.LEFT
            || _controller.LookingDirection > 0 && _controller.CollisionDetector.WallCollision.Side == WallCollisionSide.RIGHT)
            {
                if (_isGrabbingMovingPlatform && _controller.ExternalVelocity != Vector2.zero)
                {
                    _controller.SetMovementState(MovementStateType.JUMPING);
                    ResetExternalVelocity();
                    return true;
                }
                _controller.SetMovementState(MovementStateType.CLIMBINGANGLE);
                ResetExternalVelocity();
                return true;
            }
            else if (_controller.CollisionDetector.HasCornerGrabContacts())
            {
                _controller.SetMovementState(MovementStateType.JUMPING);
                return true;
            }
        }

        if (_grabTimer <= 0f || _movementInput.y < 0f)
        {
            _controller.SetMovementState(MovementStateType.FALLING);
            ResetExternalVelocity();
            return true;
        }
        else
        {
            _grabTimer -= Time.fixedDeltaTime;
        }
        return false;
    }

    private void HandleLookingDirection()
    {
        if (_movementInput.x == 0)
        {
            _controller.LookingDirection = _controller.CollisionDetector.WallCollision.Side == WallCollisionSide.LEFT ? -1 : 1;
        }
    }
}
