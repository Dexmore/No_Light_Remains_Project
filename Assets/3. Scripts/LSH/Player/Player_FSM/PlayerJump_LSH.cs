using UnityEngine;

public class PlayerJump_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    public PlayerJump_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    {
        this.ctx = ctx;
        this.fsm = fsm;
    }

    public void Enter()
    {
        // 점프 실행
        Vector2 v = ctx.rb.linearVelocity;
        v.y = ctx.jumpForce;
        ctx.rb.linearVelocity = v;

        if (ctx.animator) ctx.animator.Play("Jump");
    }

    public void Exit() { }

    public void PlayerKeyInput() { }

    public void UpdateState()
    {
        // 상승이 끝나면 Fall
        if (ctx.rb.linearVelocity.y <= 0f)
            fsm.ChangeState(ctx.fall);
    }

    public void UpdatePhysics()
    {
        // 공중 이동
        float speed = ctx.moveSpeed * ctx.airMoveMultiplier;
        ctx.rb.linearVelocity = new Vector2(ctx.XInput * speed, ctx.rb.linearVelocity.y);
    }
}
