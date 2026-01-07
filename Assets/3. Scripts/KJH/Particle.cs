using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering.Universal;
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
        PlayLight2D(cts.Token).Forget();
        if (!loop) Play_ut(cts.Token).Forget();
    }
    async UniTask Play_ut(CancellationToken token)
    {
        await UniTask.Delay(1, ignoreTimeScale: true, cancellationToken: token);
        await UniTask.Delay((int)(1000f * (ps.main.duration + 0.1f)), ignoreTimeScale: true, cancellationToken: token);
        base.Despawn();
    }
    async UniTask PlayLight2D(CancellationToken token)
    {
        Light2D light2D = GetComponentInChildren<Light2D>(true);
        if (!light2D) return;
        light2D.gameObject.SetActive(true);
        float duration = ps.main.duration;
        float targetIntensity = light2D.intensity;
        //light2D.intensity = 0f;



        await UniTask.Yield(token);



        await UniTask.Yield(token);

    }


}
