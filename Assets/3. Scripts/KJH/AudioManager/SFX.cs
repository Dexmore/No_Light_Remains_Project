using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class SFX : PoolBehaviour
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

            Debug.Log(e);
        }
        cts = null;
    }
    #endregion
    public AudioSource aus;
    public void Play(AudioClip clip, float vol, float time, float is3d)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        Play_ut(clip, vol, time, is3d, cts.Token).Forget();
    }
    async UniTask Play_ut(AudioClip clip, float vol, float time, float is3d, CancellationToken token)
    {
        await UniTask.Delay(2, ignoreTimeScale: true, cancellationToken: token);
        aus.loop = false;
        aus.clip = clip;
        aus.spatialBlend = is3d;
        aus.volume = vol;
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        if (!aus.enabled) aus.enabled = true;
        await UniTask.Delay(2, ignoreTimeScale: true, cancellationToken: token);
        aus.pitch = Random.Range(0.988f, 1.01f);
        aus.Play();
        await UniTask.Delay((int)(1000f * (time + 0.2f)), ignoreTimeScale: true, cancellationToken: token);
        base.Despawn();
    }
}
