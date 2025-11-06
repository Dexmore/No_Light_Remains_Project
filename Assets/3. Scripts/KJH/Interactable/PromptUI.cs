using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class PromptUI : MonoBehaviour
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
    #endregion


    
    async UniTask Init(CancellationToken token)
    {

        await UniTask.Yield(token);
    }
    public void Open()
    {

    }
    public void Close()
    {

    }

    

}
