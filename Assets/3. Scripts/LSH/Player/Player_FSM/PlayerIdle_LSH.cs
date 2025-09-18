using UnityEngine;

public class PlayerIdle_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    public PlayerIdle_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    {
        this.ctx = ctx;
        this.fsm = fsm;
    }

    public void Enter()
    {
        // X 속도만 감속 (부드럽게 하려면 Drag 사용)
        ctx.rb.linearVelocity = new Vector2(0f, ctx.rb.linearVelocity.y);
        if (ctx.animator) ctx.animator.Play("Idle", 0, 0f);
    }

    public void Exit() { }

    public void PlayerKeyInput()
    {
        if (Mathf.Abs(ctx.XInput) > 0.01f)
            fsm.ChangeState(ctx.run);

        if (ctx.JumpPressed && ctx.Grounded)
            fsm.ChangeState(ctx.jump);
    }

    public void UpdateState()
    {
        // 떨어지면 Fall
        if (!ctx.Grounded && ctx.rb.linearVelocity.y < -0.1f)
            fsm.ChangeState(ctx.fall);
    }

    public void UpdatePhysics() { }
}
