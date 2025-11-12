using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAttack_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerAttack_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 0.82f;   // 1타 총 길이
    public const int multiHitCount = 1; // 동시타격 가능한 적의 수
    private const float comboAvailableTime = 0.62f; //콤보나 패링등으로 전환이 가능한 시간
    private float _elapsedTime;
    private InputAction attackAction;
    bool attackComboPressed;
    private InputAction parryAction;
    bool parryPressed;
    bool flag1;
    private InputAction moveAction;
    Vector2 moveActionValue;
    public void Enter()
    {
        if (attackAction == null)
            attackAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack");
        if (parryAction == null)
            parryAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Parry");
        if (moveAction == null)
            moveAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Move");
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
        if (!parryPressed)
        {
            if (_elapsedTime > comboAvailableTime - 0.24f || _elapsedTime < 0.05f)
                parryPressed = parryAction.IsPressed();
        }
        if (_elapsedTime < 0.05f)
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
        }
        if(_elapsedTime > comboAvailableTime + 0.35f * (duration - comboAvailableTime))
        {
            if (ctx.isDash)
            {
                fsm.ChangeState(ctx.dash);
            }
        }
        if(_elapsedTime > comboAvailableTime + 0.6f * (duration - comboAvailableTime))
        {
            if (attackComboPressed)
            {
                fsm.ChangeState(ctx.attackCombo);
            }
        }
        ///////////////////////////////////////////////////////////
        if (_elapsedTime > duration)
        {
            fsm.ChangeState(ctx.idle);
        }
        //
        moveActionValue = moveAction.ReadValue<Vector2>();
        moveActionValue.y = 0f;
        if (_elapsedTime < 0.07f)
        {
            if (moveActionValue.x > 0 && ctx.childTR.right.x < 0)
                ctx.childTR.localRotation = Quaternion.Euler(0f, 0f, 0f);
            else if (moveActionValue.x < 0 && ctx.childTR.right.x > 0)
                ctx.childTR.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }
    public void UpdatePhysics()
    {
        
    }
    void PlayerAttackComboInput(InputAction.CallbackContext callback)
    {
        if (_elapsedTime < comboAvailableTime - 0.24f) return;
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
            Vector2 hitPoint = 0.7f * coll.ClosestPoint(ctx.transform.position) + 0.3f * (Vector2)coll.transform.position + Vector2.up;
            GameManager.I.onHit.Invoke
            (
                new HitData
                (
                    "Attack",
                    ctx.transform,
                    coll.transform,
                    Random.Range(0.9f, 1.1f) * 33f,
                    hitPoint
                )
            );
        }
    }
}
