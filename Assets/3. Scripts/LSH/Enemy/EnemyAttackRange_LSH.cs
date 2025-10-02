using UnityEngine;
using System.Collections.Generic;

public class EnemyAttackRange_LSH : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private LayerMask playerLayers;
    [SerializeField] private int damage = 20;

    [Header("Timing (optional)")]
    [SerializeField] private bool autoAttack = false;
    [SerializeField] private float attackInterval = 0.8f;
    private float _nextAttackTime;

    private readonly HashSet<IDamageable_LSH> _hitThisSwing = new();

    void Update()
    {
        if (!autoAttack) return;
        if (Time.time < _nextAttackTime) return;
        DoEnemyAttack();
        _nextAttackTime = Time.time + attackInterval;
    }

    // 애니메이션 이벤트에서 호출해도 됩니다.
    public void DoEnemyAttack()
    {
        if (!attackPoint) return;

        _hitThisSwing.Clear();

        var hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayers);
        foreach (var h in hits)
        {
            // 1) 패링 먼저 확인 (성공 시 이 타격은 무효화)
            var parry = h.GetComponentInParent<IParry_LSH>();
            if (parry != null && parry.TryParry(this, attackPoint.position))
                continue;

            // 2) 패링 실패 → 데미지
            var dmg = h.GetComponentInParent<IDamageable_LSH>();
            if (dmg == null) continue;
            if (_hitThisSwing.Contains(dmg)) continue;

            _hitThisSwing.Add(dmg);
            dmg.TakeDamage(damage, transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!attackPoint) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
