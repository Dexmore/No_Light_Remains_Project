using UnityEngine;

public class Enemy_LSH : MonoBehaviour, IDamageable_LSH
{
    public int maxHp = 30;
    int hp;

    void Awake() => hp = maxHp;

    public void TakeDamage(int dmg, Vector3 hitFrom)
    {
        hp -= dmg;
        Debug.Log($"[Enemy] Took {dmg}, HP:{hp}");
        if (hp <= 0) Die();
    }

    // 호환용
    public void TakeDamage(int dmg) => TakeDamage(dmg, transform.position);

    void Die()
    {
        Debug.Log("[Enemy] Dead");
        Destroy(gameObject);
    }
}
