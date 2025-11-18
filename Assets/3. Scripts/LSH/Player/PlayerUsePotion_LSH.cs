using UnityEngine;

public class PlayerUsePotion_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerUsePotion_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 2.6f;   // 총 길이
    private float _elapsedTime;
    public void Enter()
    {
        if (DBManager.I.currentCharData.potionCount <= 0)
        {
            Debug.Log("포션이 부족합니다.");
            AudioManager.I.PlaySFX("Fail1");
            fsm.ChangeState(ctx.idle);
            return;
        }
        _elapsedTime = 0f;
        //Debug.Log("Use Potion");
        ctx.animator.Play("Player_UsePotion");
        sfxFlag = false;
    }
    public void Exit()
    {
        sfx?.aus.Stop();
    }
    SFX sfx;
    bool sfxFlag = false;
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > 0.82f)
        {
            if (!sfxFlag)
            {
                sfxFlag = true;
                DBManager.I.currentCharData.potionCount--;
                sfx = AudioManager.I.PlaySFX("Drink");
            }
            ctx.currentHealth += (1f / (duration - 0.82f)) * ctx.maxHealth * Time.deltaTime;
            ctx.currentHealth = Mathf.Clamp(ctx.currentHealth, 0f, ctx.maxHealth);
            DBManager.I.currentCharData.HP = ctx.currentHealth;
        }
        if (_elapsedTime > duration)
        {
            //Debug.Log("Use Potion End");
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }
}
