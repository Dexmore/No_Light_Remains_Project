using UnityEngine;

public class PlayerDash_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerDash_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 0.5f;   // 총 길이
    private const float avoidTime = 0.38f;   // 무적 시간
    private const float dashForce = 18f;   // 대시 세기
    private float _elapsedTime;
    public bool isLeft;
    bool once;
    public void Enter()
    {
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
        if (_elapsedTime > avoidTime)
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
