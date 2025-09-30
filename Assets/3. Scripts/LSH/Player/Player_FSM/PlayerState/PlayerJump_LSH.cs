using UnityEngine;

public class PlayerJump_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    public PlayerJump_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    { this.ctx = ctx; this.fsm = fsm; }

    public void Enter()
    {
        // 점프 실행(파라미터는 컨트롤러가 자동 업데이트)
        var v = ctx.rb.linearVelocity;
        v.y = ctx.jumpForce;
        ctx.rb.linearVelocity = v;
    }

    public void Exit() { }
    public void PlayerKeyInput() { }

    public void UpdateState()
    {
        if (ctx.rb.linearVelocity.y <= 0f)
            fsm.ChangeState(ctx.fall);

        if (ctx.AttackPressed)
        {
            fsm.ChangeState(ctx.attack);
            return;
        }
    }

    public void UpdatePhysics()
    {
        float speed = ctx.moveSpeed * ctx.airMoveMultiplier;
        ctx.rb.linearVelocity = new Vector2(ctx.XInput * speed, ctx.rb.linearVelocity.y);
        ctx.UpdateFacing(ctx.XInput);
    }
}
