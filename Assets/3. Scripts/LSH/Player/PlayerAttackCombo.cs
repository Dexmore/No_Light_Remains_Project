using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAttackCombo : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerAttackCombo(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private float _elapsedTime;
    private InputAction parryAction;
    bool parryPressed;
    bool isSFX;
    public void Enter()
    {
        if (parryAction == null)
            parryAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Parry");
        ctx.attackRange.onTriggetStay2D += TriggerHandler;
        _elapsedTime = 0f;
        parryPressed = false;
        attacked.Clear();
        ctx.animator.Play("Player_Attack2");
        isSFX = false;
    }
    public void Exit()
    {
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
        if (_elapsedTime > 0.03f)
        {
            if(!isSFX)
            {
                isSFX = true;
                if(Random.value <= 0.7f)
                    AudioManager.I.PlaySFX("Swoosh3");
                else
                    AudioManager.I.PlaySFX("Swoosh2");
            }
        }
        ///////////////////////////////////////////////////////////
        if (_elapsedTime > comboAvailableTime)
        {
            if (parryPressed)
            {
                fsm.ChangeState(ctx.parry);
            }
        }
        if (_elapsedTime > comboAvailableTime + (duration - comboAvailableTime) * 0.3f)
        {
            if (ctx.isDash)
            {
                fsm.ChangeState(ctx.dash);
            }
        }
        ///////////////////////////////////////////////////////////
        if (_elapsedTime > duration)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    private const float duration = 0.55f;
    public const int multiHitCount = 1;
    private const float comboAvailableTime = 0.4f;
    public void UpdatePhysics()
    {

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
            float rnd = Random.Range(0.65f, 1.38f);
            float damage = 80.8f;
            if(rnd >= 1.2f)
            {
                rnd = Random.Range(0.8f, 1f);
                damage = 101f;
            }
            GameManager.I.onHit.Invoke
            (
                new HitData
                (
                    "AttackCombo",
                    ctx.transform,
                    coll.transform,
                    rnd * damage,
                    hitPoint,
                    new string[1]{"Hit3"}
                )
            );
        }
    }
}
