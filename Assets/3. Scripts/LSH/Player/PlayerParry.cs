using UnityEngine;

public class PlayerParry : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerParry(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 0.8f;   // 총 길이
    private const float parryTime = 0.01f;   // 패링 시간
    private float _elapsedTime;
    private float adjustedParryTime;
    public void Enter()
    {
        switch(DBManager.I.currData.difficulty)
        {
            case 0:
            adjustedParryTime = parryTime * 1.5f;
            break;
            case 1:
            adjustedParryTime = parryTime * 0.8f;
            break;
            case 2:
            adjustedParryTime = parryTime * 0.5f;
            break;
        }
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
        if(_elapsedTime > adjustedParryTime)
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
