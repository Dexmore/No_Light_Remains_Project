using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DG.Tweening;
using Steamworks;
public struct HitData
{
    public Transform attacker;
    public Transform target;
    public float damage;
    public HitData(Transform attacker, Transform target, float damage)
    {
        this.attacker = attacker;
        this.target = target;
        this.damage = damage;
    }
}
public class GameManager : SingletonBehaviour<GameManager>
{
    protected override bool IsDontDestroy() => true;
    void OnEnable()
    {
        InitFade();
        InitLoading();
    }
    void OnDisable()
    {

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
        await Task.Delay(1000);
        FadeIn(0.7f);
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
        await Task.Delay(1000);
        FadeIn(0.7f);
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
        tween = dimImg.DOFade(0f, 3.45f).SetEase(Ease.InSine);
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
    #region Hit Event

    public UnityAction<HitData> onHit = (x) => { };
    #endregion




}
