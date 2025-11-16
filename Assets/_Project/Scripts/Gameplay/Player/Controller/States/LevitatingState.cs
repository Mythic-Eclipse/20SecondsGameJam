using UnityEngine;

public class LevitatingState : MovementState
{
    private float _levitationTimer = 0f;

    public LevitatingState(PlayerController PlayerController) : base(PlayerController) { }

    public override void Enter()
    {
        _controller.Rigidbody.linearVelocity = new Vector2(_controller.Rigidbody.linearVelocityX, 0);
    }
    
    public override void UpdateState()
    {
        base.UpdateState();
        _levitationTimer += Time.fixedDeltaTime * 3f;
        float offset = Mathf.Sin(_levitationTimer) * 1f;
        _controller.Rigidbody.linearVelocity = new Vector2(Mathf.MoveTowards(_controller.Rigidbody.linearVelocityX, 0, 0.3f), offset);
    }

    public override void ApplyJump()
    {
    }

    public override bool ApplyStateTransition()
    {
        return false;
    }
}
