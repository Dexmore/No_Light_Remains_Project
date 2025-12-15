using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering.Universal;
public class PlayerLight : MonoBehaviour
{
    public bool isFreeformLight;
    public float radius;
    public LayerMask layerMask;
    public int polyCount = 127;
    Light2D spotLight;
    Light2D freeformLight;
    #region UniTask Setting
    [HideInInspector] public CancellationTokenSource cts;
    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
        if (isFreeformLight)
        {
            freeformLight.gameObject.SetActive(true);
            StartDeform(cts.Token).Forget();
        }
        else
        {
            freeformLight.gameObject.SetActive(false);
        }
    }
    void OnDisable() => UniTaskCancel();
    void OnDestroy() => UniTaskCancel();
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
    void Awake()
    {
        transform.GetChild(0).TryGetComponent(out spotLight);
        transform.GetChild(1).TryGetComponent(out freeformLight);
    }
    async UniTask StartDeform(CancellationToken token)
    {
        if (!isFreeformLight) return;
        Vector3[] buffer = new Vector3[polyCount];
        RaycastHit2D hit;
        while (true)
        {
            await UniTask.Yield(token);
            float segmentAngle = 360f / polyCount;
            for (int i = 0; i < polyCount; i++)
            {
                if (i % 5 == 1) await UniTask.Delay(1, cancellationToken: token);
                Vector2 myPos = (Vector2)transform.position;
                Vector3 dir3D = Quaternion.Euler(0f, 0f, i * segmentAngle) * Vector3.up;
                Vector2 dir = (Vector2)dir3D;
                //Debug.DrawRay(myPos, radius * dir, Color.white, 0.2f, true);
                if (hit = Physics2D.Raycast(myPos, dir, radius, layerMask))
                {
                    buffer[i] = Vector3.Slerp(buffer[i], (Vector3)(hit.point - myPos + dir * 0.1f), 50f * Time.deltaTime);
                }
                else
                {
                    buffer[i] = Vector3.Slerp(buffer[i], (Vector3)(dir * radius), 50f * Time.deltaTime);
                }
            }
            freeformLight.SetShapePath(buffer);
        }
    }
}
