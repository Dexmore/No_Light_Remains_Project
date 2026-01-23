using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using DG.Tweening;
public class BossHUD : MonoBehaviour
{
    [ReadOnlyInspector] public MonsterControl target;
    Transform canvas;
    TMP_Text textName;
    SlicedLiquidBar slicedLiquidBar;
    Image barImage;
    bool isFirst = true;
    void Awake()
    {
        canvas = transform.GetChild(0);
        transform.Find("Canvas/Wrap/Text(BossName)").TryGetComponent(out textName);
        slicedLiquidBar = GetComponentInChildren<SlicedLiquidBar>(true);
        barImage = slicedLiquidBar.GetComponent<Image>();
        currColor = phase1Color;
        barImage.color = currColor;
        isFirst = true;
        canvas.Find("Opening").gameObject.SetActive(false);
        canvas.Find("Wrap").gameObject.SetActive(false);
        canvas.gameObject.SetActive(false);
        openingChildTr = canvas.Find("Opening").GetChild(0);
    }
    void OnEnable()
    {
        GameManager.I.onHit += HitHandler;
        phaseProgress = 0;
    }
    void OnDisable()
    {
        GameManager.I.onHit -= HitHandler;
    }
    int phaseProgress = 0;
    void OnDestroy()
    {
        currColor = phase1Color;
        barImage.color = currColor;
    }
    public void SetTarget(MonsterControl target)
    {
        if (target == null)
        {
            canvas.gameObject.SetActive(false);
        }
        if (this.target == target) return;
        this.target = target;
        if (isFirst)
        {
            isFirst = false;
            StartCoroutine(nameof(Opening));
        }
        //
        float ratio = target.currHealth / target.maxHealth;
        slicedLiquidBar.Value = ratio;
        textName.text = target.data.Name;
        if (slicedLiquidBar.Value > 0.7f && currColor != phase1Color)
        {
            currColor = phase1Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value > 0.4f && slicedLiquidBar.Value <= 0.7f && currColor != phase2Color)
        {
            currColor = phase2Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value > 0.23f && slicedLiquidBar.Value <= 0.4f && currColor != phase3Color)
        {
            currColor = phase3Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value <= 0.23f && currColor != moribundColor)
        {
            currColor = moribundColor;
            barImage.color = currColor;
        }
        phaseProgress = 1;
    }
    Color currColor;
    Color phase1Color = new Color(0.27f, 0.64f, 0.03f, 1f);
    Color phase1MatColor = new Color(0.2f, 0.45f, 0.08f, 1f);
    Color phase2Color = new Color(0.68f, 0.7f, 0.01f, 1f);
    Color phase2MatColor = new Color(0.68f, 0.55f, 0.06f, 1f);
    Color phase3Color = new Color(0.7f, 0.24f, 0.05f, 1f);
    Color phase3MatColor = new Color(0.78f, 0.35f, 0.04f, 1f);
    Color moribundColor = new Color(1f, 0f, 0f, 1f);
    Color moribundMatColor = new Color(0.78f, 0.13f, 0.09f, 1f);
    Transform hPBarFill;
    IEnumerator Opening()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.4f);
        Test1();
        StartCoroutine(nameof(WaitBossDie));
    }
    void HitHandler(HitData hData)
    {
        if (target == null) return;
        if (target.isDie) return;
        if (hData.target.Root() != target.transform) return;
        float ratio = target.currHealth / target.maxHealth;
        slicedLiquidBar.Value = ratio;
        RectTransform rect = slicedLiquidBar.transform as RectTransform;
        Vector2 particlePos = Vector2.zero;
        Vector2 pivot = MethodCollection.RectTo1920x1080Position(rect);
        float x = pivot.x - 0.5f * rect.sizeDelta.x;
        float y = pivot.y;
        float addX = slicedLiquidBar.xPosRange.x + (slicedLiquidBar.xPosRange.y - slicedLiquidBar.xPosRange.x) * ratio;
        particlePos = new Vector2(x + addX, y);
        UIParticle uIParticle = ParticleManager.I.PlayUIParticle("UIGush", particlePos, Quaternion.identity);
        Color color1 = Color.Lerp(currColor, phase1Color, 0.5f);
        Color color2 = currColor * 0.6f;
        var main = uIParticle.ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color1, color2);
        uIParticle = ParticleManager.I.PlayUIParticle("UIGush2", particlePos, Quaternion.identity);
        main = uIParticle.ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color1, color2);
        if (slicedLiquidBar.Value > 0.7f && currColor != phase1Color)
        {
            currColor = phase1Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value > 0.4f && slicedLiquidBar.Value <= 0.7f && currColor != phase2Color)
        {
            currColor = phase2Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value > 0.23f && slicedLiquidBar.Value <= 0.4f && currColor != phase3Color)
        {
            currColor = phase3Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value <= 0.23f && currColor != moribundColor)
        {
            currColor = moribundColor;
            barImage.color = currColor;
        }
        CheckPhase();
    }
    public void Refresh()
    {
        float ratio = target.currHealth / target.maxHealth;
        slicedLiquidBar.Value = ratio;
        RectTransform rect = slicedLiquidBar.transform as RectTransform;
        Vector2 particlePos = Vector2.zero;
        Vector2 pivot = MethodCollection.RectTo1920x1080Position(rect);
        float x = pivot.x - 0.5f * rect.sizeDelta.x;
        float y = pivot.y;
        float addX = slicedLiquidBar.xPosRange.x + (slicedLiquidBar.xPosRange.y - slicedLiquidBar.xPosRange.x) * ratio;
        if (slicedLiquidBar.Value > 0.7f && currColor != phase1Color)
        {
            currColor = phase1Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value > 0.4f && slicedLiquidBar.Value <= 0.7f && currColor != phase2Color)
        {
            currColor = phase2Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value > 0.23f && slicedLiquidBar.Value <= 0.4f && currColor != phase3Color)
        {
            currColor = phase3Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value <= 0.23f && currColor != moribundColor)
        {
            currColor = moribundColor;
            barImage.color = currColor;
        }
    }
    IEnumerator WaitBossDie()
    {
        yield return YieldInstructionCache.WaitForSeconds(1.5f);
        yield return new WaitUntil(() => target == null || !target.gameObject.activeInHierarchy);
        yield return YieldInstructionCache.WaitForSeconds(1.5f);
        canvas.gameObject.SetActive(false);
    }
    Transform openingChildTr;
    Tween tweenAlpha;
    Tween tweenAlpha2;
    //[Button]
    public void Test1()
    {
        StopCoroutine(nameof(Test1_co));
        CanvasGroup group1 = canvas.Find("Opening").GetComponent<CanvasGroup>();
        CanvasGroup group2 = canvas.Find("Wrap").GetComponent<CanvasGroup>();
        CanvasGroup group3 = canvas.Find("Warring").GetComponent<CanvasGroup>();
        group1.alpha = 0f;
        group2.alpha = 0f;
        group3.alpha = 0f;
        RectTransform rectTr = openingChildTr as RectTransform;
        rectTr.anchoredPosition = new Vector2(400, 0);
        rectTr.localScale = new Vector3(0.2f, 1f, 1f);
        rectTr.localRotation = Quaternion.Euler(0f, 0f, -90f);
        canvas.gameObject.SetActive(true);
        canvas.Find("Opening").gameObject.SetActive(true);
        canvas.Find("Wrap").gameObject.SetActive(true);
        canvas.Find("Warring").gameObject.SetActive(true);
        group1.DOKill();
        group2.DOKill();
        rectTr.DOKill();
        tweenAlpha?.Kill();
        tweenAlpha2?.Kill();
        canvas.Find("Warring").GetChild(0).GetChild(0).DOKill();
        canvas.Find("Warring").GetChild(1).GetChild(0).DOKill();
        StartCoroutine(nameof(Test1_co));
    }
    IEnumerator Test1_co()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.3f);
        canvas.gameObject.SetActive(true);
        CanvasGroup group1 = canvas.Find("Opening").GetComponent<CanvasGroup>();
        CanvasGroup group2 = canvas.Find("Wrap").GetComponent<CanvasGroup>();
        CanvasGroup group3 = canvas.Find("Warring").GetComponent<CanvasGroup>();
        RectTransform rectTr = openingChildTr as RectTransform;
        tweenAlpha2 = DOTween.To(() => group3.alpha, x => group3.alpha = x, 0.7f, 2f).SetLink(gameObject);
        canvas.Find("Warring").GetChild(0).GetChild(0).DOLocalMoveX(600, 12f).SetEase(Ease.Linear).SetLink(gameObject);
        canvas.Find("Warring").GetChild(1).GetChild(0).DOLocalMoveX(-600, 12f).SetEase(Ease.Linear).SetLink(gameObject);
        // 트랜스폼 연출
        tweenAlpha = DOTween.To(() => group1.alpha, x => group1.alpha = x, 0.3f, 2f).SetLink(gameObject);
        rectTr.DOLocalRotate(Vector3.zero, 0.35f).SetEase(Ease.OutSine).SetLink(gameObject);
        yield return YieldInstructionCache.WaitForSeconds(0.3f);
        rectTr.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutSine).SetLink(gameObject);
        TMP_Text tMP_Text = canvas.Find("Wrap/Text(BossName)").GetComponent<TMP_Text>();
        tMP_Text.color = new Color(0.62f, 0.73f, 0.73f);
        tMP_Text.DOKill();
        tMP_Text.DOColor(new Color(0.8f, 0.2f, 0.2f), 0.9f).SetEase(Ease.InSine).SetLink(gameObject);
        GameManager.I.GlitchText(tMP_Text, 0.25f);
        yield return YieldInstructionCache.WaitForSeconds(0.3f);
        AudioManager.I.PlaySFX("BossWarring");
        group1.DOKill();
        tweenAlpha?.Kill();
        //크로스페이드
        float totalDuration = 2.1f;
        float switchRatio = 0.3f;
        float progress = 0f;
        float startAlpha1 = group1.alpha;
        tweenAlpha = DOTween.To(() => progress, x =>
        {
            progress = x;
            float masterAlpha = progress;
            if (progress <= switchRatio)
            {
                float t = progress / switchRatio;
                group1.alpha = Mathf.Lerp(startAlpha1, 0f, t);
                group2.alpha = t;
            }
            else
            {
                group1.alpha = 0f;
                group2.alpha = 1f;
            }
            group2.alpha *= masterAlpha;

        }, 1f, totalDuration).SetEase(Ease.OutSine).SetLink(gameObject);
        yield return YieldInstructionCache.WaitForSeconds(0.35f);
        GameManager.I.GlitchText(tMP_Text, 0.13f);
        yield return YieldInstructionCache.WaitForSeconds(0.35f);
        GameManager.I.GlitchText(tMP_Text, 0.13f);
        yield return YieldInstructionCache.WaitForSeconds(0.7f);
        GameManager.I.GlitchText(tMP_Text, 0.35f);
        canvas.Find("Opening").gameObject.SetActive(false);
        yield return YieldInstructionCache.WaitForSeconds(0.6f);
        tMP_Text.DOColor(new Color(0.62f, 0.73f, 0.73f), 0.9f).SetEase(Ease.InSine).SetLink(gameObject);
        yield return YieldInstructionCache.WaitForSeconds(0.6f);
        tweenAlpha?.Kill();
        tweenAlpha2 = DOTween.To(() => group3.alpha, x => group3.alpha = x, 0f, 2f).SetLink(gameObject)
        .OnComplete(() => canvas.Find("Warring").gameObject.SetActive(false));
        yield return YieldInstructionCache.WaitForSeconds(totalDuration - 2f);
    }
    void CheckPhase()
    {
        float ratio = target.currHealth / target.maxHealth;
        if (ratio <= 0.42f && phaseProgress <= 2)
        {
            phaseProgress = 3;
            Test2();
        }
        else if (ratio <= 0.72f && phaseProgress <= 1)
        {
            phaseProgress = 2;
            Test2();
        }
    }

    //[Button]
    public void Test2()
    {
        StartCoroutine(nameof(Test2_co));
    }
    UIParticle horLines;
    IEnumerator Test2_co()
    {
        yield return null;
        AudioManager.I.PlaySFX("BossPhase");
        horLines = ParticleManager.I.PlayUIParticle("UIHorizontalLines", new Vector2(960, 540), Quaternion.identity);
        // Text text = canvas.Find("Wrap/Text(Phase)").GetComponent<Text>();
        // text.DOKill();
        // if (phaseProgress == 2)
        // {
        //     text.DOFade(1f, 0.3f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        //     text.text = "Phase2";
        // }
        // else if (phaseProgress == 3)
        // {
        //     text.DOFade(1f, 0.3f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        //     text.text = "Phase3";
        // }
        yield return YieldInstructionCache.WaitForSeconds(2.7f);
        horLines?.Despawn();
        horLines = null;
        // text.DOKill();
        // text.DOFade(0f, 0.5f).SetLink(gameObject);
    }







}
