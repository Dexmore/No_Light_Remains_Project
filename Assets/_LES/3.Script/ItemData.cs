using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

// [수정] 인스펙터에서 .asset 파일로 생성할 수 있게 메뉴를 추가합니다.
[CreateAssetMenu(fileName = "NewItemData", menuName = "Project Data/Item")]
public class ItemData : ScriptableObject // [수정] ScriptableObject 상속
{
    // (기존 변수들은 그대로)
    public LocalizedString itemName;
    public Sprite icon;
    public enum ItemType { Equipment, Material }
    public ItemType type;

    [Header("중첩 설정")]
    [Tooltip("이 아이템이 한 슬롯에 최대 몇 개까지 들어가는지")]
    public int maxStack = 99;
    
    //[TextArea(3, 10)]
    public LocalizedString itemDescription; 

    [Header("Runtime Localized Strings")]
    // [추가] 실제 UI에서 사용할 번역된 문자열들입니다.
    // 인스펙터에 보일 필요가 없으므로 NonSerialized를 붙입니다.
    [System.NonSerialized] public string localizedName;
    [System.NonSerialized] public string localizedDescription;
    
    public bool isNew;

    // [수정] 데이터 로드가 완료되었을 때 실행할 이벤트 (UI 갱신용)
    public System.Action OnDataLoaded;

    public void LoadStrings()
    {
        // 1. 이름 로드
        if (!itemName.IsEmpty)
        {
            itemName.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedName = handle.Result;
            };
        }

        // 2. 설명 로드
        if (!itemDescription.IsEmpty)
        {
            itemDescription.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedDescription = handle.Result;
            };
        }
    }
}