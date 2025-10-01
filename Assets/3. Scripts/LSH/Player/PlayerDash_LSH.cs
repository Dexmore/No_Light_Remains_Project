using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerDash_LSH : IPlayerState_LSH
{
    public float duration = 0.4f;
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerDash_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    InputAction inputAction_Move;
    Vector2 moveDirection;
    float elapsedTime;
    public void Enter()
    {
        inputAction_Move = ctx.inputActionAsset.FindActionMap("Player").FindAction("Move");
        ctx.animator.Play("Player_Dash");
        ctx.state = PlayerController_LSH.State.Dash;
        elapsedTime = 0f;
    }
    public void Update()
    {
        if (inputAction_Move == null) return;
        moveDirection = inputAction_Move.ReadValue<Vector2>();
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
        
    }


}
