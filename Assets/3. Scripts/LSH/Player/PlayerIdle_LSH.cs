using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerIdle_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerIdle_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    InputAction inputAction_Move;
    Vector2 direction;
    public void Enter()
    {
        inputAction_Move = ctx.inputActionAsset.FindActionMap("Player").FindAction("Move");
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed += Input_Attack;
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
        if (inputAction_Move == null) return;
        direction = inputAction_Move.ReadValue<Vector2>();
        if (direction.x != 0)
            fsm.ChangeState(ctx.run);
    }
    public void FixedUpdate()
    {

    }
    public void Exit()
    {
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed -= Input_Attack;
    }
    void Input_Attack(InputAction.CallbackContext callback)
    {
        if (ctx.isGround)
        {
            fsm.ChangeState(ctx.attack);
        }
    }
    



}
