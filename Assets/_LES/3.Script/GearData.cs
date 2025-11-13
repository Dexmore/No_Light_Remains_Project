using UnityEngine;

[CreateAssetMenu(fileName = "NewGearData", menuName = "Project Data/Gear")]
public class GearData : ScriptableObject // [수정] ScriptableObject 상속
{
    // (기존 변수들은 그대로)
    public string gearName;
    public Sprite gearIcon;
    [TextArea(3, 10)]
    public string gearDescription;
    
    [Range(1, 3)]
    public int cost;
    
    public bool isEquipped;
    public bool isNew;
    
}