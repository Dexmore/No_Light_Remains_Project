using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using DG.Tweening;
public class TextEffect : PoolBehaviour
{
    #region UniTask Setting
    CancellationTokenSource cts;
    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
    }
    void OnDisable() { UniTaskCancel(); }
    void OnDestroy() { UniTaskCancel(); }
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
    #endregion
    [HideInInspector] public Text txt;
    void Awake()
    {
        txt = GetComponentInChildren<Text>();
    }

    public void Play()
    {
        Play_ut(cts.Token).Forget();
    }
    async UniTask Play_ut(CancellationToken token)
    {
        DOTween.Kill(transform);
        await UniTask.Delay(1, ignoreTimeScale: true, cancellationToken: token);
        float duration = Random.Range(0.55f, 0.75f);
        Vector3 direction = new Vector3(Random.Range(0f, 0.2f), Random.Range(0.5f, 1.5f), 0f);
        transform.DOLocalMove(transform.position + direction, duration).SetEase(Ease.OutSine);
        await UniTask.Delay((int)(1000f * (duration + 0.1f)), ignoreTimeScale: true, cancellationToken: token);
        base.Despawn();
    }




}
