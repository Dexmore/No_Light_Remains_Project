using UnityEngine; // Sprite, TextArea를 사용하기 위해

// [System.Serializable]을 추가해야 Inspector에서 편집할 수 있습니다.
[System.Serializable]
public class ItemData 
{
    public string itemName;
    public Sprite icon;
    public enum ItemType { Equipment, Material }
    public ItemType type;
    
    [TextArea(3, 10)] // Inspector에서 여러 줄로 입력
    public string itemDescription; 
    
    public bool isNew; 

    // 생성자 (테스트 데이터 및 코드용)
    public ItemData(string name, Sprite icon, ItemType type, string description, bool isNew = true)
    {
        this.itemName = name;
        this.icon = icon;
        this.type = type;
        this.itemDescription = description;
        this.isNew = isNew;
    }
}