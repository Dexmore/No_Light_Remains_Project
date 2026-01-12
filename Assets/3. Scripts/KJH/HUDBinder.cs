using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;
public class HUDBinder : MonoBehaviour
{
    #region UniTask Setting
    CancellationTokenSource cts;
    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
        OnEnable1();
    }
    void OnDisable() { UniTaskCancel(); OnDisable1(); }
    void OnDestroy() { UniTaskCancel(); }
    void UniTaskCancel()
    {
        cts?.Cancel();
        try
        {
            cts?.Dispose();
        }
        catch (System.Exception e)
        {

            Debug.Log(e);
        }
        cts = null;
    }
    #endregion
    PlayerControl player;
    SlicedLiquidBar healthBarFill;
    [HideInInspector] public RectTransform healthRT;
    [HideInInspector] public RectTransform goldRT;
    [HideInInspector] public RectTransform batteryRT;
    CanvasGroup goldCanvasGroup;
    TMP_Text goldText;
    float displayGold;
    float targetGold;
    Image[] potionImages;
    int displayPotionCount;
    Canvas canvas;
    public ItemNoticeText itemNoticeTextPrefab;
    Transform itemNoticeParent;
    void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();
        if (!player) player = FindFirstObjectByType<PlayerControl>();
        healthBarFill = GetComponentInChildren<SlicedLiquidBar>();
        healthRT = healthBarFill.transform.parent as RectTransform;
        goldCanvasGroup = transform.Find("HUDCanvas/TopRight/Gold").GetComponent<CanvasGroup>();
        goldRT = goldCanvasGroup.transform as RectTransform;
        goldCanvasGroup.alpha = 0f;
        goldText = goldCanvasGroup.transform.GetComponentInChildren<TMP_Text>();
        batteryRT = transform.Find("HUDCanvas/TopLeft/Bar/BatteryBar") as RectTransform;
        Transform batteryTR = transform.Find("HUDCanvas/TopLeft/Bar/BatteryBar/Fill");
        batteryFills = new Image[batteryTR.childCount];
        for (int i = 0; i < batteryFills.Length; i++)
            batteryFills[i] = batteryTR.GetChild(i).GetComponent<Image>();
        Transform potionParent = transform.Find("HUDCanvas/TopLeft/Bar/Potions");
        potionImages = new Image[potionParent.childCount];
        for (int i = 0; i < potionImages.Length; i++)
            potionImages[i] = potionParent.GetChild(i).GetChild(0).GetComponent<Image>();
        displayPotionCount = 5;
        itemNoticeParent = transform.Find("HUDCanvas/TopRight/ItemNoticeParent");
    }
    void OnEnable1()
    {
        GameManager.I.onHitAfter += HandleHit;
        GameManager.I.onParry += ParrySuccessHandler;
    }
    void OnDisable1()
    {
        GameManager.I.onHitAfter -= HandleHit;
        GameManager.I.onParry -= ParrySuccessHandler;
    }
    IEnumerator Start()
    {
        goldCanvasGroup.alpha = 0f;
        RefreshBattery();
        displayGold = DBManager.I.currData.gold;
        Refresh(0.5f);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        yield return null;
        yield return null;
        Camera camera = Camera.main;
        if (camera != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2;
        }


    }

    void ParrySuccessHandler(HitData hitData)
    {
        ParrySuccessHandler_ut(hitData, cts.Token).Forget();
    }
    async UniTask ParrySuccessHandler_ut(HitData hitData, CancellationToken token)
    {
        UIParticle localParticle = null;
        try
        {
            await UniTask.Delay(730, cancellationToken: token);
            int rnd = Random.Range(96, 138);
            localParticle = ParticleManager.I.PlayUIParticle("UIElectricity", new Vector2(rnd, 910), Quaternion.identity);
            AudioManager.I.PlaySFX("Electricity", transform.position, null, vol: 0.09f);
            await UniTask.Delay(380, cancellationToken: token);
        }
        catch (System.OperationCanceledException)
        {

        }
        finally
        {
            // 2. 어떤 상황에서도(성공 혹은 취소) 로컬 변수에 담긴 파티클만 안전하게 제거합니다.
            if (localParticle != null)
            {
                localParticle.Despawn();
                localParticle = null;
            }
        }
    }

    void HandleHit(HitData hitData)
    {
        if (hitData.target.Root() != player.transform) return;
        if (player == null) return;
        if (player.fsm.currentState == player.die && player.currHealth <= 0)
        {
            healthBarFill.Value = 0f;
            return;
        }
        healthBarFill.Value = Mathf.Clamp01(player.currHealth / player.maxHealth);
        RectTransform rect = healthBarFill.transform as RectTransform;
        Vector2 pivot = MethodCollection.RectTo1920x1080Position(rect);
        float x = pivot.x - 0.5f * rect.sizeDelta.x;
        float y = pivot.y;
        float addX = healthBarFill.xPosRange.x + (healthBarFill.xPosRange.y - healthBarFill.xPosRange.x) * healthBarFill.Value;
        healthBarHandlePos = new Vector2(x + addX, y);
        if (hitData.attackType == HitData.AttackType.Chafe)
        {
            var pa = ParticleManager.I.PlayUIParticle("UIGush3", healthBarHandlePos, Quaternion.identity);
            pa.transform.localScale = 0.5f * Vector3.one;
        }
        else
        {
            var pa = ParticleManager.I.PlayUIParticle("UIGush3", healthBarHandlePos, Quaternion.identity);
            pa.transform.localScale = 0.8f * Vector3.one;
        }
        var pa2 = ParticleManager.I.PlayUIParticle("UIGush", healthBarHandlePos, Quaternion.identity);
        pa2.transform.localScale = 0.7f * Vector3.one;
        var main = pa2.ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.59f, 0.159f, 0.196f, 1f), new Color(0.5f, 0.05f, 0.05f, 1f));
    }
    //
    [HideInInspector] public Vector2 healthBarHandlePos;
    [HideInInspector] public Vector2 batteryBarHandlePos;
    [SerializeField] Vector2 batteryPosXRange;
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
        healthBarFill.Value = Mathf.Clamp01(player.currHealth / player.maxHealth);
        RectTransform rect = healthBarFill.transform as RectTransform;
        Vector2 pivot = MethodCollection.RectTo1920x1080Position(rect);
        float x = pivot.x - 0.5f * rect.sizeDelta.x;
        float y = pivot.y;
        float addX = healthBarFill.xPosRange.x + (healthBarFill.xPosRange.y - healthBarFill.xPosRange.x) * healthBarFill.Value;
        healthBarHandlePos = new Vector2(x + addX, y);
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
        RectTransform rect = batteryRT;
        Vector2 pivot = MethodCollection.RectTo1920x1080Position(rect);
        float x = pivot.x - 0.5f * rect.sizeDelta.x;
        float y = pivot.y;
        float addX = batteryPosXRange.x + (batteryPosXRange.y - batteryPosXRange.x) * healthBarFill.Value;
        batteryBarHandlePos = new Vector2(x + addX, y);
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
        goldFadeTween = goldCanvasGroup.DOFade(1f, 0.2f).SetLink(gameObject);
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
        fadeSeq.Append(goldCanvasGroup.DOFade(0f, 2f).SetLink(gameObject));
        fadeSeq.SetLink(gameObject);
        goldFadeTween = fadeSeq;
    }
    void RefreshPotionInLoop()
    {
        if (displayPotionCount == DBManager.I.currData.currPotionCount) return;
        displayPotionCount = DBManager.I.currData.currPotionCount;
        for (int i = 0; i < potionImages.Length; i++)
            potionImages[i].gameObject.SetActive(false);
        for (int i = 0; i < displayPotionCount; i++)
            potionImages[i].gameObject.SetActive(true);



    }

    // 습득 텍스트
    // 알림 데이터를 담아둘 큐
    private Queue<string> noticeQueue = new Queue<string>();
    private bool isProcessing = false;
    // 외부에서 호출하는 함수
    public void PlayNoticeText(int type)
    {
        string message = "";
        if (SettingManager.I.setting.locale == 0) // KR
        {
            message = type switch
            {
                0 => "Item acquired.",
                1 => "Gear acquired.",
                2 => "Lantern acquired.",
                3 => "Record acquired.",
                _ => "Item acquired."
            };
        }
        else if (SettingManager.I.setting.locale == 1)
        {
            message = type switch
            {
                0 => "아이템을 습득하였습니다.",
                1 => "기어를 습득하였습니다.",
                2 => "랜턴을 습득하였습니다.",
                3 => "기록물을 습득하였습니다.",
                _ => "아이템을 습득하였습니다."
            };
        }
        // 1. 큐에 메시지 추가
        noticeQueue.Enqueue(message);
        // 2. 처리 중이 아니라면 코루틴 시작
        if (!isProcessing)
        {
            StartCoroutine(ProcessQueue());
        }
    }
    private IEnumerator ProcessQueue()
    {
        isProcessing = true;
        while (noticeQueue.Count > 0)
        {
            string msg = noticeQueue.Dequeue();
            CreateNoticeElement(msg);
            AudioManager.I.PlaySFX("Tick2");
            yield return new WaitForSeconds(0.04f);
            // --- 촤르르륵 뜨는 간격 조절 (예: 0.15초) ---
            yield return new WaitForSeconds(0.11f);
        }
        isProcessing = false;
    }
    private void CreateNoticeElement(string message)
    {
        var clone = Instantiate(itemNoticeTextPrefab, itemNoticeParent);
        ItemNoticeText element = clone.GetComponent<ItemNoticeText>();
        if (element == null) element = clone.gameObject.AddComponent<ItemNoticeText>();
        element.Setup(message);
    }

    //------ 아래 구현 예정------
    [Header("Battery Notice")]
    [SerializeField] Image batteryNoticeIcon1; // 충전(0) 또는 경고(1) 아이콘
    [SerializeField] Image batteryNoticeIcon2; // 잔고장(2) 전용 아이콘
    [SerializeField] Sprite[] batteryNoticeSprites; // 0:충전, 1:경고, 2:잔고장

    private Tween icon1Tween;
    private Tween icon2Tween;
    [HideInInspector] public bool isWarring;

    public void UpdateBatteryUI(bool isCharging, float batteryPercent, float lanternOnTime, bool hasDebuff)
    {
        isWarring = false;
        // --- 1번 아이콘 (충전 vs 부족) 로직 ---
        int icon1Index = -1;
        if (isCharging) 
        {
            icon1Index = 0; // 충전 중 우선순위 1등
            player.isBatteryMalfunction = false;
        }
        else if (batteryPercent < 0.07f && lanternOnTime > 9f)
        {
            icon1Index = 1; // 충전 중이 아닐 때만 부족 경고
            player.isBatteryMalfunction = false;
            isWarring = true;
        }

        // --- 2번 아이콘 (잔고장) 로직 ---
        // 충전 중이 아닐 때만 잔고장 표시
        bool showDebuff = !isCharging && hasDebuff;

        // --- 실제 UI 적용 ---
        HandleIconState(batteryNoticeIcon1, icon1Index, ref icon1Tween);
        HandleIconState(batteryNoticeIcon2, showDebuff ? 2 : -1, ref icon2Tween);
    }

    private void HandleIconState(Image icon, int spriteIndex, ref Tween tween)
    {
        if (spriteIndex == -1)
        {
            // 아이콘 비활성화
            if (icon.gameObject.activeSelf)
            {
                tween?.Kill();
                icon.gameObject.SetActive(false);
            }
        }
        else
        {
            // 아이콘 활성화 및 스프라이트 교체
            icon.sprite = batteryNoticeSprites[spriteIndex];
            if (!icon.gameObject.activeSelf)
            {
                icon.gameObject.SetActive(true);
                // 알파값 초기화 후 깜빡임 시작
                Color c = icon.color;
                c.a = 1f;
                icon.color = c;

                tween?.Kill();
                tween = icon.DOFade(0.15f, 0.37f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetLink(icon.gameObject);
            }
        }
    }



















}
