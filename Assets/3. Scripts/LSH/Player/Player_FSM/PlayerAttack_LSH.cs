using UnityEngine;

public class PlayerAttack_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    public PlayerAttack_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    { this.ctx = ctx; this.fsm = fsm; }

    // 타이밍(애니메이션에 맞춰 조절)
    private float _t;
    private const float AttackTotal = 0.35f;   // 1타 전체 지속
    private const float LockTime    = 0.20f;   // 초반 이동 감쇠 구간

    // 콤보 윈도우 & 버퍼
    private const float ComboOpen   = 0.12f;   // 이 시점 이후부터 콤보 허용
    private const float ComboClose  = 0.28f;   // 이 시점 지나면 콤보 불가
    private const float PreBuffer   = 0.08f;   // 오픈 전에 누른 입력 허용 범위
    private bool _comboQueued;

    // 이동 감쇠
    private const float GroundDampenEarly = 0.6f;
    private const float AirDampenEarly    = 0.4f;

    public void Enter()
    {
        _t = 0f;
        _comboQueued = false;
        ctx.TriggerAttack(); // 애니메이터 트리거(이벤트로 히트박스 on/off)
    }

    public void Exit() { }

    public void PlayerKeyInput()
    {
        if (!ctx.AttackPressed) return;

        // 콤보 오픈 전: 약간의 버퍼 허용
        if (_t < ComboOpen && _t >= ComboOpen - PreBuffer)
        {
            _comboQueued = true;
            return;
        }

        // 콤보 오픈~클로즈 구간: 즉시 큐
        if (_t >= ComboOpen && _t <= ComboClose)
        {
            _comboQueued = true;
        }
    }

    public void UpdateState()
    {
        _t += Time.deltaTime;

        // 콤보 전이
        if (_comboQueued && _t >= ComboOpen && _t <= ComboClose)
        {
            fsm.ChangeState(ctx.attackCombo);
            return;
        }

        // 종료 복귀
        if (_t >= AttackTotal)
        {
            if (ctx.Grounded)
            {
                if (Mathf.Abs(ctx.XInput) > 0.01f) fsm.ChangeState(ctx.run);
                else                                fsm.ChangeState(ctx.idle);
            }
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
