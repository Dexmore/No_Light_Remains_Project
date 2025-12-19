using UnityEngine;

public class PlayerUsePotion : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerUsePotion(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 3.4f;   // 총 길이
    private float _elapsedTime;
    public IPlayerState prevState;
    [HideInInspector] public float emptyTime;
    public void Enter()
    {
        if (DBManager.I.currData.currPotionCount <= 0)
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
        ctx.hUDBinder.Refresh(6f);
        _elapsedTime = 0f;
        ctx.animator.Play("Player_Idle");
        sfxFlag1 = false;
        sfxFlag2 = false;
        sfxFlag3 = false;
        aniFlag1 = false;
    }
    public void Exit()
    {
        if (sfx != null)
            if (sfx.aus != null)
            {
                sfx?.aus?.Stop();
                sfx = null;
            }
        sfxFlag1 = false;
        sfxFlag2 = false;
        sfxFlag3 = false;
        aniFlag1 = false;
        upa?.Despawn();
    }
    SFX sfx;
    bool sfxFlag1 = false;
    bool sfxFlag2 = false;
    bool sfxFlag3 = false;
    bool aniFlag1 = false;
    UIParticle upa;
    Camera _mainCamera;
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > 0.08f && !sfxFlag1)
        {
            sfxFlag1 = true;
            AudioManager.I.PlaySFX("Pocket1");
        }
        if (_elapsedTime > 0.26f && !aniFlag1)
        {
            aniFlag1 = true;
            ctx.animator.Play("Player_UsePotion");
        }
        if (_elapsedTime > 1.2f)
        {
            if (!sfxFlag2)
            {
                sfxFlag2 = true;
                DBManager.I.currData.currPotionCount--;
                sfx = AudioManager.I.PlaySFX("Drink");
                if (_mainCamera == null) _mainCamera = Camera.main;
                UIParticle upa = ParticleManager.I.PlayUIParticle("UIAttPotion", MethodCollection.WorldTo1920x1080Position(ctx.transform.position, _mainCamera), Quaternion.identity);
                AttractParticle ap = upa.GetComponent<AttractParticle>();
                Vector3 pos = _mainCamera.ViewportToWorldPoint(new Vector3(0.21f, 0.895f, 0f));
                ap.targetVector = pos;
            }
            ctx.currHealth += (1f / (duration - 1.2f)) * ctx.maxHealth * Time.deltaTime;
            ctx.currHealth = Mathf.Clamp(ctx.currHealth, 0f, ctx.maxHealth);
            DBManager.I.currData.currHealth = ctx.currHealth;
        }
        if (_elapsedTime > 1.32f && !sfxFlag3)
        {
            sfxFlag3 = true;
            AudioManager.I.PlaySFX("Heal");
            ParticleManager.I.PlayParticle("PotionEffect", ctx.transform.position + 0.8f * Vector3.up, Quaternion.identity);
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
