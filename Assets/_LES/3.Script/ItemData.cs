using UnityEngine;

// [수정] 인스펙터에서 .asset 파일로 생성할 수 있게 메뉴를 추가합니다.
[CreateAssetMenu(fileName = "NewItemData", menuName = "Project Data/Item")]
public class ItemData : ScriptableObject // [수정] ScriptableObject 상속
{
    // (기존 변수들은 그대로)
    public string itemName;
    public Sprite icon;
    public enum ItemType { Equipment, Material }
    public ItemType type;

    [Header("중첩 설정")]
    [Tooltip("이 아이템이 한 슬롯에 최대 몇 개까지 들어가는지")]
    public int maxStack = 99;
    
    [TextArea(3, 10)]
    public string itemDescription; 
    
    public bool isNew; 
}