using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAttack_LSH : IPlayerState_LSH
{
    public float duration = 0.7f;
    public float nextInputDuration = 0.32f;
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerAttack_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    float elapsedTime;
    bool inputAttackCombo;
    bool inputDash;
    public void Enter()
    {
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed += AttackInput;
        ctx.attackBox.onTriggetStay2D += OnTriggerStay2D;
        ctx.animator.Play("Player_Attack");
        ctx.state = PlayerController_LSH.State.Attack;
        elapsedTime = 0f;
        inputAttackCombo = false;
        inputDash = false;
    }
    public void Update()
    {

    }
    public void FixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime > duration)
        {
            if (inputAttackCombo)
            {
                fsm.ChangeState(ctx.attackCombo);
            }
            else if (inputDash)
            {
                fsm.ChangeState(ctx.dash);
            }
            else
            {
                fsm.ChangeState(ctx.idle);
            }

        }
    }
    public void Exit()
    {
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed -= AttackInput;
        ctx.attackBox.onTriggetStay2D -= OnTriggerStay2D;
    }
    void AttackInput(InputAction.CallbackContext callback)
    {
        if (elapsedTime < duration - nextInputDuration) return;
        if (ctx.isGround)
        {
            inputAttackCombo = true;
        }
    }
    void DashInput(InputAction.CallbackContext callback)
    {
        if (elapsedTime < duration - nextInputDuration) return;
        if (ctx.isGround)
        {
            inputDash = true;
        }
    }
    void OnTriggerStay2D(Collider2D coll)
    {
        Debug.Log(coll);
    }
    

}
