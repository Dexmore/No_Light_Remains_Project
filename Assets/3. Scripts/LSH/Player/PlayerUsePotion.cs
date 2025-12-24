using UnityEngine;

public class PlayerUsePotion : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerUsePotion(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 1.37f;   // 총 길이
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
        sfx?.Despawn();
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
            float startHealth = ctx.currHealth;
            if (!sfxFlag2)
            {
                sfxFlag2 = true;
                // 회복 시작 시점의 체력을 저장 (정확한 Lerp를 위해 필요)
                DBManager.I.currData.currPotionCount--;
                sfx = AudioManager.I.PlaySFX("Drink");
                if (_mainCamera == null) _mainCamera = Camera.main;
                // 지역 변수가 아닌 클래스 멤버 변수 upa에 할당
                upa = ParticleManager.I.PlayUIParticle("UIAttPotion",
                    MethodCollection.WorldTo1920x1080Position(ctx.transform.position, _mainCamera),
                    Quaternion.identity);
                if (upa != null && upa.TryGetComponent(out AttractParticle ap))
                {
                    Vector3 pos = _mainCamera.ViewportToWorldPoint(new Vector3(0.21f, 0.895f, 0f));
                    ap.targetVector = pos;
                }
            }
            // --- 가속 회복 로직 시작 ---
            float t = (_elapsedTime - 1.2f) / (duration - 1.2f);
            t = Mathf.Clamp01(t);
            float acceleratedT = t * t * t * t * t;
            ctx.currHealth = Mathf.Lerp(startHealth, ctx.maxHealth, acceleratedT);
            ctx.currHealth = Mathf.Clamp(ctx.currHealth, 0f, ctx.maxHealth);
            DBManager.I.currData.currHealth = ctx.currHealth;
            // --- 가속 회복 로직 끝 ---
        }
        if (_elapsedTime > 1.12f && !sfxFlag3)
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
