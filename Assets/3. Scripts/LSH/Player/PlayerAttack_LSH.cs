using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAttack_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerAttack_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 0.88f;   // 1타 총 길이
    public const int multiHitCount = 1; // 동시타격 가능한 적의 수
    private const float comboAvailableTime = 0.65f; //콤보나 패링등으로 전환이 가능한 시간
    private float _elapsedTime;
    private InputAction attackAction;
    bool attackComboPressed;
    private InputAction parryAction;
    bool parryPressed;
    bool flag1;
    public void Enter()
    {
        if (attackAction == null)
            attackAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack");
        if (parryAction == null)
            parryAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Parry");
        attackAction.performed += PlayerAttackComboInput;
        ctx.attackRange.onTriggetStay2D += TriggerHandler;
        _elapsedTime = 0f;
        attacked.Clear();
        attackComboPressed = false;
        flag1 = false;
        parryPressed = false;
    }
    public void Exit()
    {
        attackAction.performed -= PlayerAttackComboInput;
        ctx.attackRange.onTriggetStay2D -= TriggerHandler;
    }
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if (!parryPressed) parryPressed = parryAction.IsPressed();
        if (_elapsedTime < 0.03f)
        {
            if (parryPressed)
                fsm.ChangeState(ctx.parry);

            if (!ctx.Grounded)
                fsm.ChangeState(ctx.idle);
        }
        else if (!flag1)
        {
            flag1 = true;
            ctx.animator.Play("Player_Attack");
        }
        ///////////////////////////////////////////////////////////
        if (_elapsedTime > comboAvailableTime)
        {
            if (parryPressed)
            {
                fsm.ChangeState(ctx.parry);
            }
            else if (attackComboPressed)
            {
                fsm.ChangeState(ctx.attackCombo);
            }
        }
        ///////////////////////////////////////////////////////////
        if (_elapsedTime > duration)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }
    void PlayerAttackComboInput(InputAction.CallbackContext callback)
    {
        if (_elapsedTime < comboAvailableTime - 0.25f) return;
        if (!ctx.Grounded) return;
        attackComboPressed = true;
    }
    List<Collider2D> attacked = new List<Collider2D>();
    void TriggerHandler(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Monster")) return;
        if (attacked.Count >= multiHitCount) return;
        if (!attacked.Contains(coll))
        {
            attacked.Add(coll);
            GameManager.I.onHit.Invoke(new HitData(ctx.transform, coll.transform, Random.Range(0.9f, 1.1f) * 90));
        }
    }
}
