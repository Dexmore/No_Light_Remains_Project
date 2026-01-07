using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
public class LightPillar : MonoBehaviour
{
    Collider2D col;
    void OnEnable()
    {
        if (col == null) TryGetComponent(out col);
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
                direction.y = 0f;
                rb.AddForce(7f * direction.normalized +  0.5f * Vector2.up, ForceMode2D.Impulse);
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
        hitData.attackName = "LightPillar";
        hitData.attacker = transform;
        hitData.target = coll.transform;
        hitData.damage = Random.Range(0.9f, 1.1f) * damage;
        hitData.hitPoint = hitPoint;
        hitData.particleNames = new string[1] { "ElectricHit1" };
        hitData.staggerType = HitData.StaggerType.Large;
        hitData.attackType = HitData.AttackType.Trap;
        GameManager.I.onHit.Invoke(hitData);
    }

}
