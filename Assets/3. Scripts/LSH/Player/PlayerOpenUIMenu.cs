using UnityEngine;
public class PlayerOpenUIMenu : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerOpenUIMenu(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    float startTime = 0f;
    public void Enter()
    {
        startTime = Time.time;
    }
    public void Exit()
    {
        
    }
    public void UpdateState()
    {
        if (Time.time - startTime > 1f && !GameManager.I.isOpenPop && !GameManager.I.isOpenDialog)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }

}
