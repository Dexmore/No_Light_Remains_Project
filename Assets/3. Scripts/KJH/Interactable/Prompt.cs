using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
public class Prompt : MonoBehaviour
{
    #region UniTask Setting
    protected CancellationTokenSource cts;
    protected virtual void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
        Init(cts.Token).Forget();
    }
    bool isInit = false;
    protected virtual void OnDisable()
    {
        if (isInit)
            UniTaskCancel();
    }
    protected virtual void OnDestroy() { UniTaskCancel(); }
    void UniTaskCancel()
    {
        try
        {
            cts?.Cancel();
            cts?.Dispose();
        }
        catch (System.Exception e)
        {

            Debug.Log(e.Message);
        }
        cts = null;
    }
    async UniTask Init(CancellationToken token)
    {
        await UniTask.Yield(token);

    }
    #endregion
    Transform itrctCanvas;
    Interactable target1;
    Tween tweenLr1;
    Transform lanternCanvas;
    Interactable target2;
    Tween tweenLr2;
    void Awake()
    {
        itrctCanvas = transform.GetChild(0);
        lanternCanvas = transform.GetChild(1);
    }
    public void Open(int index, Interactable target)
    {
        if (index == 0)
        {
            target1 = target;
            ctsTracking1?.Cancel();
            ctsTracking1 = new CancellationTokenSource();
            var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsTracking1.Token);
            Open_ut(0, ctsLink.Token).Forget();
        }
        else if (index == 1)
        {
            target2 = target;
            ctsTracking2?.Cancel();
            ctsTracking2 = new CancellationTokenSource();
            var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsTracking2.Token);
            Open_ut(1, ctsLink.Token).Forget();
        }
    }
    public void Close(int index)
    {
        if (index == 0)
        {
            if (isClosing1) return;
            isClosing1 = true;
            ctsClose1?.Cancel();
            ctsClose1 = new CancellationTokenSource();
            var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsClose1.Token);
            Close_ut(0, ctsLink.Token).Forget();
        }
        else if (index == 1)
        {
            if (isClosing2) return;
            isClosing2 = true;
            ctsClose2?.Cancel();
            ctsClose2 = new CancellationTokenSource();
            var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsClose2.Token);
            Close_ut(1, ctsLink.Token).Forget();
        }
    }
    public void ClickEffect(int index)
    {
        Text text = null;
        if (index == 0)
        {
            itrctCanvas.GetChild(0).Find("Text").TryGetComponent(out text);
            DOTween.Kill(text.transform);
            text.transform.localScale = 0.3f * Vector3.one;
            text.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutSine);
            DOTween.Kill(text.material);
            text.material.SetVector("_EmissionColor", new Vector4(18f, 18f, 18f, 0.9f));
            text.material.DOVector(new Vector4(180f, 180f, 180f, 0.9f), "_EmissionColor", 0.11f)
            .OnComplete(() =>
            {
                text.material.DOVector(new Vector4(18f, 18f, 18f, 0.9f), "_EmissionColor", 0.05f);
            });
        }
        else
        {
            lanternCanvas.GetChild(0).Find("Text").TryGetComponent(out text);
            DOTween.Kill(text.transform);
            text.transform.localScale = 0.3f * Vector3.one;
            text.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutSine);
            DOTween.Kill(text.material);
            text.material.SetVector("_EmissionColor", new Vector4(18f, 18f, 18f, 0.9f));
            text.material.DOVector(new Vector4(180f, 180f, 180f, 0.9f), "_EmissionColor", 0.11f)
            .OnComplete(() =>
            {
                text.material.DOVector(new Vector4(18f, 18f, 18f, 0.9f), "_EmissionColor", 0.05f);
            });
        }
    }
    async UniTask Open_ut(int index, CancellationToken token)
    {
        Transform wrap = null;
        TMP_Text tMP_Text = null;
        Image pressRing = null;
        Text text = null;
        Interactable target = null;
        LineRenderer lr = null;
        if (index == 0)
        {
            target = target1;
            itrctCanvas.gameObject.SetActive(true);
            TrackingTarget1(token).Forget();
            wrap = itrctCanvas.GetChild(0);
            itrctCanvas.TryGetComponent(out lr);
            wrap.Find("Text(Press)").TryGetComponent(out tMP_Text);
            wrap.Find("Text").TryGetComponent(out text);
            isClosing1 = false;
            ctsClose1?.Cancel();
        }
        else if (index == 1)
        {
            target = target2;
            lanternCanvas.gameObject.SetActive(true);
            TrackingTarget2(token).Forget();
            wrap = lanternCanvas.GetChild(0);
            lanternCanvas.TryGetComponent(out lr);
            wrap.Find("Text(Press)").TryGetComponent(out tMP_Text);
            wrap.Find("Text").TryGetComponent(out text);
            wrap.Find("PressRing").TryGetComponent(out pressRing);
            isClosing2 = false;
            ctsClose2?.Cancel();
        }
        int angle = (Mathf.Abs(target.name.GetHashCode() + System.DateTime.Now.Month) % 70) - 35;
        int rnd = Mathf.Abs(target.GetInstanceID() % 10) - 5;
        angle += rnd;
        if (Mathf.Abs(angle) < 10)
        {
            if (angle < 0) angle -= 15;
            else angle += 15;
        }
        angle += 0;
        float ratio = (Mathf.Abs(target.name.GetHashCode() - System.DateTime.Now.Month) % 100) / 100f;
        float rnd1 = Mathf.Abs(target.GetInstanceID() % 11) / 150f;
        float addLength = 0f;
        if (index == 1) addLength = 1.7f;
        float length = rnd1 + addLength + 1.45f + (2.3f - 1.45f) * ratio;
        float y = length * 100f * Mathf.Cos(angle * Mathf.Deg2Rad);
        float x = length * 100f * Mathf.Sin(angle * Mathf.Deg2Rad);
        DOTween.Kill(wrap);
        wrap.localPosition = new Vector3(0.13f * x, 0.13f * y, 0);
        wrap.DOLocalMove(new Vector3(x, y, 0), 0.28f).SetEase(Ease.OutQuad);
        if (index == 0)
        {
            lr.SetPosition(0, new Vector3(0.13f * x, 0.13f * y, 0));
            tweenLr1?.Kill();
            tweenLr1 = DOTween.To
            (
                () => lr.GetPosition(0),
                (v) => lr.SetPosition(0, v),
                new Vector3(x, y, 0),
                0.28f
            ).SetEase(Ease.OutQuad);
        }
        else if (index == 1)
        {
            lr.SetPosition(0, new Vector3(0.13f * x, 0.13f * y, 0));
            tweenLr2?.Kill();
            tweenLr2 = DOTween.To
            (
                () => lr.GetPosition(0),
                (v) => lr.SetPosition(0, v),
                new Vector3(x, y, 0),
                0.28f
            ).SetEase(Ease.OutQuad);
            pressRing.color = new Color(0f, 0f, 0f, 0f);
            DOTween.Kill(pressRing);
            pressRing.DOFade(0.2f, 1f).SetEase(Ease.InSine);
        }
        DOTween.Kill(tMP_Text);
        Color color = tMP_Text.color;
        tMP_Text.color = new Color(color.r, color.g, color.b, 0f);
        tMP_Text.DOFade(0.23f, 1f).SetEase(Ease.InSine);
        DOTween.Kill(text);
        DOTween.Kill(text.transform);
        text.transform.localScale = Vector3.one;
        DOTween.Kill(text.material);
        text.material.SetVector("_EmissionColor", new Vector4(18f, 18f, 18f, 0.9f));
        Color color1 = text.color;
        text.color = new Color(color1.r, color1.g, color1.b, 0f);
        text.DOFade(0.8f, 1f).SetEase(Ease.InSine);
    }
    CancellationTokenSource ctsClose1;
    bool isClosing1;
    CancellationTokenSource ctsClose2;
    bool isClosing2;
    async UniTask Close_ut(int index, CancellationToken token)
    {
        Transform wrap = null;
        TMP_Text tMP_Text = null;
        Image pressRing = null;
        Text text = null;
        if (index == 0)
        {
            ctsTracking1?.Cancel();
            wrap = itrctCanvas.GetChild(0);
            wrap.Find("Text(Press)").TryGetComponent(out tMP_Text);
            wrap.Find("Text").TryGetComponent(out text);
            await UniTask.Yield(token);
            DOTween.Kill(tMP_Text);
            DOTween.Kill(text);
            DOTween.Kill(text.transform);
            text.transform.localScale = Vector3.one;
            DOTween.Kill(text.material);
            text.material.SetVector("_EmissionColor", new Vector4(18f, 18f, 18f, 0.9f));
            target1 = null;
            text.DOFade(0f, 0.3f).SetEase(Ease.OutSine);
            tMP_Text.DOFade(0f, 0.3f).SetEase(Ease.OutSine);
            await UniTask.Delay((int)(1000f * 0.3f), cancellationToken: token);
            itrctCanvas.gameObject.SetActive(false);
            isClosing1 = false;
        }
        else if (index == 1)
        {
            ctsTracking2?.Cancel();
            wrap = lanternCanvas.GetChild(0);
            wrap.Find("Text(Press)").TryGetComponent(out tMP_Text);
            wrap.Find("Text").TryGetComponent(out text);
            wrap.Find("PressRing").TryGetComponent(out pressRing);
            await UniTask.Yield(token);
            DOTween.Kill(tMP_Text);
            DOTween.Kill(text);
            DOTween.Kill(text.transform);
            text.transform.localScale = Vector3.one;
            DOTween.Kill(text.material);
            text.material.SetVector("_EmissionColor", new Vector4(24f, 24f, 24f, 0.9f));
            DOTween.Kill(pressRing);
            target2 = null;
            pressRing.DOFade(0f, 0.3f).SetEase(Ease.OutSine);
            text.DOFade(0f, 0.3f).SetEase(Ease.OutSine);
            tMP_Text.DOFade(0f, 0.3f).SetEase(Ease.OutSine);
            await UniTask.Delay((int)(1000f * 0.3f), cancellationToken: token);
            lanternCanvas.gameObject.SetActive(false);
            isClosing2 = false;
        }
    }
    CancellationTokenSource ctsTracking1;
    async UniTask TrackingTarget1(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(token);
            if (target1 == null) return;
            Vector3 pivot = target1.transform.position;
            itrctCanvas.position = pivot;
        }
    }
    CancellationTokenSource ctsTracking2;
    async UniTask TrackingTarget2(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(token);
            if (target2 == null) return;
            Vector3 pivot = target2.transform.position;
            lanternCanvas.position = pivot;
        }
    }









}
