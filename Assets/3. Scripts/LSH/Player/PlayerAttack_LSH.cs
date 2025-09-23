using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAttack_LSH : IPlayerState_LSH
{
    public float duration = 0.7f;
    public float inputAttackComboDuration = 0.32f;
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerAttack_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    float elapsedTime;
    bool inputAttackCombo;
    public void Enter()
    {
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed += AttackInput;
        ctx.animator.Play("Player_Attack");
        ctx.state = PlayerController_LSH.State.Attack;
        elapsedTime = 0f;
        inputAttackCombo = false;
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
            else
            {
                fsm.ChangeState(ctx.idle);
            }
            
        }
    }
    public void Exit()
    {
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed -= AttackInput;
    }
    void AttackInput(InputAction.CallbackContext callback)
    {
        if (elapsedTime < duration - inputAttackComboDuration) return;
        if (ctx.isGround)
        {
            inputAttackCombo = true;
        }
    }

}
