using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHP = 100f;
    public float currentHP = 100f;

    [Header("Lighthouse")]
    public float maxLight = 100f;
    public float currentLight = 100f;

    [Header("Currency")]
    public int gold = 0;

    // 이벤트: HUD가 이걸 구독해서 값이 바뀔 때마다 자동 업데이트
    public event Action<float, float> OnHPChanged;
    public event Action<float, float> OnLightChanged;
    public event Action<int> OnGoldChanged;

    void Start()
    {
        // 시작 시 HUD 초기화
        OnHPChanged?.Invoke(currentHP, maxHP);
        OnLightChanged?.Invoke(currentLight, maxLight);
        OnGoldChanged?.Invoke(gold);
    }
    void OnEnable()
    {
        EventManager.I.onAttack += Handler_AttackEvnet;
    }
    void OnDisable()
    {
        EventManager.I.onAttack -= Handler_AttackEvnet;
    }

    // 상태 변경용 메서드
    public void ApplyDamage(float dmg)
    {
        currentHP = Mathf.Max(0, currentHP - dmg);
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public void AddLight(float v)
    {
        currentLight = Mathf.Clamp(currentLight + v, 0, maxLight);
        OnLightChanged?.Invoke(currentLight, maxLight);
    }

    public void AddGold(int v)
    {
        gold = Mathf.Max(0, gold + v);
        OnGoldChanged?.Invoke(gold);
    }

#if UNITY_EDITOR
    // Inspector에서 값 수정 시에도 HUD가 즉시 반영되도록
    void OnValidate()
    {
        OnHPChanged?.Invoke(currentHP, maxHP);
        OnLightChanged?.Invoke(currentLight, maxLight);
        OnGoldChanged?.Invoke(gold);
    }
#endif

    void Handler_AttackEvnet(EventManager.AttackData attackData)
    {
        if (attackData.target.root != transform) return;
        ApplyDamage(attackData.damage);
    }

}
