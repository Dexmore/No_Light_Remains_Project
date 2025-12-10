using UnityEngine;
using System.Threading.Tasks;
using System.Text;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DG.Tweening;
public class GameManager : SingletonBehaviour<GameManager>
{
    protected override bool IsDontDestroy() => true;
    public Language language = Language.English;

    // 씬 넘어가도 유지시킬 변수들
    [HideInInspector] public bool isLanternOn;
    [HideInInspector] public bool isOpenPop;
    [HideInInspector] public bool isOpenDialog;
    [HideInInspector] public bool isSceneWaiting;
    [HideInInspector] public bool isShowPop0;

    // 게임의 중요 이벤트들
    public UnityAction<HitData> onHit = (x) => { };
    public UnityAction<HitData> onParry = (x) => { };
    public UnityAction<HitData> onAvoid = (x) => { };
    public UnityAction<HitData> onHitAfter = (x) => { };
    public UnityAction<HitData> onDie = (x) => { };
    public UnityAction<int, SimpleTrigger> onSimpleTriggerEnter = (x, y) => { };
    public UnityAction<int, SimpleTrigger> onSimpleTriggerExit = (x, y) => { };

    void OnEnable()
    {
        InitFade();
        InitLoading();
        isOpenPop = false;
        isOpenDialog = false;
    }
    void OnDisable()
    {

    }
    #region Load Scene
    public async void LoadSceneAsync(int index, bool loadingScreen = false)
    {
        isSceneWaiting = true;
        loadingSlider.value = 0f;
        FadeOut(1f);
        await Task.Delay(1500);
        AsyncOperation ao = SceneManager.LoadSceneAsync(index);
        if (loadingScreen)
        {
            StartLoading();
        }
        while (!ao.isDone)
        {
            await Task.Delay(10);
            loadingProgress = ao.progress;
        }
        if (loadingScreen)
        {
            while (!isLoadingDone)
            {
                await Task.Delay(10);
            }
        }
        await Task.Delay(800);
        FadeIn(0.6f);
        await Task.Delay(1500);
        isSceneWaiting = false;
    }
    public async void LoadSceneAsync(string name, bool loadingScreen = false)
    {
        isSceneWaiting = true;
        loadingSlider.value = 0f;
        FadeOut(1f);
        await Task.Delay(1500);
        AsyncOperation ao = SceneManager.LoadSceneAsync(name);
        if (loadingScreen)
        {
            StartLoading();
        }
        while (!ao.isDone)
        {
            await Task.Delay(10);
            loadingProgress = ao.progress;
        }
        if (loadingScreen)
        {
            while (!isLoadingDone)
            {
                await Task.Delay(10);
            }
        }
        await Task.Delay(800);
        FadeIn(0.6f);
        await Task.Delay(1500);
        isSceneWaiting = false;
    }
    #endregion
    #region Fade
    GameObject fadeScreen;
    Image dimImg;
    Sequence sequenceFade;
    void InitFade()
    {
        fadeScreen = transform.Find("FadeScreen").gameObject;
        dimImg = transform.Find("FadeScreen/Dim").GetComponent<Image>();
        fadeScreen.SetActive(false);
        dimImg.gameObject.SetActive(false);
        dimImg.color = new Color(0f, 0f, 0f, 0f);
    }
    public void FadeOut(float time)
    {
        sequenceFade?.Kill();
        DOTween.Kill(dimImg);
        if (time == 0)
        {
            fadeScreen.SetActive(true);
            dimImg.gameObject.SetActive(true);
            dimImg.color = new Color(0f, 0f, 0f, 1f);
            return;
        }
        float startA2 = dimImg.color.a;
        if (startA2 == 0f)
        {
            //시작
            fadeScreen.SetActive(true);
            dimImg.gameObject.SetActive(true);
            dimImg.color = new Color(0f, 0f, 0f, 0f);
            //진행
            Tween tween;
            tween = dimImg.DOFade(1f, time).SetEase(Ease.OutQuad);
            tween.OnComplete(() =>
            {
                fadeScreen.SetActive(true);
                dimImg.gameObject.SetActive(true);
                dimImg.color = new Color(0f, 0f, 0f, 1f);
            });
            sequenceFade?.Append(tween);
        }
        else
        {
            //시작
            fadeScreen.SetActive(true);
            dimImg.gameObject.SetActive(true);
            //진행
            Tween tween;
            tween = dimImg.DOFade(1f, (1f - startA2) * 1.5f + 0.5f);
            tween.OnComplete(() =>
            {
                fadeScreen.SetActive(true);
                dimImg.gameObject.SetActive(true);
                dimImg.color = new Color(0f, 0f, 0f, 1f);
            });
            sequenceFade?.Append(tween);
        }
    }
    public void FadeIn(float time)
    {
        sequenceFade?.Kill();
        DOTween.Kill(dimImg);
        if (time == 0)
        {
            fadeScreen.SetActive(false);
            dimImg.gameObject.SetActive(false);
            dimImg.color = new Color(0f, 0f, 0f, 0f);
            return;
        }
        //시작
        fadeScreen.SetActive(true);
        dimImg.gameObject.SetActive(true);
        dimImg.color = new Color(0f, 0f, 0f, 1f);
        //진행
        Tween tween;
        tween = dimImg.DOFade(0f, 2.8f).SetEase(Ease.InSine);
        tween.OnComplete(() =>
        {
            fadeScreen.SetActive(false);
            dimImg.gameObject.SetActive(false);
            dimImg.color = new Color(0f, 0f, 0f, 0f);
        });
        sequenceFade?.Append(tween);
    }
    #endregion
    #region Loading Page
    GameObject loadingScreen;
    Image loadingDim;
    Slider loadingSlider;
    float loadingProgress;
    Tween loadingTween;
    bool isLoadingDone;
    void InitLoading()
    {
        loadingScreen = transform.Find("LoadingScreen").gameObject;
        loadingDim = transform.Find("LoadingScreen/Dim").GetComponent<Image>();
        loadingSlider = transform.Find("LoadingScreen/Slider").GetComponent<Slider>();
    }
    async void StartLoading()
    {
        isLoadingDone = false;
        loadingProgress = 0f;
        loadingScreen.SetActive(true);
        loadingDim.gameObject.SetActive(true);
        loadingDim.color = new Color(0f, 0f, 0f, 1f);
        loadingTween?.Kill();
        loadingTween = loadingDim.DOFade(0f, 1.2f).SetEase(Ease.InSine);
        float elapsedTime = 0f;
        // 가짜 로딩
        while (true)
        {
            await Task.Delay(10);
            elapsedTime += 0.01f;
            loadingSlider.value = 0.4f * (elapsedTime / 3f);
            if (elapsedTime > 3f)
            {
                break;
            }
        }
        // 진짜 씬 로딩
        while (true)
        {
            await Task.Delay(10);
            loadingSlider.value = 0.4f + 0.6f * loadingProgress;
            if (loadingProgress == 1f)
            {
                break;
            }
        }
        loadingTween?.Kill();
        loadingTween = loadingDim.DOFade(1f, 1.2f).SetEase(Ease.InSine).OnComplete(() =>
        {
            loadingScreen.SetActive(false);
            loadingDim.gameObject.SetActive(false);
        });
        isLoadingDone = true;
    }

    #endregion
    #region Hit
    public Material hitTintMat;
    #endregion
    #region HitEffect
    float hitEffectStartTime;
    Transform camMainTr;
    private Tween timeSlowTween;
    public void HitEffect(Vector2 point, float amount)
    {
        if (camMainTr == null) camMainTr = Camera.main.transform;
        if (Time.time - hitEffectStartTime < 0.3f) return;
        amount = Mathf.Clamp01(amount);
        hitEffectStartTime = Time.time;
        ScreenShake(amount);
        //TimeSlow(amount);
        LineEffect(point, amount);
    }
    async void ScreenShake(float amount)
    {
        if (amount <= 0f) return;
        if (camMainTr == null) return;
        float duration = 0.037f + 0.078f * amount;
        float strength = 0.136f + 0.192f * amount;
        int vibrato = 9; // 흔들림 횟수
        float randomness = 60f; // 랜덤성
        if (duration > 0.22f) duration = 0.22f;
        DOTween.Kill(camMainTr);
        await camMainTr.DOShakePosition(duration, strength, vibrato, randomness).AsyncWaitForCompletion();
    }
    // async void TimeSlow(float amount)
    // {
    //     float a = amount;
    //     if (a <= 0.01f) a = 0.01f;
    //     float slowTimeScale = 0.2f * (1f / a);
    //     slowTimeScale = Mathf.Clamp01(slowTimeScale);
    //     float slowDuration = 0.005f + 0.01f * amount;
    //     float resetDuration = 0.008f + 0.015f * amount;
    //     timeSlowTween?.Kill();
    //     timeSlowTween = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, slowTimeScale, 0.1f).SetUpdate(true);
    //     await Task.Delay((int)(1000 * 0.1f)); // 감속 트윈이 완료될 때까지 대기
    //     await Task.Delay((int)(1000 * slowDuration));
    //     timeSlowTween = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, resetDuration)
    //         .SetUpdate(true)
    //         .OnComplete(() =>
    //         {
    //             Time.timeScale = 1f; // 혹시 모를 오차를 위해 명확하게 1로 설정
    //         });
    //     await Task.Delay((int)(1000 * resetDuration)); // 복구 트윈이 완료될 때까지 대기
    // }
    async void LineEffect(Vector2 pos, float amount)
    {

    }
    #endregion
    #region Glitch Effect
    private const int glitchDelayMs = 25;
    private readonly string[] glitchChars = { "#", "$", "%", "@", "^", "&", "*", "!", "?", "█", "■", "░", "~", "_", "=" };
    public async void GlitchText(UnityEngine.UI.Text Text, float glitchDuration = 0.4f)
    {
        if (Text == null) return;
        string originalText = Text.text;
        int totalIterations = (int)(glitchDuration * 1000f / glitchDelayMs);
        for (int i = 0; i < totalIterations; i++)
        {
            await Task.Delay(glitchDelayMs);
            // 글리치 문자열 생성 (태그가 출력되지 않도록 보장)
            Text.text = GenerateGlitchString(originalText, Text.color);
        }
        // 효과 완료 후 원본 텍스트로 복구
        Text.text = originalText;
    }
    public async void GlitchText(TMPro.TMP_Text tMP_Text, float glitchDuration = 0.4f)
    {
        if (tMP_Text == null) return;
        string originalText = tMP_Text.text;
        int totalIterations = (int)(glitchDuration * 1000f / glitchDelayMs);
        for (int i = 0; i < totalIterations; i++)
        {
            await Task.Delay(glitchDelayMs);
            // 글리치 문자열 생성 (태그가 출력되지 않도록 보장)
            tMP_Text.text = GenerateGlitchString(originalText, tMP_Text.color);
        }
        // 효과 완료 후 원본 텍스트로 복구
        tMP_Text.text = originalText;
    }
    private string GenerateGlitchString(string original, Color originalColor)
    {
        if (string.IsNullOrEmpty(original))
        {
            return "";
        }
        StringBuilder sb = new StringBuilder(original.Length * 3);
        string[] InsertChars = new string[] { "*", "░", "█" };
        for (int i = 0; i < original.Length; i++)
        {
            char originalChar = original[i];
            if (Random.value < 0.1f)
            {
                int insertCount = Random.Range(1, 4);
                for (int k = 0; k < insertCount; k++)
                {
                    sb.Append(InsertChars[Random.Range(0, InsertChars.Length)]);
                }
            }
            if (Random.value < 0.6f)
            {
                if (Random.value < 0.3f)
                {
                    sb.Append(glitchChars[Random.Range(0, glitchChars.Length)]);
                }
                else
                {
                    Color randomColor = 0.6f * originalColor + 0.4f * new Color(Random.value, Random.value, Random.value);
                    string colorHex = ColorUtility.ToHtmlStringRGB(randomColor);
                    sb.Append($"<color=#{colorHex}>{originalChar}</color>");
                }
            }
            else
            {
                sb.Append(originalChar);
            }
        }
        return sb.ToString();
    }
    #endregion

    public async void SetPlayerPosition(Vector2 vector2)
    {
        PlayerControl playerControl = null;
        FollowCamera followCamera = null;
<<<<<<< Updated upstream
        await Task.Delay(200);
        while (isSceneWaiting)
        {
            await Task.Delay(200);
=======
        await Task.Delay(50);
        while (isSceneWaiting)
        {
            await Task.Delay(50);
>>>>>>> Stashed changes
            if (playerControl == null)
                playerControl = FindAnyObjectByType<PlayerControl>();
            if (followCamera == null)
                followCamera = FindAnyObjectByType<FollowCamera>();
        }
<<<<<<< Updated upstream
        await Task.Delay(200);
        float _time = Time.time;
        while (Time.time - _time < 2f)
        {
            await Task.Delay(200);
=======
        await Task.Delay(50);
        float _time = Time.time;
        while (Time.time - _time < 2f)
        {
>>>>>>> Stashed changes
            if (playerControl == null)
                playerControl = FindAnyObjectByType<PlayerControl>();
            if (followCamera == null)
                followCamera = FindAnyObjectByType<FollowCamera>();
            if (playerControl != null && followCamera != null) break;
            await Task.Delay(50);
        }
        if (playerControl != null)
            playerControl.transform.position = vector2;
        if (followCamera != null)
            followCamera.transform.position = vector2;
    }


}
public struct HitData
{
    public string attackName;
    public Transform attacker;
    public Transform target;
    public Vector3 hitPoint;
    public float damage;
    public AttackType attackType;
    public StaggerType staggerType;
    public bool isCannotParry;
    public string[] particleNames;
    public enum AttackType
    {
        Default,
        Chafe,
    }
    public enum StaggerType
    {
        None,
        Small,
        Middle,
        Large,
    }
    public HitData(string attackName, Transform attacker, Transform target, float damage, Vector3 hitPoint, string[] particleNames, StaggerType staggerType = StaggerType.Small, AttackType attackType = AttackType.Default)
    {
        this.attackName = attackName;
        this.attacker = attacker;
        this.target = target;
        this.hitPoint = hitPoint;
        this.damage = damage;
        this.staggerType = staggerType;
        this.attackType = attackType;
        this.isCannotParry = false;
        this.particleNames = particleNames;
    }
}

public enum Language
{
    English, // 영어
    Korean,  // 한국어
    German,  // 독일어
    French,  // 프랑스어
    Spanish, // 스페인어
    ChineseSimplified, // 중국어(간체)
    Japanese, // 일본어
    Russian,  // 러시아어
    PortugueseBrazil, // 브라질-포르투갈어
    Arabic, //아랍어
    Hindi, //인도어
}
