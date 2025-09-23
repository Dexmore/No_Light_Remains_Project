using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerAttackCombo_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerAttackCombo_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    float elapsedTime;
    public void Enter()
    {
        ctx.animator.Play("Player_Attack2");
        ctx.state = PlayerController_LSH.State.AttackCombo;
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
