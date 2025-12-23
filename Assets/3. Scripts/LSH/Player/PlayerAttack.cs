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
    private const float duration = 0.77f;
    public const int multiHitCount = 1;
    private const float comboAvailableTime = 0.53f;
    float adjustedTime1;
    float adjustedTime2;
    public float finishTime;
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
            adjustedTime1 = duration;
            adjustedTime2 = comboAvailableTime;
            break;
            case 1:
            adjustedTime1 = duration + 0.09f;
            adjustedTime2 = comboAvailableTime + 0.11f;
            break;
            case 2:
            adjustedTime1 = duration + 0.16f;
            adjustedTime2 = comboAvailableTime + 0.18f;
            break;
        }
        attacked.Clear();
        attackComboPressed = false;
        flag1 = false;
        parryPressed = false;
        isSFX = false;
        finishTime = 0f;
    }
    public void Exit()
    {
        attackAction.performed -= PlayerAttackComboInput;
        ctx.attackRange.onTriggetStay2D -= TriggerHandler;
        finishTime = Time.time;
    }
    bool isSFX;
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if (!parryPressed)
        {
            if (_elapsedTime > adjustedTime2 - 0.24f || _elapsedTime < 0.1f)
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
        if (!parryPressed) parryPressed = parryAction.IsPressed();
        if (_elapsedTime > adjustedTime2)
        {
            if (parryPressed)
            {
                fsm.ChangeState(ctx.parry);
            }
        }
        if(_elapsedTime > adjustedTime2 + 0.3f * (adjustedTime1 - adjustedTime2))
        {
            if (ctx.isDash)
            {
                fsm.ChangeState(ctx.dash);
            }
        }
        if(_elapsedTime > adjustedTime2 + 0.6f * (adjustedTime1 - adjustedTime2))
        {
            if (attackComboPressed)
            {
                fsm.ChangeState(ctx.attackCombo);
            }
        }
        ///////////////////////////////////////////////////////////
        if (_elapsedTime > adjustedTime1)
        {
            finishTime = Time.time;
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
        if (_elapsedTime < adjustedTime2 - 0.24f) return;
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
            float lanternOn = 1f;
            if(GameManager.I.isLanternOn) lanternOn = 1.2f;
            attacked.Add(coll);
            Vector2 hitPoint = 0.7f * coll.ClosestPoint(ctx.transform.position) + 0.3f * (Vector2)coll.transform.position + Vector2.up;
            GameManager.I.onHit.Invoke
            (
                new HitData
                (
                    "Attack",
                    ctx.transform,
                    coll.transform,
                    Random.Range(1f, 1.05f) * 34f * lanternOn,
                    hitPoint,
                    new string[1]{"Hit3"}
                )
            );
        }
    }
}
