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
            duration = 0.3f;
            //Debug.Log("HitSmall");
        }
        else if (staggerType == HitData.StaggerType.Middle)
        {
            duration = 0.7f * 2f;
            //Debug.Log("HitMiddle");
        }
        else if (staggerType == HitData.StaggerType.Large)
        {
            duration = 1.8f * 2f;
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
