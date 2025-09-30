using UnityEngine;

public class PlayerParry_LSH : IPlayerState_LSH
{
    private PlayerController_LSH ctx;
    private PlayerStateMachine_LSH fsm;

    private float _enterTime;
    private bool _recovering; // 실패 경직 중인가

    public PlayerParry_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    { this.ctx = ctx; this.fsm = fsm; }

    public void Enter()
    {
        _enterTime = Time.time;
        _recovering = false;

        // 방어 창 고정 시간 오픈
        ctx.parryActive = true;
        ctx.parrySuccess = false;
        ctx.parryEndTime = _enterTime + ctx.parryGuardTime;

        // 연출(선택)
        ctx.animator?.ResetTrigger("ParryStart");
        ctx.animator?.SetTrigger("ParryStart");
        ctx.StartCoroutine(ctx.ResetTriggerNextFrame("ParryStart"));
    }

    public void Exit()
    {
        // 창 종료 & 쿨다운 마크는 컨트롤러에서 관리 (Update/Exit 중 택1)
        ctx.parryActive = false;
        ctx.SetParryCooldown();
    }

    public void PlayerKeyInput() { }

    public void UpdateState()
    {
        // 방어창이 끝났고, 성공도 못 했으면 실패 경직 시작
        if (!_recovering && Time.time >= ctx.parryEndTime)
        {
            if (!ctx.parrySuccess)
            {
                _recovering = true;

                ctx.animator?.ResetTrigger("ParryFail");
                ctx.animator?.SetTrigger("ParryFail");
                ctx.StartCoroutine(ctx.ResetTriggerNextFrame("ParryFail"));

                ctx.parryActive = false; // 창 닫기
            }
            else
            {
                ReturnToMove();
                return;
            }
        }

        // 실패 경직 종료 시 복귀
        if (_recovering && Time.time >= ctx.parryEndTime + ctx.parryRecover)
        {
            ReturnToMove();
        }
    }

    public void UpdatePhysics()
    {
        // 패링 중 이동 억제 (원하면 값 조정)
        ctx.rb.linearVelocity = new Vector2(ctx.rb.linearVelocity.x * 0.2f, ctx.rb.linearVelocity.y);
    }

    private void ReturnToMove()
    {
        if (ctx.Grounded)
            fsm.ChangeState(Mathf.Abs(ctx.XInput) > 0.01f ? ctx.run : ctx.idle);
        else
            fsm.ChangeState(ctx.fall);
    }
}
