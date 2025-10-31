using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAttackCombo_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerAttackCombo_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 1.3f;   // 1타 총 길이
    public const int multiHitCount = 2; // 동시타격 가능한 적의 수
    private const float comboAvailableTime = 0.8f; //콤보나 패링등으로 전환이 가능한 시간
    private float _elapsedTime;
    private InputAction parryAction;
    bool parryPressed;
    public void Enter()
    {
        if (parryAction == null)
            parryAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Parry");
        ctx.attackRange.onTriggetStay2D += TriggerHandler;
        _elapsedTime = 0f;
        parryPressed = false;
        attacked.Clear();
        ctx.animator.Play("Player_Attack2");
    }
    public void Exit()
    {
        ctx.attackRange.onTriggetStay2D -= TriggerHandler;
    }
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if(!parryPressed) parryPressed = parryAction.IsPressed();
        if (_elapsedTime < 0.03f)
        {
            if (parryPressed)
                fsm.ChangeState(ctx.parry);
            if (!ctx.Grounded)
                fsm.ChangeState(ctx.idle);
        }
        ///////////////////////////////////////////////////////////
        if (_elapsedTime > comboAvailableTime)
        {
            if (parryPressed)
            {
                fsm.ChangeState(ctx.parry);
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
    List<Collider2D> attacked = new List<Collider2D>();
    void TriggerHandler(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Monster")) return;
        if (attacked.Count >= multiHitCount) return;
        if (!attacked.Contains(coll))
        {
            attacked.Add(coll);
            GameManager.I.onHit.Invoke(new HitData(ctx.transform, coll.transform, Random.Range(0.9f, 1.1f) * 120));
            ParticleManager.I.PlayParticle("Hit2", coll.transform.position + Vector3.up, Quaternion.identity, null);
            AudioManager.I.PlaySFX("Hit8Bit", coll.transform.position + Vector3.up, null);
        }
    }
}
