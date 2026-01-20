using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class PlayerUsePotion : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerUsePotion(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }


    // 포션 1회 사용으로 차게 할 체력 회복량 (ex. 아래값이 1일경우 최대체력의 100%, 아래값이 0.8일경우 최대체력의 80%, 0.5일경우 최대체력의 50%가 참)
    private const float healAmount = 0.6f;
    // 포션사용 동작 길이
    private const float duration = 1.7f;


    private float _elapsedTime;
    public IPlayerState prevState;
    [HideInInspector] public float emptyTime;
    private float adjustedTime;
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
        if (cts != null)
        {
            if (!cts.IsCancellationRequested && once)
            {
                if (prevState == ctx.run)
                    fsm.ChangeState(ctx.run);
                else
                    fsm.ChangeState(ctx.idle);
                return;
            }
        }
        once = false;
        cts?.Cancel();
        cts = new CancellationTokenSource();
        Heal(cts.Token).Forget();
        ctx.hUDBinder.Refresh(6f);
        _elapsedTime = 0f;
        ctx.animator.Play("Player_Idle");
        sfxFlag1 = false;
        sfxFlag2 = false;
        sfxFlag3 = false;
        aniFlag1 = false;
        switch (DBManager.I.currData.difficulty)
        {
            case 0:
                adjustedTime = duration;
                break;
            case 1:
                adjustedTime = duration * 1.1f + 0.1f;
                break;
            case 2:
                adjustedTime = duration * 1.2f + 0.2f;
                break;
        }
        //Gear 기어 (신복의 기어) 007_QuickHealGear
        bool outValue = false;
        if (DBManager.I.HasGear("007_QuickHealGear", out outValue))
        {
            if (outValue)
            {
                int level = DBManager.I.GetGearLevel("007_QuickHealGear");
                if (level == 0)
                {
                    adjustedTime = 0.9f * adjustedTime - 0.05f;
                }
                else if (level == 1)
                {
                    adjustedTime = 0.8f * adjustedTime - 0.1f;
                }
            }
        }
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
        sfx = null;
        upa?.Despawn();
        upa = null;
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
        if (_elapsedTime > 0.2f && !sfxFlag1)
        {
            sfxFlag1 = true;
            AudioManager.I.PlaySFX("Pocket1");
        }
        if (_elapsedTime > 0.15f && !aniFlag1)
        {
            aniFlag1 = true;
            ctx.animator.Play("Player_UsePotion");
        }
        if (_elapsedTime > duration * 0.3f)
        {
            float startHealth = ctx.currHealth;
            if (!sfxFlag2)
            {
                sfxFlag2 = true;
                DBManager.I.currData.currPotionCount--;
                sfx = AudioManager.I.PlaySFX("Drink");
                if (_mainCamera == null) _mainCamera = Camera.main;
                upa = ParticleManager.I.PlayUIParticle("UIAttPotion",
                    MethodCollection.WorldTo1920x1080Position(ctx.transform.position, _mainCamera),
                    Quaternion.identity);
                if (upa != null && upa.TryGetComponent(out AttractParticle ap))
                {
                    Vector3 pos = _mainCamera.ViewportToWorldPoint(new Vector3(0.21f, 0.895f, 0f));
                    ap.targetVector = pos;
                }
            }
        }
        if (_elapsedTime > duration * 0.5f && !sfxFlag3)
        {
            sfxFlag3 = true;
            sfx2 = AudioManager.I.PlaySFX("Heal");
            ParticleManager.I.PlayParticle("PotionEffect", ctx.transform.position + 0.8f * Vector3.up, Quaternion.identity);
        }
        if (_elapsedTime > adjustedTime)
        {
            //Debug.Log("Use Potion End");
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }
    #region UniTask Setting
    [HideInInspector] public CancellationTokenSource cts;
    void UniTaskCancel()
    {
        cts?.Cancel();
        try
        {
            cts?.Dispose();
        }
        catch (System.Exception e)
        {

            Debug.Log(e.Message);
        }
        cts = null;
    }
    #endregion
    bool once = false;
    [HideInInspector] public SFX sfx2;
    async UniTask Heal(CancellationToken token)
    {
        await UniTask.Yield(token);
        if (once) return;
        once = true;
        float du = 3.6f;
        //Gear 기어 (신복의 기어) 007_QuickHealGear
        bool outValue = false;
        if (DBManager.I.HasGear("007_QuickHealGear", out outValue))
        {
            if (outValue)
            {
                int level = DBManager.I.GetGearLevel("007_QuickHealGear");
                if (level == 0)
                {
                    du = 2.4f;
                }
                else if (level == 1)
                {
                    du = 1.95f;
                }

            }
        }
        float e = 0;
        // 1. 계산 영역 (루프 진입 전)
        float startHealth = DBManager.I.currData.currHealth;
        float amountToHeal = healAmount * ctx.maxHealth;
        float targetHealth = Mathf.Min(startHealth + amountToHeal, ctx.maxHealth);

        while (!token.IsCancellationRequested)
        {
            e += Time.deltaTime;
            float ratio = Mathf.Clamp01(e / du); // ratio가 1을 넘지 않도록 방어
            float acceleratedT = Mathf.Pow(ratio, 1.5f);

            // Lerp의 시작값과 끝값을 고정했으므로, 240에서 490까지 부드럽게 증가합니다.
            ctx.currHealth = Mathf.Lerp(startHealth, targetHealth, acceleratedT);

            // 데이터 업데이트
            DBManager.I.currData.currHealth = ctx.currHealth;

            if (ratio >= 1f) break;
            await UniTask.Yield(token);
        }
        once = false;

    }


}
