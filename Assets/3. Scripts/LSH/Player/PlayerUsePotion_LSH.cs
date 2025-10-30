using UnityEngine;

public class PlayerUsePotion_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerUsePotion_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 0.5f;   // 총 길이
    private float _elapsedTime;
    public void Enter()
    {
        _elapsedTime = 0f;
    }
    public void Exit()
    {
        
    }
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if(_elapsedTime > duration)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }
}
