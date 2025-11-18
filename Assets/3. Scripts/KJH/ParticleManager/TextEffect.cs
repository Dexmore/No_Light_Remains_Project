using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
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
    TMP_Text tMP_Text;
    void Awake()
    {
        tMP_Text = GetComponentInChildren<TMP_Text>();
    }
    



}
