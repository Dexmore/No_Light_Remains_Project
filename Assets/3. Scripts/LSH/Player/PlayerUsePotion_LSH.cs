using UnityEngine;

public class PlayerUsePotion_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerUsePotion_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 1.8f;   // 총 길이
    private float _elapsedTime;
    public void Enter()
    {
        _elapsedTime = 0f;
        Debug.Log("Use Potion");
    }
    public void Exit()
    {

    }
    public void UpdateState()
    {
        ctx.currentHealth += (1f / duration) * ctx.maxHealth * Time.deltaTime;
        ctx.currentHealth = Mathf.Clamp(ctx.currentHealth, 0f, ctx.maxHealth);
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > duration)
        {
            Debug.Log("Use Potion End");
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }
}
