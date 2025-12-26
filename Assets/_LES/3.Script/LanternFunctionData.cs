using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "NewLanternData", menuName = "Project Data/Lantern Function")]
public class LanternFunctionData : ScriptableObject
{
    // (기존 변수 유지)
    public LocalizedString functionName;
    public Sprite functionIcon;
    public LocalizedString functionDescription;

    [Header("Runtime Localized Strings")]
    [System.NonSerialized] public string localizedName;
    [System.NonSerialized] public string localizedDescription;
    
    public bool isEquipped;
    public bool isNew;

    [Header("설정")]
    [Tooltip("체크 해제 시, 장착하면 다시는 뺄 수 없게 됩니다.")]
    public bool isRemovable = true; // [추가] 탈부착 가능 여부 (기본값 True)

    public void LoadStrings()
    {
        // (기존 코드 유지)
        if (!functionName.IsEmpty)
        {
            functionName.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedName = handle.Result;
            };
        }

        if (!functionDescription.IsEmpty)
        {
            functionDescription.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedDescription = handle.Result;
            };
        }
    }
}