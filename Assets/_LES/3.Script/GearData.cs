using UnityEngine;
using UnityEngine.Localization; // 필수
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "NewGearData", menuName = "Project Data/Gear")]
public class GearData : ScriptableObject // [수정] ScriptableObject 상속
{
    // (기존 변수들은 그대로)
    public LocalizedString gearName;
    public Sprite gearIcon;
    //[TextArea(3, 10)]
    public LocalizedString gearDescription;

    [Header("Runtime Localized Strings")]
    // [추가] 실제 UI에서 사용할 번역된 문자열들입니다.
    // 인스펙터에 보일 필요가 없으므로 NonSerialized를 붙입니다.
    [System.NonSerialized] public string localizedName;
    [System.NonSerialized] public string localizedDescription;
    
    [Range(1, 3)]
    public int cost;
    
    public bool isEquipped;
    public bool isNew;

    public void LoadStrings()
    {
        // 1. 이름 로드
        if (!gearName.IsEmpty)
        {
            gearName.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedName = handle.Result;
            };
        }

        // 2. 설명 로드
        if (!gearDescription.IsEmpty)
        {
            gearDescription.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedDescription = handle.Result;
            };
        }
    }
    
    [Header("Enhancement Stats")]
    // 예: Attack, Defense, Health ...
    public StatType statType; 
    
    // 0강일 때의 기본 수치 (예: 공격력 10)
    public float baseStatValue; 
    
    // 1강당 증가하는 수치 (예: 2씩 증가)
    // 복잡한 공식(지수 상승 등)을 원하면 별도 커브(AnimationCurve) 사용 가능
    public float statGrowthPerLevel; 

    // 최대 강화 가능 레벨 (예: 10강)
    public int maxLevel = 10;
    
    // 이 장비 강화에 필요한 전용 재료 그룹 (없으면 공용 사용)
    public EnhancementManager specificEnhancementSettings;
}

// 스탯 타입 정의 (필요시 확장)
public enum StatType { Attack, Defense, MaxHp, Speed }