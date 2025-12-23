using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAttack : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerAttack(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private float _elapsedTime;
    private InputAction attackAction;
    bool attackComboPressed;
    private InputAction parryAction;
    bool parryPressed;
    bool flag1;
    private InputAction moveAction;
    Vector2 moveActionValue;
    float adjustedTime1;
    float adjustedTime2;
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
        switch(DBManager.I.currData.difficulty)
        {
            case 0:
            adjustedTime1 = duration - 0.2f;
            adjustedTime2 = comboAvailableTime - 0.15f;
            break;
            case 1:
            adjustedTime1 = duration;
            adjustedTime2 = comboAvailableTime;
            break;
            case 2:
            adjustedTime1 = duration + 0.2f;
            adjustedTime2 = comboAvailableTime + 0.1f;
            break;
        }
        attacked.Clear();
        attackComboPressed = false;
        flag1 = false;
        parryPressed = false;
        isSFX = false;
    }
    public void Exit()
    {
        attackAction.performed -= PlayerAttackComboInput;
        ctx.attackRange.onTriggetStay2D -= TriggerHandler;
    }
    bool isSFX;
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if (!parryPressed)
        {
            if (_elapsedTime > comboAvailableTime - 0.24f || _elapsedTime < 0.08f)
            {
                parryPressed = parryAction.IsPressed();
            }
        }
        if (_elapsedTime > 0.25f)
        {
            if(!isSFX)
            {
                isSFX = true;
                if(Random.value <= 0.7f)
                    AudioManager.I.PlaySFX("Swoosh1");
                else
                    AudioManager.I.PlaySFX("Swoosh2");
            }
        }
        if (_elapsedTime < 0.05f)
        {
            if (parryPressed)
                fsm.ChangeState(ctx.parry);
            if (!ctx.Grounded)
                fsm.ChangeState(ctx.idle);
        }
        if (!flag1)
        {
            flag1 = true;
            ctx.animator.Play("Player_Attack");
        }
        ///////////////////////////////////////////////////////////
        if (_elapsedTime > comboAvailableTime)
        {
            if (!parryPressed) parryPressed = parryAction.IsPressed();
            if (parryPressed)
            {
                fsm.ChangeState(ctx.parry);
            }
        }
        if(_elapsedTime > comboAvailableTime + 0.3f * (duration - comboAvailableTime))
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
    private const float duration = 0.75f;
    public const int multiHitCount = 1;
    private const float comboAvailableTime = 0.5f;
    public void UpdatePhysics()
    {
        
    }
    void PlayerAttackComboInput(InputAction.CallbackContext callback)
    {
        if (_elapsedTime < comboAvailableTime - 0.24f) return;
        if (!ctx.Grounded) return;
        attackComboPressed = true;
    }
    private const float duration = 0.78f;
    public const int multiHitCount = 1;
    private const float comboAvailableTime = 0.59f;
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
                    Random.Range(1f, 1.05f) * 35f,
                    hitPoint,
                    new string[1]{"Hit3"}
                )
            );
        }
    }
}
