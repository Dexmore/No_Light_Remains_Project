using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
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
    }
    Transform camTR;
    PlayerController_LSH control;
    [ReadOnlyInspector][SerializeField] Interactable target1;
    [ReadOnlyInspector][SerializeField] Interactable target2;
    Vector3 distancePivot;
    Prompt prompt;
    LightSystem lightSystem;
    void Awake()
    {
        prompt = FindAnyObjectByType<Prompt>();
        lightSystem = FindAnyObjectByType<LightSystem>();
    }
    void UnInit()
    {
        target1 = null;
        target2 = null;
        interactionAction.performed -= InputInteraction;
        interactionAction.canceled -= CancelInteraction;
        lanternAction.performed -= InputLantern;
        lanternAction.canceled -= CancelLantern;
    }
    void Init()
    {
        interactionAction = inputActionAsset.FindActionMap("Player").FindAction("Interaction");
        lanternAction = inputActionAsset.FindActionMap("Player").FindAction("LanternInteraction");
        camTR = FindAnyObjectByType<FollowCamera>(FindObjectsInactive.Include).transform.GetChild(0);
        TryGetComponent(out control);
        Sensor(cts.Token).Forget();
        target1 = null;
        target2 = null;
        interactionAction.performed += InputInteraction;
        interactionAction.canceled += CancelInteraction;
        lanternAction.performed += InputLantern;
        lanternAction.canceled += CancelLantern;
    }
    public bool press1;
    public bool press2;
    void InputInteraction(InputAction.CallbackContext callback)
    {
        if(!press1)
        {
            if(target1 != null)
            {
                DropItem dropItem = target1 as DropItem;
                if(dropItem != null)
                {
                    AudioManager.I.PlaySFX("Button1");
                    dropItem.Get();
                    prompt.Close(0);                
                }
            }
        }
        press1 = true;
    }
    void CancelInteraction(InputAction.CallbackContext callback)
    {
        press1 = false;
    }
    void InputLantern(InputAction.CallbackContext callback)
    {
        press2 = true;
        if(target2 == null) return;
    }
    void CancelLantern(InputAction.CallbackContext callback)
    {
        press2 = false;
    }
    async UniTask Sensor(CancellationToken token)
    {
        await UniTask.Yield(token);
        GameObject fLight = lightSystem.transform.GetChild(1).gameObject;
        while (!token.IsCancellationRequested)
        {
            int rnd = Random.Range(12, 18);
            await UniTask.DelayFrame(rnd, cancellationToken: token);
            distancePivot = transform.position + (0.4f * control.height * Vector3.up) + (0.4f * interactDistance * (camTR.position - transform.position).normalized);
            colliders = Physics2D.OverlapCircleAll(transform.position, interactDistance, interactLayer);
            sensorDatas.Clear();
            // Auto Interaction
            for (int i = 0; i < colliders.Length; i++)
            {
                int find = sensorDatas.FindIndex(x => x.collider == colliders[i]);
                if (find != -1) continue;
                Transform root = colliders[i].transform.Root();
                if (root.TryGetComponent(out Interactable interactable))
                {
                    if (!interactable.isReady) continue;
                    SensorData data = new SensorData();
                    data.collider = colliders[i];
                    data.interactable = interactable;
                    sensorDatas.Add(data);
                    switch (interactable.type)
                    {
                        case Interactable.Type.Portal:
                            Portal portal = interactable as Portal;
                            if (portal.isAuto)
                            {
                                portal.Run();
                            }
                            break;
                        case Interactable.Type.DropItem:
                            DropItem dropItem = interactable as DropItem;
                            if (dropItem.isAuto)
                            {
                                dropItem.Get();
                            }
                            break;
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
                // Interaction Prompt
                int find = -1;
                for (int k = 0; k < sensorDatas.Count; k++)
                {
                    var type = sensorDatas[k].interactable.type;
                    if (type != Interactable.Type.LightObject && type != Interactable.Type.DarkObject)
                    {
                        var _interactable = sensorDatas[k].interactable;
                        Portal portal = _interactable as Portal;
                        DropItem dropItem = _interactable as DropItem;
                        if (portal != null)
                        {
                            if (!portal.isAuto)
                            {
                                find = k;
                                break;
                            }
                        }
                        else if (dropItem != null)
                        {
                            if (!dropItem.isAuto)
                            {
                                find = k;
                                break;
                            }
                        }
                        else
                        {
                            find = k;
                            break;
                        }
                    }
                }
                if (find >= 0)
                {
                    if (target1 != sensorDatas[find].interactable)
                    {
                        target1 = sensorDatas[find].interactable;
                        prompt.Open(0, target1);
                    }
                }
                else if (target1 != null)
                {
                    target1 = null;
                    prompt.Close(0);
                }
                // Lantern Interaction Prompt
                if (fLight.activeSelf)
                {
                    find = -1;
                    for (int k = 0; k < sensorDatas.Count; k++)
                    {
                        if (sensorDatas[k].interactable.type == Interactable.Type.LightObject
                        || sensorDatas[k].interactable.type == Interactable.Type.DarkObject)
                        {
                            find = k;
                            break;
                        }
                    }
                    if (find >= 0)
                    {
                        if (target2 != sensorDatas[find].interactable)
                        {
                            target2 = sensorDatas[find].interactable;
                            prompt.Open(1, target2);
                        }
                    }
                    else if (target2 != null)
                    {
                        target2 = null;
                        prompt.Close(1);
                    }
                }
                else if (target2 != null)
                {
                    target2 = null;
                    prompt.Close(1);
                }
            }
            else if (target1 != null)
            {
                target1 = null;
                prompt.Close(0);
            }
            else if (target2 != null)
            {
                target2 = null;
                prompt.Close(1);
            }
        }
    }
}
