using UnityEngine;

public class PlayerIdle_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    public PlayerIdle_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    { this.ctx = ctx; this.fsm = fsm; }

    public void Enter() { /* 애니메이션은 파라미터로 처리 */ }
    public void Exit()  { }

    public void PlayerKeyInput()
    {
        if (Mathf.Abs(ctx.XInput) > 0.01f) fsm.ChangeState(ctx.run);
        if (ctx.JumpPressed && ctx.Grounded) fsm.ChangeState(ctx.jump);
        ctx.UpdateFacing(ctx.XInput);
    }

    public void UpdateState()
    {
        if (!ctx.Grounded && ctx.rb.linearVelocity.y < -0.1f)
            fsm.ChangeState(ctx.fall);

        if (ctx.AttackPressed)
        {
            fsm.ChangeState(ctx.attack);
            return;
        }  
    }

    public void UpdatePhysics() { }
}
