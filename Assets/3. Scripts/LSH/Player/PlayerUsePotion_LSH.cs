using UnityEngine;

public class PlayerUsePotion : IPlayerState
{
    private readonly PlayerController ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerUsePotion(PlayerController ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 2.9f;   // 총 길이
    private float _elapsedTime;
    public IPlayerState prevState;
    [HideInInspector] public float emptyTime;
    public void Enter()
    {
        if (DBManager.I.currentCharData.potionCount <= 0)
        {
            emptyTime = Time.time;
            AudioManager.I.PlaySFX("Fail1");
            ParticleManager.I.PlayText("Empty Potion", ctx.transform.position + Vector3.up, ParticleManager.TextType.PlayerNotice);
            //Debug.Log("포션이 부족합니다.");
            if (prevState == ctx.run)
                fsm.ChangeState(ctx.run);
            else
                fsm.ChangeState(ctx.idle);
            return;
        }
        _elapsedTime = 0f;
        ctx.animator.Play("Player_UsePotion");
        sfxFlag1 = false;
        sfxFlag2 = false;
    }
    public void Exit()
    {
        if (sfx != null)
            if (sfx.aus != null)
            {
                sfx?.aus?.Stop();
                sfx = null;
            }

    }
    SFX sfx;
    bool sfxFlag1 = false;
    bool sfxFlag2 = false;
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > 0.02f && !sfxFlag1)
        {
            sfxFlag1 = true;
            AudioManager.I.PlaySFX("Pocket1");
        }
        if (_elapsedTime > 0.92f)
        {
            if (!sfxFlag2)
            {
                sfxFlag2 = true;
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
