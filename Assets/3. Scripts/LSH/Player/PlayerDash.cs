using UnityEngine;

public class PlayerDash : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerDash(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 0.5f;   // 총 길이
    private const float avoidTime = 0.33f;   // 무적 시간
    private const float dashForce = 18f;   // 대시 세기
    private float _elapsedTime;
    public bool isLeft;
    bool once;
    private float adjustedAvoidTime;
    public void Enter()
    {
        switch(DBManager.I.currData.difficulty)
        {
            case 0:
            adjustedAvoidTime = avoidTime * 1.1f + 0.2f;
            break;
            case 1:
            adjustedAvoidTime = avoidTime * 0.9f + 0.1f;
            break;
            case 2:
            adjustedAvoidTime = avoidTime * 0.8f;
            break;
        }
        _elapsedTime = 0f;
    }
    public void Exit()
    {
        ctx.Avoided = false;
        ctx.isDash = false;
        once = false;
    }
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > 0.02f && !once)
        {
            once = true;
            ctx.animator.Play("Player_Dash");
            AudioManager.I.PlaySFX("Dash");
            if (isLeft)
            {
                ctx.childTR.localRotation = Quaternion.Euler(0f, 180f, 0f);
                ctx.rb.AddForce(Vector2.left * dashForce, ForceMode2D.Impulse);
            }
            else
            {
                ctx.childTR.localRotation = Quaternion.Euler(0f, 0f, 0f);
                ctx.rb.AddForce(Vector2.right * dashForce, ForceMode2D.Impulse);
            }
            ctx.Avoided = true;
            ctx.isDash = false;
        }
        if (_elapsedTime > adjustedAvoidTime)
        {
            ctx.Avoided = false;
        }
        if (_elapsedTime > duration)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }
}
