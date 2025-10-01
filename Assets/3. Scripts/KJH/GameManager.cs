using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using DG.Tweening;
using Steamworks;
public class GameManager : SingletonBehaviour<GameManager>
{
    protected override bool IsDontDestroy() => true;
    protected override void Awake()
    {
        base.Awake();
        if(transform.childCount > 0)
            imgFade = transform.Find("Canvas/Fade").GetComponent<Image>();
    }

    #region Load Scene
    public async void LoadSceneAsync(int index)
    {
        FadeOut(1.2f);
        await Task.Delay(1200);
        AsyncOperation ao = SceneManager.LoadSceneAsync(index);
        while (!ao.isDone)
        {
            await Task.Delay(10);
        }
        await Task.Delay(1200);
        FadeIn(2f);
    }
    public async void LoadSceneAsync(string name)
    {
        FadeOut(1.2f);
        await Task.Delay(1200);
        AsyncOperation ao = SceneManager.LoadSceneAsync(name);
        while (!ao.isDone)
        {
            await Task.Delay(10);
        }
        await Task.Delay(1200);
        FadeIn(2f);
    }
    #endregion
    #region Fade
    public bool isFade;
    Image imgFade;
    Sequence sqFade;
    public void FadeOut(float duration)
    {
        isFade = true;
        if (duration == 0)
        {
            imgFade.gameObject.SetActive(true);
            imgFade.color = new Color(0f, 0f, 0f, 1f);
            return;
        }
        sqFade.Kill();
        imgFade.gameObject.SetActive(true);
        imgFade.color = new Color(0f, 0f, 0f, 0f);
        Tween tween;
        tween = imgFade.DOColor(new Color(0f, 0f, 0f, 1f), duration).SetEase(Ease.OutQuad);
        sqFade?.Append(tween);
    }
    public void FadeIn(float duration)
    {
        isFade = false;
        if (duration == 0)
        {
            imgFade.gameObject.SetActive(false);
            imgFade.color = new Color(0f, 0f, 0f, 0f);
            return;
        }
        sqFade.Kill();
        imgFade.gameObject.SetActive(true);
        imgFade.color = new Color(0f, 0f, 0f, 1f);
        Tween tween;
        tween = imgFade.DOColor(new Color(0f, 0f, 0f, 0f), duration).SetEase(Ease.InSine)
        .OnComplete(() => imgFade.gameObject.SetActive(false));
        sqFade?.Append(tween);
    }
    #endregion




}
