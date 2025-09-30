using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerHit_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerHit_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    float elapsedTime;
    public void Enter()
    {
        ctx.animator.Play("Player_Attack");
        ctx.state = PlayerController_LSH.State.Attack;
        elapsedTime = 0f;
    }
    public void Update()
    {

    }
    public void FixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime > 0.7f)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void Exit()
    {

    }
    
}
