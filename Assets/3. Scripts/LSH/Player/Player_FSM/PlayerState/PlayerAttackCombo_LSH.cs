using UnityEngine;

public class PlayerAttackCombo_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    public PlayerAttackCombo_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    { this.ctx = ctx; this.fsm = fsm; }

    private float _t;
    private const float AttackTotal = 0.40f; // 2타 총 길이
    private const float LockTime = 0.22f;

    private const float GroundDampenEarly = 0.55f;
    private const float AirDampenEarly = 0.38f;

    // ★ 2타 히트 타이밍
    private const float HitTime2 = 0.18f;

    private bool _didHit;

    public void Enter()
    {
        _t = 0f;
        _didHit = false;
        ctx.TriggerAttack2();
    }

    public void Exit()
    {
        _didHit = false;
        ctx.animator?.ResetTrigger("Attack");
        ctx.animator?.ResetTrigger("Attack2");
    }

    public void PlayerKeyInput() { }

    public void UpdateState()
    {
        _t += Time.deltaTime;

        // ★ 타이머로 히트 1회
        if (!_didHit && _t >= HitTime2)
        {
            ctx.AttackSwingBegin();
            ctx.DoDamage_Public(2);
            _didHit = true;
        }

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
