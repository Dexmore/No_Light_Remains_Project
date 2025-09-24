using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerIdle_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerIdle_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    InputAction moveInputAction;
    Vector2 moveDirection;
    public void Enter()
    {
        moveInputAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Move");
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed += AttackInput;
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Dash").performed += DashInput;
        ctx.state = PlayerController_LSH.State.Idle;
        if (ctx.isGround)
        {
            ctx.animator.Play("Player_Idle");
        }
        else
        {
            if (ctx.isJump)
            {
                if (!ctx.animator.GetCurrentAnimatorStateInfo(0).IsName("Player_Jump"))
                {
                    ctx.animator.Play("Player_Jump");
                }
            }
            else
            {
                if (!ctx.animator.GetCurrentAnimatorStateInfo(0).IsName("Player_Fall"))
                {
                    ctx.animator.Play("Player_Fall");
                }
            }
        }
    }
    public void Update()
    {
        if (moveInputAction == null) return;
        moveDirection = moveInputAction.ReadValue<Vector2>();
        if (moveDirection.x != 0)
            fsm.ChangeState(ctx.run);
    }
    public void FixedUpdate()
    {

    }
    public void Exit()
    {
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed -= AttackInput;
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Dash").performed -= DashInput;
    }
    void AttackInput(InputAction.CallbackContext callback)
    {
        if (ctx.isGround)
        {
            fsm.ChangeState(ctx.attack);
        }
    }
    void DashInput(InputAction.CallbackContext callback)
    {
        if (ctx.isGround)
        {
            fsm.ChangeState(ctx.dash);
        }
    }

}
