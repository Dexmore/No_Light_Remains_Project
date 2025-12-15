using UnityEngine;
public class PlayerStop : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerStop(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    float startTime = 0f;
    public void Enter()
    {
        startTime = Time.time;
        ctx.animator.Play("Player_Idle");
            
    }
    public void Exit()
    {

    }
    public void UpdateState()
    {
        if (Time.time - startTime > 1.5f && !GameManager.I.isOpenPop && !GameManager.I.isOpenDialog)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }

}
