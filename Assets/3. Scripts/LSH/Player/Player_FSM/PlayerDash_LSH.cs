using UnityEngine;

public class PlayerDash_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    // 타이밍/속도는 PlayerController_LSH의 인스펙터 값(dashTime, dashSpeed, dashCooldown) 사용
    private float _t;
    private int _dir; // -1(왼쪽) / +1(오른쪽)

    // 재사용 버퍼
    private readonly ContactPoint2D[] _contacts = new ContactPoint2D[8];

    public PlayerDash_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    { this.ctx = ctx; this.fsm = fsm; }

    public void Enter()
    {
        _t = 0f;

        // 입력 방향 우선, 없으면 바라보는 방향 사용
        if (Mathf.Abs(ctx.XInput) > 0.1f) _dir = ctx.XInput > 0 ? 1 : -1;
        else _dir = ctx.transform.localScale.x >= 0 ? 1 : -1;

        // 무적/쿨다운 플래그는 컨트롤러에 이미 존재
        ctx.isDashing = true;
        ctx._dashReadyTime = Time.time + ctx.dashCooldown;

        // 애니메이션
        if (ctx.animator)
        {
            ctx.animator.ResetTrigger("Dash");
            ctx.animator.SetTrigger("Dash");
            ctx.StartCoroutine(ctx.ResetTriggerNextFrame("Dash"));
        }
    }

    public void Exit()
    {
        ctx.isDashing = false;
    }

    public void PlayerKeyInput()
    {
        /* 대시 중 입력 무시(원하면 유지) */
    }

    public void UpdateState()
    {
        _t += Time.deltaTime;

        // 대시 종료
        if (_t >= ctx.dashTime)
        {
            if (ctx.Grounded)
                fsm.ChangeState(Mathf.Abs(ctx.XInput) > 0.01f ? ctx.run : ctx.idle);
            else
                fsm.ChangeState(ctx.fall);
        }
    }

    public void UpdatePhysics()
    {
        // 지면 탄젠트 벡터 계산 → 그 방향으로 속도 설정
        Vector2 dashDir = GetGroundAlignedDir(_dir);
        ctx.rb.linearVelocity = new Vector2(dashDir.x * ctx.dashSpeed,
                                            dashDir.y * ctx.dashSpeed);

        // 바라보는 방향 갱신(원하면 유지)
        ctx.UpdateFacing(_dir);
    }

    private Vector2 GetGroundAlignedDir(int sign)
    {
        if (!ctx.Grounded)
            return new Vector2(sign, 0f);

        int c = ctx.rb.GetContacts(_contacts);
        Vector2 bestNormal = Vector2.up; float bestY = -1f;
        for (int i = 0; i < c; i++)
        {
            Vector2 n = _contacts[i].normal;
            if (n.y > bestY) { bestY = n.y; bestNormal = n; }
        }

        Vector2 tangent = new Vector2(bestNormal.y, -bestNormal.x).normalized;
        if (sign < 0) tangent = -tangent;

        if (tangent.sqrMagnitude < 0.0001f)
            tangent = new Vector2(sign, 0f);

        return tangent;
    }
}
