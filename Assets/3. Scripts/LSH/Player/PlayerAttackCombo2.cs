using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAttackCombo2 : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerAttackCombo2(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private float _elapsedTime;
    private InputAction parryAction;
    bool parryPressed;
    bool isSFX;
    private const float duration = 0.66f;
    public const int multiHitCount = 1;
    private const float comboAvailableTime = 0.5f;
    float adjustedTime1;
    float adjustedTime2;
    public void Enter()
    {
        if (parryAction == null)
            parryAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Parry");
        ctx.attackRange.onTriggetStay2D += TriggerHandler;
        _elapsedTime = 0f;
        parryPressed = false;
        attacked.Clear();
        switch (DBManager.I.currData.difficulty)
        {
            case 0:
                adjustedTime1 = duration;
                adjustedTime2 = comboAvailableTime;
                break;
            case 1:
                adjustedTime1 = duration + 0.08f;
                adjustedTime2 = comboAvailableTime + 0.08f;
                break;
            case 2:
                adjustedTime1 = duration + 0.11f;
                adjustedTime2 = comboAvailableTime + 0.11f;
                break;
        }
        ctx.animator.Play("Player_Attack2");
        isSFX = false;
        ctx.attack.finishTime = 0;
    }
    public void Exit()
    {
        ctx.attackRange.onTriggetStay2D -= TriggerHandler;
        attacked.Clear();
        ctx.attack.finishTime = 0;
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
        if (_elapsedTime > 0.03f)
        {
            if (!isSFX)
            {
                isSFX = true;
                if (Random.value <= 0.7f)
                    AudioManager.I.PlaySFX("Swoosh3");
                else
                    AudioManager.I.PlaySFX("Swoosh2");
            }
        }
        ///////////////////////////////////////////////////////////
        if (_elapsedTime > adjustedTime2)
        {
            if (parryPressed)
            {
                fsm.ChangeState(ctx.parry);
            }
        }
        if (_elapsedTime > adjustedTime2 + (adjustedTime1 - adjustedTime2) * 0.3f)
        {
            if (ctx.isDash)
            {
                fsm.ChangeState(ctx.dash);
            }
        }
        ///////////////////////////////////////////////////////////
        if (_elapsedTime > adjustedTime1)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }
    List<Collider2D> attacked = new List<Collider2D>();
    void TriggerHandler(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Monster") && coll.gameObject.layer != LayerMask.NameToLayer("Interactable")) return;
        if (attacked.Count > 0)
        {
            int mCount = attacked.Count(x => x.gameObject.layer == LayerMask.NameToLayer("Monster"));
            if (mCount >= multiHitCount) return;
        }
        if (!attacked.Contains(coll))
        {
            attacked.Add(coll);
            Vector2 hitPoint = 0.7f * coll.ClosestPoint(ctx.transform.position) + 0.3f * (Vector2)coll.transform.position + Vector2.up;
            float rnd = Random.Range(0.78f, 1.38f);
            float damage = 36.8f;
            if (rnd >= 1.22f)
            {
                rnd = Random.Range(0.8f, 0.999f);
                damage = 45f;
            }
            float lanternOn = 1f;
            if (GameManager.I.isLanternOn) lanternOn = 1.33f;
            GameManager.I.onHit.Invoke
            (
                new HitData
                (
                    "AttackCombo",
                    ctx.transform,
                    coll.transform,
                    rnd * damage * lanternOn,
                    hitPoint,
                    new string[1] { "Hit3" }
                )
            );
        }
    }
}
