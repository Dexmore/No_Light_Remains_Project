using System.Collections.Generic;
using UnityEngine;
public class Bullet : PoolBehaviour
{
    public string bulletName;
    public MonsterControl owner;
    public float damage;
    // 0 , 1, 2, 3
    public HitData.StaggerType staggerType;
    public bool canParry = true;
    public bool canAvoid = true;
    AttackRange attackRange;
    void Awake()
    {
        attackRange = GetComponentInChildren<AttackRange>(true);
    }
    void OnEnable()
    {
        if (attackRange)
            attackRange.onTriggetStay2D += AttackRangeHandler;
    }
    void OnDisable()
    {
        if (attackRange)
            attackRange.onTriggetStay2D -= AttackRangeHandler;
    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            Damage(coll);
        }
    }
    void AttackRangeHandler(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            Damage(coll);
        }
    }
    void Damage(Collider2D coll)
    {
        Vector2 hitPoint = 0.7f * coll.ClosestPoint(transform.position) + 0.3f * (Vector2)coll.transform.position + Vector2.up;
        HitData hitData = new HitData();
        if (owner != null)
            hitData.attackName = $"{owner.name}-{bulletName}";
        else
            hitData.attackName = $"Null-{bulletName}";
        hitData.attacker = transform;
        hitData.target = coll.transform;
        hitData.damage = Random.Range(0.9f, 1.1f) * damage;
        hitData.hitPoint = hitPoint;
        hitData.particleNames = new string[1] { "Hit2" };
        hitData.staggerType = staggerType;
        hitData.attackType = HitData.AttackType.Bullet;
        GameManager.I.onHit.Invoke(hitData);
        Despawn();
    }








}
