using UnityEngine;
public class PlayerParry_LSH : IPlayerState_LSH
{
    public float duration = 0.4f;
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerParry_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    float elapsedTime;
    public void Enter()
    {
        ctx.state = PlayerController_LSH.State.Parry;
        elapsedTime = 0f;
    }
    public void Update()
    {

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
