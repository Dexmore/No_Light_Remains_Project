using UnityEngine;

public class PlayerParry : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerParry(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 0.6f;   // 총 길이
    private const float parryTime = 0.22f;   // 패링 시간
    private float _elapsedTime;
    public void Enter()
    {
        flag1 = false;
        isSuccess = false;
        GameManager.I.onParry += ParrySuccessHandler;
        _elapsedTime = 0f;
        ctx.animator.Play("Player_Parry");
        ctx.Parred = true;
        switch (DBManager.I.currData.difficulty)
        {
            case 0:
                adjustedTime2 = duration;
                adjustedTime1 = parryTime * 1.2f + 0.1f;
                break;
            case 1:
                adjustedTime2 = duration * 1.05f + 0.03f;
                adjustedTime1 = parryTime * 0.8f + 0.05f;
                break;
            case 2:
                adjustedTime2 = duration * 1.1f + 0.06f;
                adjustedTime1 = parryTime * 0.7f;
                break;
        }
        //Gear 기어 (격퇴의 기어) 009_ParryGear
        bool outValue = false;
        if (DBManager.I.HasGear("009_ParryGear", out outValue))
        {
            if (outValue)
            {
                adjustedTime1 *= 1.4f + 0.15f;

            }
        }
    }
    public void Exit()
    {
        ctx.Parred = false;
        GameManager.I.onParry -= ParrySuccessHandler;
        flag1 = false;
    }
    void ParrySuccessHandler(HitData hitData)
    {
        isSuccess = true;
    }
    bool isSuccess;

    private float adjustedTime1;
    private float adjustedTime2;
    bool flag1 = false;
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > adjustedTime1)
        {
            ctx.Parred = false;
            if (!flag1 && !isSuccess)
            {
                Debug.Log("Fail");
                flag1 = true;
            }
        }
        if (_elapsedTime > adjustedTime2)
        {
            fsm.ChangeState(ctx.idle);
        }
        if (!flag1 && isSuccess)
        {
            Debug.Log("Success");
            flag1 = true;
        }
    }
    public void UpdatePhysics()
    {

    }
}
