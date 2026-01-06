using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings; // 언어 감지용

[CreateAssetMenu(fileName = "NewGearData", menuName = "Project Data/Gear")]
public class GearData : ScriptableObject
{
    [Header("기본 정보")]
    public LocalizedString gearName;
    public Sprite gearIcon;

    [Header("기본설명")]
    public LocalizedString gearDescription; 

    [Space(10)]
    [Header("==================================================")]
    [Header("[ 0강 텍스트 (신규) ] - 상단: 위(영어) / 아래(한글)")]
    [TextArea(3, 5)] public string upgradeMain_EN; 
    [TextArea(3, 5)] public string upgradeMain_KR; 

    [Space(10)]
    [Header("[ 1강 텍스트 (신규) ] - 하단: 위(영어) / 아래(한글)")]
    [TextArea(3, 5)] public string upgradeSub_EN; 
    [TextArea(3, 5)] public string upgradeSub_KR; 
    [Header("==================================================")]

    [Header("Runtime Strings")]
    [System.NonSerialized] public string localizedName;
    [System.NonSerialized] public string localizedNormalDescription; 
    
    [Range(1, 3)]
    public int cost;
    public bool isEquipped;
    public bool isNew;

    // 현재 강화 레벨
    public int currentLevel = 0; 

    // 강화 비용 설정
    public EnhancementManager.LevelInfo[] specificEnhancementSettings;

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

        // ✅ [복구됨] 설명 로드 로직
        if (!gearDescription.IsEmpty)
        {
            gearDescription.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedNormalDescription = handle.Result;
            };
        }
    }

    // [유지] 현재 언어 및 레벨에 맞춰 수동 텍스트 반환하는 함수
    public string GetEffectText(int level)
    {
        // 현재 언어 코드 가져오기 (예: "ko", "en")
        string localeCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        bool isKR = localeCode.Contains("ko"); // 한국어인지 확인

        if (level == 0)
            return isKR ? upgradeMain_KR : upgradeMain_EN;
        else
            return isKR ? upgradeSub_KR : upgradeSub_EN;
    }
}