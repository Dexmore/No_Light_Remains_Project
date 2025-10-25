using UnityEngine;

[System.Serializable] //Inspector에서 보기 편하도록
public class GearData
{
    public string gearName;
    public Sprite gearIcon; //그리드 슬롯 및 우측 상세 이미지에 사용할 아이콘
    [TextArea(3, 10)]
    public string gearDescription;
    
    [Range(1, 3)] //기어 코스트는 1~6 사이로 가정
    public int cost;
    
    public bool isEquipped; //현재 장착 여부

    //생성자 (테스트 데이터용)
    public GearData(string name, Sprite icon, string desc, int cost, bool equipped = false)
    {
        this.gearName = name;
        this.gearIcon = icon;
        this.gearDescription = desc;
        this.cost = cost;
        this.isEquipped = equipped;
    }
}