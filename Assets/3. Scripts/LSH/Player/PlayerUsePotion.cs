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
    private const float duration = 1.2f;
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
                adjustedTime = duration * 1.2f + 0.4f;
                break;
            case 2:
                adjustedTime = duration * 1.3f + 0.6f;
                break;
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
        float du = 2.8f;
        float e = 0;
        float startHealth = DBManager.I.currData.currHealth;
        ctx.hUDBinder.Refresh(du + 1.5f);
        while (!token.IsCancellationRequested)
        {
            e += Time.deltaTime;
            float ratio = (e / du);
            float acceleratedT = ratio * ratio;
            ctx.currHealth = Mathf.Lerp(startHealth, ctx.maxHealth, acceleratedT);
            ctx.currHealth = Mathf.Clamp(ctx.currHealth, 0f, ctx.maxHealth);
            DBManager.I.currData.currHealth = ctx.currHealth;
            await UniTask.Yield(token);
            if (ratio >= 1) break;
        }
    }


}
