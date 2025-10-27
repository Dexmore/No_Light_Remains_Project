using UnityEngine;

public class PlayerJump_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    public PlayerJump_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    { this.ctx = ctx; this.fsm = fsm; }

    public void Enter()
    {
        // 지상 점프 진입 시 점프력 적용(네 프로젝트 기준에 맞춰 유지)
        ctx.rb.linearVelocity = new Vector2(ctx.rb.linearVelocity.x, ctx.jumpForce);
        ctx.animator?.ResetTrigger("Jump");
        ctx.animator?.SetTrigger("Jump");
        ctx.StartCoroutine(ctx.ResetTriggerNextFrame("Jump"));
    }

    public void Exit() { }

    public void PlayerKeyInput()
    {
        if (ctx.JumpPressed && !ctx.Grounded && ctx.CanAirJump())
        {
            ctx.DoAirJump();
            // 상태 유지(상승 상태) 또는 재진입 둘 다 가능.
            fsm.ChangeState(this);
            return;
        }
    }

    public void UpdateState()
    {
        // 상승→하강 전환 감지 시 Fall로
        if (ctx.rb.linearVelocity.y <= 0f)
            fsm.ChangeState(ctx.fall);
    }

    public void UpdatePhysics()
    {
        float speed = ctx.Grounded ? ctx.moveSpeed : ctx.moveSpeed * ctx.airMoveMultiplier;
        ctx.rb.linearVelocity = new Vector2(ctx.XInput * speed, ctx.rb.linearVelocity.y);
        ctx.UpdateFacing(ctx.XInput);
    }
}
