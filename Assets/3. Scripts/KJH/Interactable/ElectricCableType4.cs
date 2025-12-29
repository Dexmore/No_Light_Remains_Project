using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
public class ElectricCableType4 : MonoBehaviour
{
    private float swayForce = 0.7f;
    private float swayInterval = 1.5f;
    private float impactForce = 3.0f;
    void Awake()
    {
        Rigidbody2D[] _rbs = GetComponentsInChildren<Rigidbody2D>();
        rbs = _rbs.Where(x => !x.transform.name.Contains("Anchor")).ToArray();
    }
    Collider2D col;
    void OnEnable()
    {
        if (col == null) TryGetComponent(out col);
    }
    void Start()
    {
        StartCoroutine(IdleSwayRoutine());
    }
    private Rigidbody2D[] rbs;
    IEnumerator IdleSwayRoutine()
    {
        while (true)
        {
            Vector2 totalVelo = Vector2.zero;
            foreach (Rigidbody2D rb in rbs)
            {
                totalVelo += rb.linearVelocity;
            }
            if (totalVelo.magnitude < 0.1f)
            {
                float randomDirection = Random.Range(-1f, 1f);
                foreach (Rigidbody2D rb in rbs)
                {
                    rb.AddForce(new Vector2(randomDirection * swayForce, 0), ForceMode2D.Impulse);
                }
            }
            yield return new WaitForSeconds(swayInterval + Random.Range(0f, 1f));
        }
    }
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            Damage(coll);
            Rigidbody2D rb = coll.GetComponentInChildren<Rigidbody2D>();
            if (rb == null)
                rb = coll.GetComponentInParent<Rigidbody2D>();
            if (rb)
            {
                Vector2 pos = col.bounds.center;
                Vector2 direction = ((Vector2)coll.transform.position + 1.2f * Vector2.up) - pos;
                rb.AddForce(5f * direction.normalized, ForceMode2D.Impulse);
            }
        }
    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    void Damage(Collider2D coll)
    {
        Vector2 hitPoint = 0.7f * coll.ClosestPoint(transform.position) + 0.3f * (Vector2)coll.transform.position + Vector2.up;
        Vector2 adjustPos = col.bounds.center;
        hitPoint = 0.3f * adjustPos + 0.7f * hitPoint;
        hitPoint.y = 0.3f * hitPoint.y + 0.7f * coll.transform.position.y;
        float damage = 10f;
        HitData hitData = new HitData();
        hitData.attackName = "ElectricCableType4";
        hitData.attacker = transform;
        hitData.target = coll.transform;
        hitData.damage = Random.Range(0.9f, 1.1f) * damage;
        hitData.hitPoint = hitPoint;
        hitData.particleNames = new string[1] { "ElectricHit1" };
        hitData.staggerType = HitData.StaggerType.Large;
        hitData.attackType = HitData.AttackType.Trap;
        GameManager.I.onHit.Invoke(hitData);
        StartCoroutine(nameof(ClearList));
    }
    IEnumerator ClearList()
    {
        yield return YieldInstructionCache.WaitForSeconds(1.2f);
        attackedColliders.Clear();
    }




}
