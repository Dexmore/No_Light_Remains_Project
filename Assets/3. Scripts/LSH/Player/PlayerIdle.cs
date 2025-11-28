using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerIdle : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerIdle(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private InputAction moveAction;
    Vector2 moveActionValue;
    private InputAction jumpAction;
    bool jumpPressed;
    private InputAction attackAction;
    bool attackPressed;
    private InputAction potionAction;
    bool potionPressed;
    private InputAction inventoryAction;
    bool inventoryPressed;
    int flagInt = 0;
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
        if (inventoryAction == null)
            inventoryAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Inventory");
        ctx.animator.Play("Player_Idle");
        flagInt = 0;
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
        if (jumpPressed && !ctx.Jumped && ctx.Grounded)
        {
            ctx.Jumped = true;
            fsm.ChangeState(ctx.jump);
        }

        attackPressed = attackAction.IsPressed();
        if (attackPressed && ctx.Grounded)
            fsm.ChangeState(ctx.attack);

        if (!ctx.Grounded && ctx.rb.linearVelocity.y < -0.1f)
            fsm.ChangeState(ctx.fall);

        potionPressed = potionAction.IsPressed();
        if (potionPressed && ctx.Grounded && (ctx.currentHealth / ctx.maxHealth) < 1f)
        {
            if (DBManager.I.currentCharData.potionCount > 0
            || (DBManager.I.currentCharData.potionCount <= 0 && Time.time - ctx.usePotion.emptyTime > 0.2f))
            {
                ctx.usePotion.prevState = ctx.idle;
                fsm.ChangeState(ctx.usePotion);
            }
        }

        inventoryPressed = inventoryAction.IsPressed();
        if(!inventoryPressed && flagInt == 0)
        {
            flagInt = 1;
        }
        else if(inventoryPressed && flagInt == 1 && ctx.Grounded)
        {
            fsm.ChangeState(ctx.openInventory);
        }


    }
    public void UpdatePhysics()
    {

    }
}
