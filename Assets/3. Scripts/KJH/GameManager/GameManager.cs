using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DG.Tweening;
using Steamworks;
public struct HitData
{
    public string attackName;
    public Transform attacker;
    public Transform target;
    public Vector3 hitPoint;
    public float damage;
    public AttackType attackType;
    public StaggerType staggerType;
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
    public HitData(string attackName, Transform attacker, Transform target, float damage, Vector3 hitPoint, StaggerType staggerType = StaggerType.Small, AttackType attackType = AttackType.Default)
    {
        this.attackName = attackName;
        this.attacker = attacker;
        this.target = target;
        this.hitPoint = hitPoint;
        this.damage = damage;
        this.staggerType = staggerType;
        this.attackType = attackType;
    }
}
public class GameManager : SingletonBehaviour<GameManager>
{
    
    protected override bool IsDontDestroy() => true;
    void OnEnable()
    {
        InitFade();
        InitLoading();
        //onHit += HitHandler;
    }
    void OnDisable()
    {
        //onHit -= HitHandler;
    }
    #region Load Scene
    public async void LoadSceneAsync(int index, bool loadingScreen = false)
    {
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
    }
    public async void LoadSceneAsync(string name, bool loadingScreen = false)
    {
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
    public UnityAction<HitData> onHit = (x) => { };
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
        TimeSlow(amount);
        LineEffect(point, amount);
    }
    async void ScreenShake(float amount)
    {
        if (amount <= 0f) return;
        if (camMainTr == null) return;
        float duration = 0.03f + 0.06f * amount;
        float strength = 0.08f + 0.14f * amount;
        int vibrato = 7; // 흔들림 횟수
        float randomness = 70f; // 랜덤성
        if (duration > 0.2f) duration = 0.2f;
        DOTween.Kill(camMainTr);
        await camMainTr.DOShakePosition(duration, strength, vibrato, randomness).AsyncWaitForCompletion();
    }
    async void TimeSlow(float amount)
    {
        float a = amount;
        if (a <= 0.01f) a = 0.01f;
        float slowTimeScale = 0.1f * (1f / a);
        slowTimeScale = Mathf.Clamp01(slowTimeScale);
        float slowDuration = 0.008f + 0.011f * amount;
        float resetDuration = 0.01f + 0.022f * amount;
        timeSlowTween?.Kill();
        timeSlowTween = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, slowTimeScale, 0.1f).SetUpdate(true);
        await Task.Delay((int)(1000 * 0.1f)); // 감속 트윈이 완료될 때까지 대기
        await Task.Delay((int)(1000 * slowDuration));
        timeSlowTween = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, resetDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Time.timeScale = 1f; // 혹시 모를 오차를 위해 명확하게 1로 설정
            });
        await Task.Delay((int)(1000 * resetDuration)); // 복구 트윈이 완료될 때까지 대기
    }
    async void LineEffect(Vector2 pos, float amount)
    {

    }
    #endregion


}
