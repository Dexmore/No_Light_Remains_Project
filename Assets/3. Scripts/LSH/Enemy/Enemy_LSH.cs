using UnityEngine;

public class Enemy_LSH : MonoBehaviour, IDamageable_LSH
{
    public int maxHp = 30;
    public int hp;

    void Awake() => hp = maxHp;

    public void TakeDamage(int dmg, Vector3 hitFrom)
    {
        hp -= dmg;
        // 넉백, 히트 플래시, 히트스톱 등 효과 추가 가능
        if (hp <= 0) Die();
    }

    // 기존 코드 호환용
    public void TakeDamage(int dmg) => TakeDamage(dmg, transform.position);

    void Die()
    {
        // 사망 처리(애니, 파티클, 드랍 등)
        Destroy(gameObject);
    }
}
