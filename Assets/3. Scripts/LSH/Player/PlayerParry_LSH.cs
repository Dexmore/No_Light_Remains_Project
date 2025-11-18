using UnityEngine;

public class PlayerParry_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerParry_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 0.8f;   // 총 길이
    private const float parryTime = 0.5f;   // 패링 시간
    private float _elapsedTime;
    public void Enter()
    {
        _elapsedTime = 0f;
        ctx.animator.Play("Player_Parry");
        ctx.Parred = true;
    }
    public void Exit()
    {
        ctx.Parred = false;
    }
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if(_elapsedTime > parryTime)
        {
            ctx.Parred = false;
        }
        if(_elapsedTime > duration)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }
}
