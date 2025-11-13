using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerIdle_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerIdle_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    private InputAction moveAction;
    Vector2 moveActionValue;
    private InputAction jumpAction;
    bool jumpPressed;
    private InputAction attackAction;
    bool attackPressed;
    private InputAction potionAction;
    bool potionPressed;
    public void Enter()
    {
        if (moveAction == null)
            moveAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Move");
        if (jumpAction == null)
            jumpAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Jump");
        if (attackAction == null)
            attackAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack");
        if (potionAction == null)
            potionAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Potion");
        ctx.animator.Play("Player_Idle");
    }
    public void Exit()
    {
        
    }
    public void UpdateState()
    {
        moveActionValue = moveAction.ReadValue<Vector2>();
        if (moveActionValue.x != 0)
            fsm.ChangeState(ctx.run);
        
        jumpPressed = jumpAction.IsPressed();
        if (jumpPressed && ctx.Grounded)
            fsm.ChangeState(ctx.jump);

        attackPressed = attackAction.IsPressed();
        if (attackPressed && ctx.Grounded)
            fsm.ChangeState(ctx.attack);

        if (!ctx.Grounded && ctx.rb.linearVelocity.y < -0.1f)
            fsm.ChangeState(ctx.fall);

        potionPressed = potionAction.IsPressed();
        if (potionPressed && ctx.Grounded && (ctx.currentHealth/ctx.maxHealth) < 1f)
            fsm.ChangeState(ctx.usePotion);
        
    }
    public void UpdatePhysics()
    {

    }
}
