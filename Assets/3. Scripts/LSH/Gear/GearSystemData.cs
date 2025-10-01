using UnityEngine;

public enum GearType
{
    Passive,
    OnHit,
    Utility,
}

public enum StatType
{
    MoveSpeed,
    DashCooldown,     // 값이 작을수록 좋음 → 음수 보정 허용
    MaxHealth,
    AttackPower,
    JumpPower,
    AttackSpeed,
    SkillDamage,
    ExtraJumpCount,   // 더블점프: +1 주면 됨
    ComboExtraAnimCount // 콤보 추가 애니메이션 +1
}

public enum CCType { Freeze, Stun, Root } // 빙결, 스턴, 속박
public enum DOTType { Burn, Bleed }

[System.Serializable]
public struct StatModifier
{
    public StatType stat;
    public float value;
    public bool isPercent; // true면 % 보정(0.1f = +10%), false면 절대값
}

[System.Serializable]
public struct CrowdControl
{
    public CCType type;
    public float duration;        // 지속시간
    public float shatterBonus;    // Freeze 전용: 해제 시 추가데미지 (없으면 0)
    public bool disableMovement;  // Stun/Root 구분 보조 (Root는 false로 공격 가능)
    public bool disableActions;   // Stun은 true, Root는 false 등
}

[System.Serializable]
public struct DOTSpec
{
    public DOTType dotType;
    public float dps;           // 초당 데미지
    public float duration;      // 지속시간
    public float tickInterval;  // 틱 간격 (예: 0.5f)
}

[System.Serializable]
public struct OnHitEffect
{
    [Header("발동 확률 (0~1)")]
    [Range(0, 1f)] public float procChance;

    [Header("군중제어 (선택)")]
    public bool applyCC;
    public CrowdControl cc;

    [Header("도트데미지 (선택)")]
    public bool applyDOT;
    public DOTSpec dot;
}

[System.Serializable]
public struct UtilityEffect
{
    [Header("추가 재화")]
    public bool addCurrency;
    [Range(0, 1f)] public float currencyBonusPercent; // 드랍/획득량 % 증가

    [Header("상점 할인")]
    public bool shopDiscount;
    [Range(0, 1f)] public float shopDiscountPercent; // 0.2 = 20% 할인
}

[CreateAssetMenu(fileName = "NewGear", menuName = "Game/Gear")]
public class GearSystemData : ScriptableObject
{
    [Header("Meta")]
    public GearType gearType;
    public string gearName;
    [TextArea] public string description;
    public Sprite icon;
    public int gearLevel = 1;
    public int gearCost = 100;

    [Header("Passive (장착 시 즉시 적용)")]
    public StatModifier[] passiveStats;

    [Header("OnHit (공격 적중 시 확률 발동)")]
    public OnHitEffect[] onHitEffects;

    [Header("Utility (획득/상점 등 경제 효과)")]
    public UtilityEffect utility;

    [Header("중복 장착 규칙")]
    public bool stackable;         // 같은 기어 여러개 허용?
    public int maxStacks = 1;
}
