using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class Particle : PoolBehaviour
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
    ParticleSystem ps;
    public bool loop;
    void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
    }
    public void Play()
    {
        ps.Play();
        if (!loop) Play_ut(cts.Token).Forget();
    }
    async UniTask Play_ut(CancellationToken token)
    {
        await UniTask.Delay(1, ignoreTimeScale: true, cancellationToken: token);
        await UniTask.Delay((int)(1000f * (ps.main.duration + 0.1f)), ignoreTimeScale: true, cancellationToken: token);
        base.Despawn();
    }
}
