using UnityEngine;
/*
public class PlayerSlam_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    private bool _impacted;
    private float _t; // 임팩트 락(잠깐 경직) 타이머

    public PlayerSlam_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    { this.ctx = ctx; this.fsm = fsm; }

    public void Enter()
    {
        _impacted = false;
        _t = 0f;

        ctx.isSlamming = true;
        ctx.SlamSwingBegin(); // 캐시 초기화

        // 애니(있다면)
        ctx.animator?.ResetTrigger("SlamStart");
        ctx.animator?.SetTrigger("SlamStart");
        ctx.StartCoroutine(ctx.ResetTriggerNextFrame("SlamStart"));

        // 시작 즉시 강하 속도 부여
        var vx = ctx.lockHorizontalOnSlam ? 0f : ctx.rb.linearVelocity.x;
        ctx.rb.linearVelocity = new Vector2(vx, -Mathf.Abs(ctx.slamFallSpeed));
    }

    public void Exit()
    {
        ctx.isSlamming = false;
        // 트리거 정리
        ctx.animator?.ResetTrigger("SlamStart");
        ctx.animator?.ResetTrigger("SlamImpact");
    }

    public void PlayerKeyInput() { } // 입력 무시

    public void UpdateState()
    {
        if (!_impacted)
        {
            // 착지 감지: 지면에 닿았다면 임팩트 처리
            if (ctx.Grounded)
            {
                DoImpact();
            }
        }
        else
        {
            _t += Time.deltaTime;
            if (_t >= ctx.slamImpactLock)
            {
                // 임팩트 락 종료 → 이동 상태 복귀
                if (ctx.Grounded)
                    fsm.ChangeState(Mathf.Abs(ctx.XInput) > 0.01f ? ctx.run : ctx.idle);
                else
                    fsm.ChangeState(ctx.fall);
            }
        }
    }

    public void UpdatePhysics()
    {
        if (_impacted) return;

        // 공중 내려찍기 중에는 계속 강하 유지
        float vx = ctx.lockHorizontalOnSlam ? 0f : ctx.rb.linearVelocity.x;
        float vy = Mathf.MoveTowards(ctx.rb.linearVelocity.y, -ctx.slamFallSpeed, ctx.slamAccel * Time.fixedDeltaTime);
        ctx.rb.linearVelocity = new Vector2(vx, vy);
    }

    private void DoImpact()
    {
        _impacted = true;
        _t = 0f;

        // 임팩트 애니(있다면)
        ctx.animator?.ResetTrigger("SlamImpact");
        ctx.animator?.SetTrigger("SlamImpact");
        ctx.StartCoroutine(ctx.ResetTriggerNextFrame("SlamImpact"));

        // 데미지 & 넉백 & 튕김 처리
        ctx.DoSlamDamage();

        if (ctx.slamBounceUpForce > 0f)
        {
            ctx.rb.linearVelocity = new Vector2(ctx.rb.linearVelocity.x, ctx.slamBounceUpForce);
        }
    }
}
*/