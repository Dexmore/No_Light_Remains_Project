using UnityEngine;

public class PlayerJumpAttack_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerJumpAttack_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    public void Enter()
    {

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
