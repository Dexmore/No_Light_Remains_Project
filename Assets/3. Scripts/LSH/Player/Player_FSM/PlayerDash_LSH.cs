using UnityEngine;

public class PlayerDash_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    private float _endTime;
    private int _dir; // -1(좌) / +1(우)

    public PlayerDash_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    { this.ctx = ctx; this.fsm = fsm; }

    public void Enter()
    {
        // 입력 방향 우선, 없다면 바라보는 방향
        if (Mathf.Abs(ctx.XInput) > 0.1f) _dir = ctx.XInput > 0 ? 1 : -1;
        else _dir = ctx.transform.localScale.x >= 0 ? 1 : -1;

        ctx.isDashing = true;
        _endTime = Time.time + ctx.dashTime;
        ctx._dashReadyTime = Time.time + ctx.dashCooldown; // 쿨다운 시작

        // 애니메이터 트리거(있다면)
        ctx.animator?.ResetTrigger("Dash");
        ctx.animator?.SetTrigger("Dash");
        ctx.StartCoroutine(ctx.ResetTriggerNextFrame("Dash"));
    }

    public void Exit()
    {
        ctx.isDashing = false;
    }

    public void PlayerKeyInput() { /* 대시 중 입력 무시(원하면 유지) */ }

    public void UpdateState()
    {
        if (Time.time >= _endTime)
        {
            // 끝나면 이동 상태로 복귀
            if (ctx.Grounded)
                fsm.ChangeState(Mathf.Abs(ctx.XInput) > 0.01f ? ctx.run : ctx.idle);
            else
                fsm.ChangeState(ctx.fall);
        }
    }

    public void UpdatePhysics()
    {
        // 수평으로 강제 가속, 수직은 유지
        ctx.rb.linearVelocity = new Vector2(_dir * ctx.dashSpeed, ctx.rb.linearVelocity.y);
        // 바라보는 방향 갱신(선택)
        ctx.UpdateFacing(_dir);
    }
}
