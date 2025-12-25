using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using DG.Tweening;
public class PlayerInteraction : MonoBehaviour
{
    #region UniTask Setting
    [HideInInspector] public CancellationTokenSource cts;
    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
        Init();
    }
    void OnDisable() => UniTaskCancel();
    void OnDestroy() => UniTaskCancel();
    void UniTaskCancel()
    {
        UnInit();
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
    [SerializeField] float interactDistance = 1.3f;
    [SerializeField] LayerMask interactLayer;
    Collider2D[] colliders = new Collider2D[50];
    List<SensorData> sensorDatas = new List<SensorData>();
    [SerializeField] InputActionAsset inputActionAsset;
    private InputAction interactionAction;
    private InputAction lanternAction;
    struct SensorData
    {
        public Collider2D collider;
        public Interactable interactable;
        public Lanternable lanternable;
    }
    Transform camTR;
    PlayerControl playerControl;
    [ReadOnlyInspector][SerializeField] Interactable target1;
    [ReadOnlyInspector][SerializeField] Lanternable target2;
    Vector3 distancePivot;
    PromptControl prompt;
    PlayerLight PlayerLight;
    HUDBinder hUDBinder;
    void Awake()
    {
        prompt = FindAnyObjectByType<PromptControl>();
        hUDBinder = FindAnyObjectByType<HUDBinder>();
        PlayerLight = FindAnyObjectByType<PlayerLight>();
    }
    void UnInit()
    {
        target1 = null;
        target2 = null;
        interactionAction.performed -= InputInteract;
        interactionAction.canceled -= CancelInteract;
        lanternAction.performed -= InputLanternInteract;
        lanternAction.canceled -= CancelLanternInteract;
    }
    void Init()
    {
        interactionAction = inputActionAsset.FindActionMap("Player").FindAction("Interaction");
        lanternAction = inputActionAsset.FindActionMap("Player").FindAction("LanternInteraction");
        camTR = FindAnyObjectByType<FollowCamera>(FindObjectsInactive.Include).transform.GetChild(0);
        TryGetComponent(out playerControl);
        Sensor(cts.Token).Forget();
        target1 = null;
        target2 = null;
        interactionAction.performed += InputInteract;
        interactionAction.canceled += CancelInteract;
        lanternAction.performed += InputLanternInteract;
        lanternAction.canceled += CancelLanternInteract;
        flag1 = false;
    }
    public bool press1;
    public bool press2;
    void InputInteract(InputAction.CallbackContext callback)
    {
        if (playerControl.fsm.currentState == playerControl.openInventory) return;
        if (playerControl.fsm.currentState == playerControl.die) return;
        if (!press1)
        {
            if (target1 != null)
            {
                if (target1.type != Interactable.Type.DropItem && !playerControl.Grounded) return;
                if (flag1) return;
                flag1 = true;
                AudioManager.I.PlaySFX("UIClick2");
                prompt.ClickEffect(0);
                Run(target1, cts.Token).Forget();
            }
        }
        press1 = true;
    }
    bool flag1;
    async UniTask Run(Interactable iobj, CancellationToken token)
    {
        await UniTask.Delay(110, cancellationToken: token);
        prompt.Close(0);
        await UniTask.Delay(110, cancellationToken: token);
        iobj.Run();
        flag1 = false;
    }
    void CancelInteract(InputAction.CallbackContext callback)
    {
        press1 = false;
    }
    void InputLanternInteract(InputAction.CallbackContext callback)
    {
        if (playerControl.fsm.currentState == playerControl.openInventory) return;
        if (playerControl.fsm.currentState == playerControl.die) return;
        if (target2 == null) return;
        if (!press2)
        {
            ctsLanternInteraction?.Cancel();
            ctsLanternInteraction = new CancellationTokenSource();
            var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternInteraction.Token);
            LanternInteraction(ctsLink.Token).Forget();
            sfxLanternInteraction = AudioManager.I.PlaySFX("ElectricityUsing");
            target2.PromptFill();
        }
        press2 = true;
    }
    void CancelLanternInteract(InputAction.CallbackContext callback)
    {
        press2 = false;
        ctsLanternInteraction?.Cancel();
        DOTween.Kill(prompt.lanternCanvas.transform.Find("Wrap/PressFill"));
        DOTween.Kill(prompt.lanternCanvas.transform.Find("Wrap/PressRing"));
        sfxLanternInteraction?.Despawn();
        target2?.PromptCancel();
    }
    CancellationTokenSource ctsLanternAnimationStart;
    CancellationTokenSource ctsLanternAnimationExit;
    async UniTask LanternAnimationStart(CancellationToken token)
    {
        
    }
    async UniTask LanternAnimationExit(CancellationToken token)
    {
        
    }
    CancellationTokenSource ctsLanternInteraction;
    SFX sfxLanternInteraction;
    async UniTask LanternInteraction(CancellationToken token)
    {
        Transform tr1 = prompt.lanternCanvas.transform.Find("Wrap/PressFill");
        Transform tr2 = prompt.lanternCanvas.transform.Find("Wrap/PressRing");
        DOTween.Kill(tr1);
        DOTween.Kill(tr2);
        tr1.gameObject.SetActive(true);
        tr2.gameObject.SetActive(true);
        tr1.DOScale(1f, 2f);
        tr2.DOScale(1f, 2f);
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(token);
            if (playerControl.fsm.currentState == playerControl.openInventory)
                return;
            if (playerControl.fsm.currentState == playerControl.die)
                return;
            if (target2 == null)
                return;
            target2.promptFill += 0.6f * Time.deltaTime;
            target2.promptFill = Mathf.Clamp01(target2.promptFill);
            prompt.lanternFill.fillAmount = target2.promptFill;
            if (target2.promptFill == 1f) break;
        }
        if (target2 != null)
        {
            target2.Run();
            prompt.Close(1);
            target2?.PromptCancel();
            sfxLanternInteraction?.Despawn();
            target2 = null;
        }
    }
    async UniTask Sensor(CancellationToken token)
    {
        await UniTask.Yield(token);
        GameObject fLight = PlayerLight.transform.GetChild(1).gameObject;
        while (!token.IsCancellationRequested)
        {
            int rnd = Random.Range(12, 18);
            await UniTask.DelayFrame(rnd, cancellationToken: token);
            if (playerControl.fsm.currentState == playerControl.openInventory)
            {
                prompt.Close(0);
                target1 = null;
                prompt.Close(1);
                target2?.PromptCancel();
                sfxLanternInteraction?.Despawn();
                target2 = null;
                continue;
            }
            if (playerControl.fsm.currentState == playerControl.die)
            {
                prompt.Close(0);
                target1 = null;
                prompt.Close(1);
                target2?.PromptCancel();
                sfxLanternInteraction?.Despawn();
                target2 = null;
                continue;
            }
            distancePivot = transform.position + (0.4f * playerControl.height * Vector3.up) + (0.4f * interactDistance * (camTR.position - transform.position).normalized);
            colliders = Physics2D.OverlapCircleAll(transform.position, interactDistance, interactLayer);
            sensorDatas.Clear();
            // Auto Interaction
            playerControl.isNearSavePoint = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                int find = sensorDatas.FindIndex(x => x.collider == colliders[i]);
                if (find != -1) continue;
                Transform root = colliders[i].transform.Root();
                if (root.TryGetComponent(out Interactable interactable))
                {
                    SavePoint_LSH savePoint_LSH = interactable as SavePoint_LSH;
                    if (savePoint_LSH != null)
                    {
                        playerControl.isNearSavePoint = true;
                    }
                    if (!interactable.isReady) continue;
                    SensorData data = new SensorData();
                    data.collider = colliders[i];
                    data.interactable = interactable;
                    sensorDatas.Add(data);
                    if (interactable.isAuto)
                    {
                        interactable.Run();
                        hUDBinder.Refresh();
                    }
                }
                else if (root.TryGetComponent(out Lanternable lanternable))
                {
                    if (!lanternable.isReady) continue;
                    SensorData data = new SensorData();
                    data.collider = colliders[i];
                    data.lanternable = lanternable;
                    sensorDatas.Add(data);
                    if (lanternable.isAuto)
                    {
                        lanternable.Run();
                        hUDBinder.Refresh();
                    }
                }
            }
            //
            sensorDatas.Sort
            (
                (x, y) =>
                Vector3.SqrMagnitude(x.collider.transform.position - distancePivot)
                .CompareTo(Vector3.SqrMagnitude(y.collider.transform.position - distancePivot))
            );
            if (sensorDatas.Count > 0)
            {
                // Sensor --> Interactable Prompt Open / Close
                int find = -1;
                for (int k = 0; k < sensorDatas.Count; k++)
                {
                    var _interactable = sensorDatas[k].interactable;
                    if (_interactable == null) continue;
                    if (!_interactable.isAuto)
                    {
                        find = k;
                        break;
                    }
                }
                if (find >= 0)
                {
                    if (target1 != sensorDatas[find].interactable)
                    {
                        target1 = sensorDatas[find].interactable;
                        prompt.OpenType1(target1);
                    }
                }
                else if (target1 != null)
                {
                    target1 = null;
                    prompt.Close(0);
                }

                // Sensor --> Lanternable Prompt Open / Close
                find = -1;
                if (GameManager.I.isLanternOn)
                {
                    for (int k = 0; k < sensorDatas.Count; k++)
                    {
                        var _lanternable = sensorDatas[k].lanternable;
                        if (_lanternable == null) continue;
                        if (!_lanternable.isAuto)
                        {
                            find = k;
                            break;
                        }
                    }
                    if (find >= 0)
                    {
                        if (target2 != sensorDatas[find].lanternable)
                        {
                            target2 = sensorDatas[find].lanternable;
                            prompt.OpenType2(target2);
                        }
                    }
                    else if (target2 != null)
                    {
                        prompt.Close(1);
                        target2?.PromptCancel();
                        sfxLanternInteraction?.Despawn();
                        target2 = null;
                    }
                }
                else
                {
                    if (target2 != null)
                    {
                        prompt.Close(1);
                        target2?.PromptCancel();
                        sfxLanternInteraction?.Despawn();
                        target2 = null;
                    }
                    if (prompt.lanternCanvas.gameObject.activeInHierarchy)
                    {
                        prompt.Close(1, true);
                        target2?.PromptCancel();
                        sfxLanternInteraction?.Despawn();
                        target2 = null;
                    }
                }
            }
            else if (sensorDatas.Count == 0)
            {
                if (target1 != null)
                {
                    prompt.Close(0);
                    target1 = null;
                }
                if (target2 != null)
                {
                    prompt.Close(1);
                    target2?.PromptCancel();
                    sfxLanternInteraction?.Despawn();
                    target2 = null;
                }
                if (prompt.itrctCanvas.gameObject.activeInHierarchy)
                {
                    prompt.Close(0, true);
                    target1 = null;
                }
                if (prompt.lanternCanvas.gameObject.activeInHierarchy)
                {
                    prompt.Close(1);
                    target2?.PromptCancel();
                    sfxLanternInteraction?.Despawn();
                    target2 = null;
                }
            }
        }
    }
}
