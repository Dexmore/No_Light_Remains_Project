using UnityEngine;

public class PlayerFall_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    public PlayerFall_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    {
        this.ctx = ctx;
        this.fsm = fsm;
    }

    public void Enter()
    {
        if (ctx.animator) ctx.animator.Play("Fall");
    }

    public void Exit() { }

    public void PlayerKeyInput() { }

    public void UpdateState()
    {
        if (ctx.Grounded)
        {
            if (Mathf.Abs(ctx.XInput) > 0.01f) fsm.ChangeState(ctx.run);
            else                               fsm.ChangeState(ctx.idle);
        }
    }

    public void UpdatePhysics()
    {
        float speed = ctx.moveSpeed * ctx.airMoveMultiplier;
        ctx.rb.linearVelocity = new Vector2(ctx.XInput * speed, ctx.rb.linearVelocity.y);
    }
}
