using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class Bullet : PoolBehaviour
{
    public enum BulletType
    {
        Line, // 완전 직선형
        Radial, // 원형 방사
        Homing, // 유도형
        Firefly, // 반딫불형
        Parabolic, // 포물선형
        // Boomerang, //부메랑형
        // SineWave //사인파형
    }
    public Rigidbody2D rb;
    public Collider2D coll;
    public BulletType bulletType;
    public MonsterControl owner;
    [HideInInspector] public float damage;
    // 0 , 1, 2, 3
    public HitData.StaggerType staggerType;
    public bool canParry = true;
    public bool canAvoid = true;
    public Animator animator;
    AttackRange attackRange;
    [SerializeField] LayerMask groundLayer;
    ParticleSystem particleSystem;
    protected virtual void Awake()
    {
        TryGetComponent(out rb);
        attackRange = GetComponentInChildren<AttackRange>(true);
        animator = GetComponentInChildren<Animator>(true);
        coll = GetComponentInChildren<Collider2D>();
        transform.GetComponentInChildren<ParticleSystem>(true);
    }
    protected virtual void OnEnable()
    {
        attackedColliders.Clear();
        if (attackRange)
            attackRange.onTriggetStay2D += AttackRangeHandler;
        if (particleSystem)
        {
            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();
        }
    }
    protected virtual void OnDisable()
    {
        if (attackRange)
            attackRange.onTriggetStay2D -= AttackRangeHandler;
    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    protected virtual void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            Damage(coll);
        }
    }
    protected virtual void AttackRangeHandler(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            Damage(coll);
        }
    }
    protected virtual void Damage(Collider2D coll)
    {
        Vector2 hitPoint = 0.7f * coll.ClosestPoint(transform.position) + 0.3f * (Vector2)coll.transform.position + Vector2.up;
        HitData hitData = new HitData();
        if (owner != null)
            hitData.attackName = $"{owner.name}-{transform.name}";
        else
            hitData.attackName = $"Null-{transform.name}";
        hitData.attacker = transform;
        hitData.target = coll.transform;
        hitData.damage = Random.Range(0.9f, 1.1f) * damage;
        hitData.hitPoint = hitPoint;
        hitData.particleNames = new string[1] { "Hit2" };
        hitData.staggerType = staggerType;
        hitData.attackType = HitData.AttackType.Bullet;
        GameManager.I.onHit.Invoke(hitData);
    }








}
