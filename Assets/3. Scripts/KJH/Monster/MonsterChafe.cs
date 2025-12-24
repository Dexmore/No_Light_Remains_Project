using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterChafe : MonoBehaviour
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
    MonsterControl control;
    Rigidbody2D rb;
    Transform child;
    List<Collider2D> monsterColliders = new List<Collider2D>();
    Dictionary<Collider2D, CancellationTokenSource> playerColliders = new Dictionary<Collider2D, CancellationTokenSource>();
    public HitData.StaggerType staggerType;
    void Awake()
    {
        control = GetComponentInParent<MonsterControl>();
        rb = GetComponentInParent<Rigidbody2D>();
        child = transform.Root().GetChild(0);
    }
    async UniTask RepulsiveLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int timeDelta = Random.Range(80, 550);
            await UniTask.Delay(timeDelta, cancellationToken: token);
            if (monsterColliders.Count == 0) continue;
            float meanPosX = 0f;
            foreach (var col in monsterColliders)
            {
                meanPosX += col.transform.position.x;
            }
            meanPosX /= monsterColliders.Count;
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
            rb.AddForce(0.7f * force * dir * Vector2.right, ForceMode2D.Impulse);
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.Root() == transform) return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (!playerColliders.ContainsKey(collision))
            {
                CancellationTokenSource ctsCA = new CancellationTokenSource();
                var ctsLinkine = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsCA.Token);
                ChafeAttackLoop(ctsLinkine.Token, collision).Forget();
                playerColliders.Add(collision, ctsCA);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
            if (!monsterColliders.Contains(collision))
                monsterColliders.Add(collision);
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.Root() == transform) return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (playerColliders.ContainsKey(collision))
            {
                playerColliders[collision].Cancel();
                try
                {
                    playerColliders[collision].Dispose();
                }
                catch
                {

                }
                playerColliders.Remove(collision);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
            monsterColliders.Remove(collision);
    }
    async UniTask ChafeAttackLoop(CancellationToken token, Collider2D playerCol)
    {
        await UniTask.Yield(token);
        while (!token.IsCancellationRequested)
        {
            if (playerColliders.Count > 0)
            {
                HitData hitData = new HitData();
                hitData.attackName = "Chafe";
                hitData.hitPoint = playerCol.transform.position + Vector3.up + 0.5f * (playerCol.transform.position - control.transform.position).normalized;
                hitData.attackType = HitData.AttackType.Chafe;
                hitData.staggerType = staggerType;
                hitData.attacker = control.transform;
                hitData.target = playerCol.transform;
                hitData.damage = Random.Range(0.15f, 0.28f) * control.adjustedAttack;
                hitData.particleNames = new string[1] { "SparkHit1" };
                GameManager.I.onHit.Invoke(hitData);
            }
            int timeDelta = Random.Range(600, 1600);
            await UniTask.Delay(timeDelta, cancellationToken: token);
        }
    }





}
