using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class DoorStage1To0 : Interactable
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
    public override Type type => Type.Normal;

    public override bool isReady { get; set; } = true;

    public override bool isAuto => false;
    void Awake()
    {
        isReady = true;
    }
    public override void Run()
    {
        if (GameManager.I.isOpenDialog || GameManager.I.isOpenPop || GameManager.I.isOpenInventory) return;
        Run_ut(cts.Token).Forget();
        isReady = false;
        transform.SetParent(null);
        AudioManager.I.PlaySFX("DoorOpen2");
        DontDestroyOnLoad(gameObject);
    }
    async UniTask Run_ut(CancellationToken token)
    {
        GameManager.I.LoadSceneAsync("Stage0");
        ElevatorUp elevatorUp = null;
        elevatorUp = FindAnyObjectByType<ElevatorUp>();
        await UniTask.Delay(800, cancellationToken: token);
        if (elevatorUp == null) elevatorUp = FindAnyObjectByType<ElevatorUp>();
        await UniTask.WaitUntil(() => !GameManager.I.isSceneWaiting, cancellationToken: token);
        if (elevatorUp == null) elevatorUp = FindAnyObjectByType<ElevatorUp>();
        await UniTask.Delay(200, cancellationToken: token);
        if (elevatorUp == null) elevatorUp = FindAnyObjectByType<ElevatorUp>();
        //
        GameManager.I.SetScene(new Vector2(181.48f, 10.3f), true);
        elevatorUp.isReady = false;
        SFX sfx = AudioManager.I.PlaySFX("ElevatorUp");
        Transform platform = elevatorUp.transform.Find("Platform");
        DOTween.Kill(platform);
        platform.transform.localPosition = new Vector3(0f, 4.25f, 0f);
        await UniTask.Delay(500, cancellationToken: token);
        platform.DOLocalMoveY(0f,1.5f).SetEase(Ease.Linear).Play().SetLink(gameObject);
        //181.48 , 10
        await UniTask.Delay(1500, cancellationToken: token);
        elevatorUp.isReady = true;
        platform.transform.localPosition = Vector3.zero;
        sfx?.Despawn();
        sfx = null;
        Destroy(gameObject);
    }


}
