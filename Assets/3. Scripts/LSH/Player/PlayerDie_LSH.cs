using UnityEngine;

public class PlayerDie_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerDie_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    public void Enter()
    {
        ctx.animator.Play("Player_Die");
    }
    public void Exit()
    {
        
    }
    public void UpdateState()
    {

    }
    public void UpdatePhysics()
    {

    }
}
