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
        float duration = Random.Range(0.65f, 0.75f);
        Vector3 direction = new Vector3(Random.Range(0f, 0.2f), Random.Range(0.5f, 1.5f), 0f);
        Vector3 startPos = transform.position;
        if (transform.name == "DamageText") duration = 0.6f;
        else transform.position = startPos + 0.6f * direction;
        await UniTask.Delay((int)(1000f * (duration - 0.5f)), ignoreTimeScale: true, cancellationToken: token);
        if (transform.name == "DamageText")
        {
            string str1 = txt.text.Split("<size=")[0];
            string str2 = txt.text.Split("</size>")[1];
            char[] splits = (str1 + str2).ToCharArray();
            for (int i = 0; i < splits.Length; i++)
            {
                Vector3 reposition = Vector3.zero;
                reposition = -splits.Length * 0.31f * 0.5f * Vector3.right + new Vector3(0.1f, 0.02f, 0f);
                if(i == splits.Length - 1)
                {
                    reposition += 0.05f * Vector3.right;
                }
                reposition += i * 0.31f * Vector3.right;
                ParticleManager.I.PlayNumParticle(int.Parse(splits[i].ToString()), transform.position + reposition);
            }
        }
        else
        {
            transform.DOLocalMove(startPos + 0.85f * direction, duration).SetEase(Ease.OutSine).SetLink(gameObject);
        }
        await UniTask.Delay((int)(1000f * (0.55f)), ignoreTimeScale: true, cancellationToken: token);
        base.Despawn();
    }




}
