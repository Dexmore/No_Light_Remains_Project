using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAttackCombo_LSH : IPlayerState_LSH
{
    public float duration = 0.7f;
    public int multiHitCount = 1;
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerAttackCombo_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    float elapsedTime;
    public void Enter()
    {
        ctx.attackRange.onTriggetStay2D += OnTriggerStay2D_Child;
        ctx.animator.Play("Player_Attack2");
        ctx.state = PlayerController_LSH.State.AttackCombo;
        elapsedTime = 0f;
        attackedColliders.Clear();
    }
    public void Update()
    {

    }
    public void FixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime > duration)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void Exit()
    {
        ctx.attackRange.onTriggetStay2D -= OnTriggerStay2D_Child;
    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    void OnTriggerStay2D_Child(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Monster")) return;
        if (attackedColliders.Count >= multiHitCount) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            EventManager.I.onAttack(new EventManager.AttackData(ctx.transform, coll.transform, Random.Range(0.9f, 1.1f) * 11f));
        }
    }
    
    
}
