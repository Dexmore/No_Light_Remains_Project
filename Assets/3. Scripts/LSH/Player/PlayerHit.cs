using UnityEngine;

public class PlayerHit : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerHit(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    [HideInInspector] public HitData.StaggerType staggerType;
    float duration = 0.1f;
    private float _elapsedTime;
    public void Enter()
    {
        _elapsedTime = 0f;
        duration = 0.1f;
        if (staggerType == HitData.StaggerType.Small)
        {
            ctx.animator.Play("Player_HitSmall");
            duration = 0.4f;
        }
        else if (staggerType == HitData.StaggerType.Middle)
        {
            ctx.animator.Play("Player_HitMiddle");
            duration = 1.1f;
        }
        else if (staggerType == HitData.StaggerType.Large)
        {
            ctx.animator.Play("Player_HitLarge");
            duration = 2.9f;
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
