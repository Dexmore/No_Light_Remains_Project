using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
public class HUDBinder : MonoBehaviour
{
    // Player 오브젝트 지정
    PlayerControl player;
    SlicedLiquidBar healthBar;
    CanvasGroup goldCanvasGroup;
    TMP_Text goldText;
    float displayGold;
    float targetGold;
    Image[] potionImages;
    int displayPotionCount;
    void Awake()
    {
        if (!player) player = FindFirstObjectByType<PlayerControl>();
        healthBar = GetComponentInChildren<SlicedLiquidBar>();
        goldCanvasGroup = transform.Find("HUDCanvas/TopRight/Gold").GetComponent<CanvasGroup>();
        goldCanvasGroup.alpha = 0f;
        goldText = goldCanvasGroup.transform.GetComponentInChildren<TMP_Text>();
        Transform batteryFillParent = transform.Find("HUDCanvas/TopLeft/Bar/BatteryBar/Fill");
        batteryFills = new Image[batteryFillParent.childCount];
        for (int i = 0; i < batteryFills.Length; i++)
            batteryFills[i] = batteryFillParent.GetChild(i).GetComponent<Image>();
        Transform potionParent = transform.Find("HUDCanvas/TopLeft/Bar/Potions");
        potionImages = new Image[potionParent.childCount];
        for (int i = 0; i < potionImages.Length; i++)
            potionImages[i] = potionParent.GetChild(i).GetChild(0).GetComponent<Image>();
        displayPotionCount = 5;
    }
    void OnEnable()
    {
        GameManager.I.onHitAfter += HandleHit;
    }
    void OnDisable()
    {
        GameManager.I.onHitAfter -= HandleHit;
    }
    void Start()
    {
        RefreshBattery();
        displayGold = DBManager.I.currData.gold;
        Refresh(0.5f);
    }
    void HandleHit(HitData hitData)
    {
        if (hitData.target.Root() != player.transform) return;
        if (player == null) return;
        if (player.fsm.currentState == player.die) return;
        healthBar.Value = Mathf.Clamp01(player.currHealth / player.maxHealth);
        RectTransform rect = healthBar.transform as RectTransform;
        Vector2 pivot = MethodCollection.Absolute1920x1080Position(rect);
        float x = pivot.x - 0.5f * rect.sizeDelta.x;
        float y = pivot.y;
        float addX = healthBar.xPosRange.x + (healthBar.xPosRange.y - healthBar.xPosRange.x) * healthBar.Value;
        hpBarPos = new Vector2(x + addX, y);
        if (hitData.attackType == HitData.AttackType.Chafe)
        {
            var pa = ParticleManager.I.PlayUIParticle("Gush3", hpBarPos, Quaternion.identity);
            pa.transform.localScale = 0.5f * Vector3.one;
        }
        else
        {
            var pa = ParticleManager.I.PlayUIParticle("Gush3", hpBarPos, Quaternion.identity);
            pa.transform.localScale = 0.8f * Vector3.one;
        }
        var pa2 = ParticleManager.I.PlayUIParticle("Gush", hpBarPos, Quaternion.identity);
        pa2.transform.localScale = 0.7f * Vector3.one;
        var main = pa2.ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.59f, 0.159f, 0.196f, 1f), new Color(0.5f, 0.05f, 0.05f, 1f));
    }
    //
    [SerializeField] public Vector2 hpBarPos;
    public void Refresh(float time = 6f)
    {
        StopCoroutine(nameof(Refresh_co));
        StartCoroutine(nameof(Refresh_co), time);
    }
    IEnumerator Refresh_co(float time)
    {
        float startTime = Time.time;
        while (Time.time - startTime < time)
        {
            yield return YieldInstructionCache.WaitForSeconds(0.04f);
            RefreshHealthInLoop();
            RefreshGoldInLoop();
            RefreshPotionInLoop();
        }
    }
    void RefreshHealthInLoop()
    {
        if (player == null) return;
        healthBar.Value = Mathf.Clamp01(player.currHealth / player.maxHealth);
        RectTransform rect = healthBar.transform as RectTransform;
        Vector2 pivot = MethodCollection.Absolute1920x1080Position(rect);
        float x = pivot.x - 0.5f * rect.sizeDelta.x;
        float y = pivot.y;
        float addX = healthBar.xPosRange.x + (healthBar.xPosRange.y - healthBar.xPosRange.x) * healthBar.Value;
        hpBarPos = new Vector2(x + addX, y);
    }
    Image[] batteryFills;
    Color batteryFillColor = new Color(0.57f, 0.69f, 0.82f);
    public void RefreshBattery()
    {
        if (player == null) return;
        float ratio = Mathf.Clamp01(player.currBattery / player.maxBattery);
        float countFloat = ratio * batteryFills.Length;
        int count100Percent = (int)countFloat; //완전히 켜져야 하는 갯수
        // ex. 예를들어 countFloat가 5.545...f 일시... 
        // 배터리 틱 5개가 켜지고 제일 바깥쪽 틱은 켜지되 alpha가 0.545..여야함
        for (int i = 0; i < batteryFills.Length; i++)
        {
            // 완전 켜지기
            if (i < count100Percent)
                batteryFills[i].color = new Color(batteryFillColor.r, batteryFillColor.g, batteryFillColor.b, 1f);
            // 적당한 alpha값으로 켜지기
            else if (i == count100Percent)
                batteryFills[i].color = new Color(batteryFillColor.r, batteryFillColor.g, batteryFillColor.b, countFloat - count100Percent);
            // 완전 꺼지기
            else
                batteryFills[i].color = new Color(batteryFillColor.r, batteryFillColor.g, batteryFillColor.b, 0f);
        }
    }
    Tween goldCountingTween;
    Tween goldFadeTween;
    void RefreshGoldInLoop()
    {
        float currentDBGold = DBManager.I.currData.gold;
        if (targetGold == currentDBGold)
        {
            if (goldCountingTween == null || !goldCountingTween.IsActive() || !goldCountingTween.IsPlaying())
                goldText.text = displayGold.ToString("F0");
            return;
        }
        targetGold = currentDBGold;
        goldCountingTween?.Kill();
        float diff = Mathf.Abs(targetGold - displayGold);
        float duration = 1f;
        if (diff > 10000) duration = 4.5f;
        else if (diff > 1000) duration = 3.6f;
        else if (diff > 100) duration = 3f;
        else if (diff > 50) duration = 2f;
        else if (diff > 25) duration = 1.5f;
        else duration = 1f;
        goldFadeTween?.Kill(true);
        goldFadeTween = goldCanvasGroup.DOFade(1f, 0.2f);
        goldCountingTween = DOTween.To
        (
            () => displayGold,
            x => displayGold = x,
            targetGold,
            duration
        )
        .SetEase(Ease.OutCubic)
        .OnUpdate(() =>
        {
            goldText.text = displayGold.ToString("F0");
        })
        .OnComplete(StartFadeOutSequence);
    }
    private void StartFadeOutSequence()
    {
        goldFadeTween?.Kill(true);
        Sequence fadeSeq = DOTween.Sequence();
        fadeSeq.AppendInterval(1.5f);
        fadeSeq.Append(goldCanvasGroup.DOFade(0f, 2f));
        goldFadeTween = fadeSeq;
    }
    void RefreshPotionInLoop()
    {
        if (displayPotionCount == DBManager.I.currData.potionCount) return;
        displayPotionCount = DBManager.I.currData.potionCount;
        for (int i = 0; i < potionImages.Length; i++)
            potionImages[i].gameObject.SetActive(false);
        for (int i = 0; i < displayPotionCount; i++)
            potionImages[i].gameObject.SetActive(true);



    }










}
