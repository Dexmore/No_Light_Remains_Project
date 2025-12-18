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
    
}