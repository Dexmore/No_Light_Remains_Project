using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class Repulsive : MonoBehaviour
{
    #region UniTask Setting
    [HideInInspector] public CancellationTokenSource cts;
    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
        RepulsiveLoop(cts.Token).Forget();
    }
    void OnDisable() => UniTaskCancel();
    void OnDestroy() => UniTaskCancel();
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
    public LayerMask targetLayer;
    Rigidbody2D rb;
    Transform child;
    List<Collider2D> colliders = new List<Collider2D>();
    void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        child = transform.Root().GetChild(0);
    }
    async UniTask RepulsiveLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int timeDelta = Random.Range(80, 550);
            await UniTask.Delay(timeDelta, cancellationToken: token);
            if (colliders.Count == 0) continue;
            float meanPosX = 0f;
            foreach (var col in colliders)
            {
                meanPosX += col.transform.position.x;
            }
            meanPosX /= colliders.Count;
            if (meanPosX == 0)
                meanPosX = transform.position.x + child.right.x;
            float dir = transform.position.x - meanPosX;
            float dist = Mathf.Abs(dir);
            dist = Mathf.Clamp(dist, 0.5f, 1.5f);
            float force = 1 / dist;
            if (dir < 0) dir = -1f;
            else if (dir == 0)
                dir = (Random.value < 0.5f) ? 1f : -1f;
            else dir = 1f;
            rb.AddForce(0.22f * force * dir * Vector2.right, ForceMode2D.Impulse);
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.Root() == transform) return;
        if (((1 << collision.gameObject.layer) & targetLayer) == 0) return;
        if (collision.gameObject.TryGetComponent(out DropItem dropItem))
            if (!colliders.Contains(collision))
                colliders.Add(collision);
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.Root() == transform) return;
        if (((1 << collision.gameObject.layer) & targetLayer) == 0) return;
        if (collision.gameObject.TryGetComponent(out DropItem dropItem))
            colliders.Remove(collision);
    }




}
