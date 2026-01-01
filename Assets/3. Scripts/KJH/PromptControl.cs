using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
public class PromptControl : MonoBehaviour
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
    async UniTask Init(CancellationToken token)
    {
        await UniTask.Yield(token);

    }
    #endregion
    [HideInInspector] public Transform itrctCanvas;
    Interactable target1;
    Tween tweenLr1;
    [HideInInspector] public Transform lanternCanvas;
    Lanternable target2;
    Tween tweenLr2;
    [HideInInspector] public Image lanternFill;
    void Awake()
    {
        itrctCanvas = transform.GetChild(0);
        lanternCanvas = transform.GetChild(1);
        lanternFill = lanternCanvas.Find("Wrap/PressFill").GetComponent<Image>();
    }
    public void OpenType1(Interactable target)
    {
        target1 = target;
        ctsTracking1?.Cancel();
        ctsTracking1 = new CancellationTokenSource();
        var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsTracking1.Token);
        Open_ut(0, ctsLink.Token).Forget();
    }
    public void OpenType2(Lanternable target)
    {
        target2 = target;
        lanternCanvas.Find("Wrap/PressFill").gameObject.SetActive(false);
        lanternCanvas.Find("Wrap/PressRing").gameObject.SetActive(false);
        lanternCanvas.Find("Wrap/PressFill").transform.localScale = 0.2f * Vector3.one;
        lanternCanvas.Find("Wrap/PressRing").transform.localScale = 0.2f * Vector3.one;
        lanternFill.fillAmount = target.promptFill;
        ctsTracking2?.Cancel();
        ctsTracking2 = new CancellationTokenSource();
        var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsTracking2.Token);
        Open_ut(1, ctsLink.Token).Forget();
    }
    public void Close(int type, bool force = false)
    {
        if (type == 0)
        {
            if (force)
            {
                ctsClose1?.Cancel();
                ctsTracking1?.Cancel();
                target1 = null;
                itrctCanvas.gameObject.SetActive(false);
                isClosing1 = false;
            }
            if (!itrctCanvas.gameObject.activeSelf) return;
            if (isClosing1) return;
            isClosing1 = true;
            ctsClose1?.Cancel();
            ctsClose1 = new CancellationTokenSource();
            var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsClose1.Token);
            Close_ut(0, ctsLink.Token).Forget();
        }
        else if (type == 1)
        {
            if (force)
            {
                ctsClose2?.Cancel();
                ctsTracking2?.Cancel();
                lanternCanvas.gameObject.SetActive(false);
                isClosing2 = false;
            }
            if (!lanternCanvas.gameObject.activeSelf) return;
            if (isClosing2) return;
            isClosing2 = true;
            ctsClose2?.Cancel();
            ctsClose2 = new CancellationTokenSource();
            var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsClose2.Token);
            Close_ut(1, ctsLink.Token).Forget();
            DOTween.Kill(lanternCanvas.transform.Find("Wrap/PressFill"));
            DOTween.Kill(lanternCanvas.transform.Find("Wrap/PressRing"));
            lanternCanvas.transform.Find("Wrap/PressFill").DOScale(0.2f, 0.5f).SetLink(gameObject);
            lanternCanvas.transform.Find("Wrap/PressRing").DOScale(0.2f, 0.5f).SetLink(gameObject);
        }
    }
    public void ClickEffect(int type)
    {
        Text text = null;
        if (type == 0)
        {
            itrctCanvas.GetChild(0).Find("Text").TryGetComponent(out text);
            DOTween.Kill(text.transform);
            text.transform.localScale = 0.3f * Vector3.one;
            text.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutSine).SetLink(gameObject);
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
            text.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutSine).SetLink(gameObject);
            DOTween.Kill(text.material);
            text.material.SetVector("_EmissionColor", new Vector4(18f, 18f, 18f, 0.9f));
            text.material.DOVector(new Vector4(180f, 180f, 180f, 0.9f), "_EmissionColor", 0.11f)
            .OnComplete(() =>
            {
                text.material.DOVector(new Vector4(18f, 18f, 18f, 0.9f), "_EmissionColor", 0.05f);
            });
        }
    }
    async UniTask Open_ut(int type, CancellationToken token)
    {
        Transform wrap = null;
        TMP_Text tMP_Text = null;
        Image pressRing = null;
        Text text = null;
        LineRenderer lr = null;
        if (type == 0)
        {
            itrctCanvas.gameObject.SetActive(true);
            TrackingTarget1(token).Forget();
            wrap = itrctCanvas.GetChild(0);
            itrctCanvas.TryGetComponent(out lr);
            wrap.Find("Text(Press)").TryGetComponent(out tMP_Text);
            wrap.Find("Text").TryGetComponent(out text);
            string keyName = SettingManager.I.GetBindingName("Interaction");
            if (keyName == "") keyName = "↑";
            text.text = keyName;
            isClosing1 = false;
            ctsClose1?.Cancel();
            int angle = (Mathf.Abs(target1.name.GetHashCode() + System.DateTime.Now.Month) % 70) - 35;
            int rnd = Mathf.Abs(target1.GetInstanceID() % 10) - 5;
            angle += rnd;
            if (Mathf.Abs(angle) < 10)
            {
                if (angle < 0) angle -= 15;
                else angle += 15;
            }
            angle += 0;
            float ratio = (Mathf.Abs(target1.name.GetHashCode() - System.DateTime.Now.Month) % 100) / 100f;
            float rnd1 = Mathf.Abs(target1.GetInstanceID() % 11) / 150f;
            float addLength = 0f;
            if (type == 1) addLength = 1.7f;
            float length = rnd1 + addLength + 1.45f + (2.3f - 1.45f) * ratio;
            float y = length * 100f * Mathf.Cos(angle * Mathf.Deg2Rad);
            float x = length * 100f * Mathf.Sin(angle * Mathf.Deg2Rad);
            DOTween.Kill(wrap);
            wrap.localPosition = new Vector3(0.13f * x, 0.13f * y, 0);
            wrap.DOLocalMove(new Vector3(x, y, 0), 0.28f).SetEase(Ease.OutQuad);
            if (type == 0)
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
            else if (type == 1)
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
                pressRing.DOFade(0.2f, 1f).SetEase(Ease.InSine).SetLink(gameObject);
            }
            DOTween.Kill(tMP_Text);
            Color color = tMP_Text.color;
            tMP_Text.color = new Color(color.r, color.g, color.b, 0f);
            tMP_Text.DOFade(0.23f, 1f).SetEase(Ease.InSine).SetLink(gameObject);
            DOTween.Kill(text);
            DOTween.Kill(text.transform);
            text.transform.localScale = Vector3.one;
            DOTween.Kill(text.material);
            text.material.SetVector("_EmissionColor", new Vector4(18f, 18f, 18f, 0.9f));
            Color color1 = text.color;
            text.color = new Color(color1.r, color1.g, color1.b, 0f);
            text.DOFade(0.8f, 1f).SetEase(Ease.InSine).SetLink(gameObject).SetLink(gameObject);
        }
        else if (type == 1)
        {
            lanternCanvas.gameObject.SetActive(true);
            TrackingTarget2(token).Forget();
            wrap = lanternCanvas.GetChild(0);
            lanternCanvas.TryGetComponent(out lr);
            wrap.Find("Text(Hold)").TryGetComponent(out tMP_Text);
            wrap.Find("Text").TryGetComponent(out text);
            string keyName = SettingManager.I.GetBindingName("LanternInteraction");
            if (keyName == "") keyName = "C";
            text.text = keyName;
            wrap.Find("PressRing").TryGetComponent(out pressRing);
            isClosing2 = false;
            ctsClose2?.Cancel();
            int angle = (Mathf.Abs(target2.name.GetHashCode() + System.DateTime.Now.Month) % 70) - 35;
            int rnd = Mathf.Abs(target2.GetInstanceID() % 10) - 5;
            angle += rnd;
            if (Mathf.Abs(angle) < 10)
            {
                if (angle < 0) angle -= 15;
                else angle += 15;
            }
            angle += 0;
            float ratio = (Mathf.Abs(target2.name.GetHashCode() - System.DateTime.Now.Month) % 100) / 100f;
            float rnd1 = Mathf.Abs(target2.GetInstanceID() % 11) / 150f;
            float addLength = 0f;
            if (type == 1) addLength = 1.7f;
            float length = rnd1 + addLength + 1.45f + (2.3f - 1.45f) * ratio;
            float y = length * 100f * Mathf.Cos(angle * Mathf.Deg2Rad);
            float x = length * 100f * Mathf.Sin(angle * Mathf.Deg2Rad);
            DOTween.Kill(wrap);
            wrap.localPosition = new Vector3(0.13f * x, 0.13f * y, 0);
            wrap.DOLocalMove(new Vector3(x, y, 0), 0.28f).SetEase(Ease.OutQuad);
            if (type == 0)
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
            else if (type == 1)
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
                pressRing.DOFade(0.2f, 1f).SetEase(Ease.InSine).SetLink(gameObject);
            }
            DOTween.Kill(tMP_Text);
            Color color = tMP_Text.color;
            tMP_Text.color = new Color(color.r, color.g, color.b, 0f);
            tMP_Text.DOFade(0.23f, 1f).SetEase(Ease.InSine).SetLink(gameObject);
            DOTween.Kill(text);
            DOTween.Kill(text.transform);
            text.transform.localScale = Vector3.one;
            DOTween.Kill(text.material);
            text.material.SetVector("_EmissionColor", new Vector4(18f, 18f, 18f, 0.9f));
            Color color1 = text.color;
            text.color = new Color(color1.r, color1.g, color1.b, 0f);
            text.DOFade(0.8f, 1f).SetEase(Ease.InSine).SetLink(gameObject);
        }

    }
    CancellationTokenSource ctsClose1;
    bool isClosing1;
    CancellationTokenSource ctsClose2;
    bool isClosing2;
    async UniTask Close_ut(int type, CancellationToken token)
    {
        Transform wrap = null;
        TMP_Text tMP_Text = null;
        Image pressRing = null;
        Text text = null;
        if (type == 0)
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
            text.DOFade(0f, 0.3f).SetEase(Ease.OutSine).SetLink(gameObject);
            tMP_Text.DOFade(0f, 0.3f).SetEase(Ease.OutSine).SetLink(gameObject);
            await UniTask.Delay((int)(1000f * 0.3f), cancellationToken: token);
            itrctCanvas.gameObject.SetActive(false);
            isClosing1 = false;
        }
        else if (type == 1)
        {
            ctsTracking2?.Cancel();
            wrap = lanternCanvas.GetChild(0);
            wrap.Find("Text(Hold)").TryGetComponent(out tMP_Text);
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
            pressRing.DOFade(0f, 0.3f).SetEase(Ease.OutSine).SetLink(gameObject);
            text.DOFade(0f, 0.3f).SetEase(Ease.OutSine).SetLink(gameObject);
            tMP_Text.DOFade(0f, 0.3f).SetEase(Ease.OutSine).SetLink(gameObject);
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
