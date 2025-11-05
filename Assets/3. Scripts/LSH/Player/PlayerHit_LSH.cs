using UnityEngine;

public class PlayerHit_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerHit_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    [HideInInspector] public HitData.StaggerType staggerType;
    float duration = 0.1f;
    private float _elapsedTime;
    public void Enter()
    {
        _elapsedTime = 0f;
        duration = 0.1f;
        if (staggerType == HitData.StaggerType.Small)
        {
            duration = 0.4f;
            ctx.animator.Play("Idle");
            //Debug.Log("HitSmall");
        }
        else if (staggerType == HitData.StaggerType.Middle)
        {
            duration = 1.2f;
            ctx.animator.Play("Idle");
            //Debug.Log("HitMiddle");
        }
        else if (staggerType == HitData.StaggerType.Large)
        {
            duration = 3.5f;
            ctx.animator.Play("Idle");
            //Debug.Log("HitLarge");
        }
    }
    public void Exit()
    {
        
    }
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if(_elapsedTime > duration)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }
}
