using UnityEngine;

public class PlayerAttack_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    public PlayerAttack_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    { this.ctx = ctx; this.fsm = fsm; }

    private float _t;
    private const float AttackTotal = 0.35f;   // 1타 총 길이
    private const float LockTime = 0.20f;   // 초반 이동 감쇠

    // ★ 타이머 방식 히트 타이밍(초) — 클립에 맞춰 조정
    private const float HitTime = 0.16f;

    // 콤보 윈도우
    private const float ComboOpen = 0.12f;
    private const float ComboClose = 0.28f;
    private const float PreBuffer = 0.10f;
    private bool _comboQueued;

    private const float GroundDampenEarly = 0.6f;
    private const float AirDampenEarly = 0.4f;

    private bool _didHit;

    public void Enter()
    {
        _t = 0f;
        _didHit = false;
        _comboQueued = false;
        ctx.TriggerAttack();
    }

    public void Exit()
    {
        _comboQueued = false;
        _didHit = false;
        ctx.animator?.ResetTrigger("Attack");
        ctx.animator?.ResetTrigger("Attack2");
    }

    public void PlayerKeyInput()
    {
        if (!ctx.AttackPressed) return;

        if (_t < ComboOpen && _t >= ComboOpen - PreBuffer) { _comboQueued = true; return; }
        if (_t >= ComboOpen && _t <= ComboClose) _comboQueued = true;
    }

    public void UpdateState()
    {
        _t += Time.deltaTime;

        // ★ 타이머로 히트 1회
        if (!_didHit && _t >= HitTime)
        {
            ctx.AttackSwingBegin();
            ctx.DoDamage_Public(1);
            _didHit = true;
        }

        // 콤보 전이
        if (_comboQueued && _t >= ComboOpen && _t <= ComboClose)
        { fsm.ChangeState(ctx.attackCombo); return; }

        // 종료 복귀
        if (_t >= AttackTotal)
        {
            if (ctx.Grounded)
                fsm.ChangeState(Mathf.Abs(ctx.XInput) > 0.01f ? ctx.run : ctx.idle);
            else fsm.ChangeState(ctx.fall);
        }
    }

    public void UpdatePhysics()
    {
        bool early = _t < LockTime;
        float speed = ctx.Grounded
            ? ctx.moveSpeed * (early ? GroundDampenEarly : 1f)
            : ctx.moveSpeed * (early ? AirDampenEarly : ctx.airMoveMultiplier);

        ctx.rb.linearVelocity = new Vector2(ctx.XInput * speed, ctx.rb.linearVelocity.y);
        ctx.UpdateFacing(ctx.XInput);
    }
}
