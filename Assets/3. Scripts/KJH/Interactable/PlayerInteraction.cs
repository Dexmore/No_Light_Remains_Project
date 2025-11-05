using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
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
    // List<Collider2D> colliderList = new List<Collider2D>();
    // List<Interactable> interactableList = new List<Interactable>();
    List<SensorData> sensorDatas = new List<SensorData>();
    struct SensorData
    {
        public Collider2D collider;
        public Interactable interactable;
    }
    [ReadOnlyInspector][SerializeField] Interactable target;
    Vector3 distancePivot;
    Transform camTR;
    PlayerController_LSH control;
    void Init()
    {
        camTR = FindAnyObjectByType<FollowCamera>(FindObjectsInactive.Include).transform.GetChild(0);
        TryGetComponent(out control);
        Sensor(cts.Token).Forget();
        target = null;
    }
    async UniTask Sensor(CancellationToken token)
    {
        await UniTask.Yield(token);
        while (!token.IsCancellationRequested)
        {
            int rnd = Random.Range(12, 18);
            await UniTask.DelayFrame(rnd, cancellationToken: token);
            distancePivot = transform.position + (0.4f * control.height * Vector3.up) + (0.4f * interactDistance * (camTR.position - transform.position).normalized);
            colliders = Physics2D.OverlapCircleAll(transform.position, interactDistance, interactLayer);
            sensorDatas.Clear();
            for (int i = 0; i < colliders.Length; i++)
            {
                int find = sensorDatas.FindIndex(x => x.collider == colliders[i]);
                if (find != -1) continue;
                Transform root = colliders[i].transform.Root();
                if (root.TryGetComponent(out Interactable interactable))
                {
                    SensorData data = new SensorData();
                    data.collider = colliders[i];
                    data.interactable = interactable;
                    sensorDatas.Add(data);
                }
            }
            sensorDatas.Sort
            (
                (x, y) =>
                Vector3.SqrMagnitude(x.collider.transform.position - distancePivot)
                .CompareTo(Vector3.SqrMagnitude(y.collider.transform.position - distancePivot))
            );
#if UNITY_EDITOR
            foreach (var e in sensorDatas)
            {
                Debug.DrawLine(e.collider.transform.position, distancePivot, Color.yellow, 15f * Time.deltaTime, true);
            }
#endif
            if (sensorDatas.Count > 0)
            {
                if (target != sensorDatas[0].interactable)
                {
                    target = sensorDatas[0].interactable;
                    OpenPrompt(target);
                }
            }
            else
            {
                target = null;
                ClosePrompt();
            }
        }
    }
    void OpenPrompt(Interactable interactable)
    {
        if (interactable.type == Interactable.Type.Portal)
        {
            Portal portal = interactable as Portal;
            if (portal.isAuto)
                portal.Run();
        }
        else if(interactable.type == Interactable.Type.DropItem)
        {
            DropItem dropItem = interactable as DropItem;
            if (dropItem.isAuto)
                dropItem.Get();
        }
    }
    void ClosePrompt()
    {

    }
    void UnInit()
    {
        target = null;
    }
}
