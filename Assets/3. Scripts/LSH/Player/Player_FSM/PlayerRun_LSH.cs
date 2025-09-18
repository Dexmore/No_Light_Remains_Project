using UnityEngine;

public class PlayerRun_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    public PlayerRun_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    {
        this.ctx = ctx;
        this.fsm = fsm;
    }

    public void Enter()
    {
        if (ctx.animator) ctx.animator.Play("Run");
    }

    public void Exit() { }

    public void PlayerKeyInput()
    {
        if (Mathf.Abs(ctx.XInput) <= 0.01f)
            fsm.ChangeState(ctx.idle);

        if (ctx.JumpPressed && (ctx.Grounded))
            fsm.ChangeState(ctx.jump);
    }

    public void UpdateState()
    {
        if (!ctx.Grounded && ctx.rb.linearVelocity.y < -0.1f)
            fsm.ChangeState(ctx.fall);
    }

    public void UpdatePhysics()
    {
        float speed = ctx.Grounded ? ctx.moveSpeed
                                   : ctx.moveSpeed * ctx.airMoveMultiplier;

        ctx.rb.linearVelocity = new Vector2(ctx.XInput * speed, ctx.rb.linearVelocity.y);

        // 좌우 뒤집기
        if (Mathf.Abs(ctx.XInput) > 0.01f)
            ctx.transform.localScale = new Vector3(Mathf.Sign(ctx.XInput), 1f, 1f);
    }
}
