using UnityEngine;

public class PlayerAttackCombo_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;

    public PlayerAttackCombo_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm)
    {
        this.ctx = ctx;
        this.fsm = fsm;
    }

    private float _t;
    private const float AttackTotal = 0.40f; // 2타 전체 지속(조금 길게)
    private const float LockTime    = 0.22f;

    private const float GroundDampenEarly = 0.55f;
    private const float AirDampenEarly    = 0.38f;

    public void Enter()
    {
        _t = 0f;
        ctx.TriggerAttack2(); // 애니메이터 트리거(2타 클립, 이벤트로 히트박스 on/off)
    }

    public void Exit() { }
    public void PlayerKeyInput() { /* 2타는 추가 콤보 없음(원하면 3타도 가능) */ }

    public void UpdateState()
    {
        _t += Time.deltaTime;

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
