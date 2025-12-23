using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
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
    private InputAction parryAction;
    bool parryPressed;
    private InputAction escAction;
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
        if (escAction == null)
            escAction = ctx.inputActionAsset.FindActionMap("UI").FindAction("ESC");
        if (parryAction == null)
            parryAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Parry");
        ctx.animator.Play("Player_Idle");
        flagInt = 0;
        escAction.performed += InputESC;
        escAction.canceled += CancelESC;
        isESC = false;
        tween?.Kill();
    }
    public void Exit()
    {
        escAction.performed -= InputESC;
        escAction.canceled -= CancelESC;
        isESC = false;
        tween?.Kill();
    }
    Tween tween;
    public void UpdateState()
    {
        moveActionValue = moveAction.ReadValue<Vector2>();
        if (moveActionValue.x != 0)
            fsm.ChangeState(ctx.run);

        jumpPressed = jumpAction.IsPressed();
        if (jumpPressed && !ctx.Jumped && ctx.Grounded)
        {
            if (moveActionValue.x == 0 && moveActionValue.y < 0)
            {
                ctx.fallThroughPlatform = true;
                tween = DOVirtual.DelayedCall(0.1f, () => {ctx.fallThroughPlatform = false;}).Play();
                ctx.rb.AddForce(Vector2.down);
            }
            else if(!ctx.fallThroughPlatform)
            {
                ctx.Jumped = true;
                fsm.ChangeState(ctx.jump);
            }
        }
        
        attackPressed = attackAction.IsPressed();
        if (attackPressed && ctx.Grounded)
        {
            if(Time.time - ctx.attack.finishTime < 0.24f)
            {
                Debug.Log(Time.time - ctx.attack.finishTime);
                fsm.ChangeState(ctx.attackCombo);
            }
            else
            {
                fsm.ChangeState(ctx.attack);
            }
        }

        if (!ctx.Grounded && ctx.rb.linearVelocity.y < -0.1f)
            fsm.ChangeState(ctx.fall);

        potionPressed = potionAction.IsPressed();
        if (potionPressed && ctx.Grounded && (ctx.currHealth / ctx.maxHealth) < 1f)
        {
            if (DBManager.I.currData.currPotionCount > 0
            || (DBManager.I.currData.currPotionCount <= 0 && Time.time - ctx.usePotion.emptyTime > 0.2f))
            {
                ctx.usePotion.prevState = ctx.idle;
                fsm.ChangeState(ctx.usePotion);
            }
        }

        inventoryPressed = inventoryAction.IsPressed();
        if (!inventoryPressed && flagInt == 0)
        {
            flagInt = 1;
        }
        else if (inventoryPressed && flagInt == 1 && ctx.Grounded)
        {
            fsm.ChangeState(ctx.openInventory);
        }

        parryPressed = parryAction.IsPressed();
        if (parryPressed)
        {
            if (ctx.Grounded)
            {
                fsm.ChangeState(ctx.parry);
            }
        }

    }
    public void UpdatePhysics()
    {

    }
    bool isESC;
    PopupUI PopupUI;
    void InputESC(InputAction.CallbackContext callbackContext)
    {
        if (!isESC && !GameManager.I.isOpenDialog && !GameManager.I.isOpenPop && !GameManager.I.isOpenInventory)
        {
            if (PopupUI == null)
                PopupUI = GameManager.I.transform.GetComponent<PopupUI>();
            fsm.ChangeState(ctx.stop);
            GameManager.I.isOpenPop = true;
            PopupUI.OpenPop(1);
        }
        isESC = true;
    }
    void CancelESC(InputAction.CallbackContext callbackContext)
    {
        isESC = false;
    }



}
