using System.Threading;
using System.Collections.Generic;
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
    List<Collider2D> collidersList = new List<Collider2D>();
    [ReadOnlyInspector][SerializeField] Collider2D target;
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
        while(!token.IsCancellationRequested)
        {
            await UniTask.DelayFrame(15, cancellationToken: token);
            distancePivot = transform.position + (0.4f * control.height * Vector3.up) + (0.4f * interactDistance * (camTR.position - transform.position).normalized);
            colliders = Physics2D.OverlapCircleAll(transform.position, interactDistance, interactLayer);
            collidersList.Clear();
            for (int i = 0; i < colliders.Length; i++)
            {
                if (collidersList.Contains(colliders[i])) continue;
                Transform root = colliders[i].transform.Root();
                if(root.TryGetComponent(out Interactable interactable))
                {
                    collidersList.Add(colliders[i]);
                }
            }
            collidersList.Sort((x, y) => Vector3.SqrMagnitude(x.transform.position - distancePivot).CompareTo(Vector3.SqrMagnitude(y.transform.position - distancePivot)));
            foreach(var e in collidersList)
            {
                Debug.DrawLine(e.transform.position, distancePivot, Color.yellow, 15f * Time.deltaTime, true);
            }
            if (collidersList.Count > 0)
            {
                if (target != collidersList[0])
                {
                    target = collidersList[0];
                }
                else
                {

                }
            }
            else
            {
                target = null;
            }

            
        }
    }
    void UnInit()
    {
        target = null;
    }
}
