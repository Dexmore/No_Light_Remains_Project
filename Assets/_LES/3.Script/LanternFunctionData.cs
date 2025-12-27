using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "NewLanternData", menuName = "Project Data/Lantern Function")]
public class LanternFunctionData : ScriptableObject // [수정] ScriptableObject 상속
{
    // (기존 변수들은 그대로)
    public LocalizedString functionName;
    public Sprite functionIcon;
    //[TextArea(3, 10)]
    public LocalizedString functionDescription;

    [Header("Runtime Localized Strings")]
    // [추가] 실제 UI에서 사용할 번역된 문자열들입니다.
    // 인스펙터에 보일 필요가 없으므로 NonSerialized를 붙입니다.
    [System.NonSerialized] public string localizedName;
    [System.NonSerialized] public string localizedDescription;
    
    public bool isEquipped;
    public bool isNew;
    
    [Header("설정")]
    [Tooltip("체크 해제 시, 장착하면 다시는 뺄 수 없게 됩니다.")]
    public bool isRemovable = true;

    public void LoadStrings()
    {
        // 1. 이름 로드
        if (!functionName.IsEmpty)
        {
            functionName.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedName = handle.Result;
            };
        }

        // 2. 설명 로드
        if (!functionDescription.IsEmpty)
        {
            functionDescription.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedDescription = handle.Result;
            };
        }
    }
    
    // (참고: 이 데이터에는 'isNew'가 없었으므로 추가하지 않았습니다.)
}