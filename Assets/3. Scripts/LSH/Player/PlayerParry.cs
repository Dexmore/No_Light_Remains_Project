using UnityEngine;

public class PlayerParry : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerParry(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 0.6f;   // 총 길이
    private const float parryTime = 0.3f;   // 패링 시간
    public void Enter()
    {
        _elapsedTime = 0f;
        ctx.animator.Play("Player_Parry");
        ctx.Parred = true;
        switch(DBManager.I.currData.difficulty)
        {
            case 0:
            adjustedTime1 = parryTime * 1.3f + 0.12f;
            adjustedTime2 = duration + 0.2f;
            break;
            case 1:
            adjustedTime1 = parryTime * 0.8f + 0.05f;
            adjustedTime2 = duration * 1.1f;
            break;
            case 2:
            adjustedTime1 = parryTime * 0.55f;
            adjustedTime2 = duration * 1.2f;
            break;
        }
    }
    public void Exit()
    {
        ctx.Parred = false;
    }
    private float _elapsedTime;
    private float adjustedTime1;
    private float adjustedTime2;
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if(_elapsedTime > adjustedTime1)
        {
            ctx.Parred = false;
        }
        if(_elapsedTime > adjustedTime2)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }
}
