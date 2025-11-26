using UnityEngine;

public class PlayerJumpAttack : IPlayerState
{
    private readonly PlayerController ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerJumpAttack(PlayerController ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 0.5f;   // 총 길이
    private float _elapsedTime;
    public void Enter()
    {
        _elapsedTime = 0f;
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
