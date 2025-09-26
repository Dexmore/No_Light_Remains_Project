using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAttack_LSH : IPlayerState_LSH
{
    public float duration = 0.7f;
    public int multiHitCount = 1;
    public float nextInputDuration = 0.32f;
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerAttack_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    float elapsedTime;
    bool inputAttackCombo;
    bool inputDash;
    public void Enter()
    {
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed += Input_Attack;
        ctx.attackRange.onTriggetStay2D += Handler_TriggerStay2D;
        ctx.state = PlayerController_LSH.State.Attack;
        elapsedTime = 0f;
        inputAttackCombo = false;
        inputDash = false;
        isAnimation = false;
        attackedColliders.Clear();
    }
    public void Update()
    {

    }
    bool isAnimation;
    public void FixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime < 0.05f)
        {
            if (ctx.isParryInput)
            {
                Debug.Log("Parry");
                fsm.ChangeState(ctx.parry);
            }
        }
        else if(!isAnimation)
        {
            ctx.animator.Play("Player_Attack");
            isAnimation = true;
        }
        if (elapsedTime > duration)
        {
            if (inputAttackCombo)
            {
                fsm.ChangeState(ctx.attackCombo);
            }
            else
            {
                fsm.ChangeState(ctx.idle);
            }
        }
    }
    public void Exit()
    {
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed -= Input_Attack;
        ctx.attackRange.onTriggetStay2D -= Handler_TriggerStay2D;
    }
    void Input_Attack(InputAction.CallbackContext callback)
    {
        if (elapsedTime < duration - nextInputDuration) return;
        if (ctx.isGround)
        {
            inputAttackCombo = true;
        }
    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    void Handler_TriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Monster")) return;
        if (attackedColliders.Count >= multiHitCount) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            EventManager.I.onAttack(new EventManager.AttackData(ctx.transform, coll.transform, Random.Range(0.9f, 1.1f) * 100f));
        }
    }


}
