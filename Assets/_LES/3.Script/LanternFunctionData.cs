using UnityEngine;

[CreateAssetMenu(fileName = "NewLanternData", menuName = "Project Data/Lantern Function")]
public class LanternFunctionData : ScriptableObject // [수정] ScriptableObject 상속
{
    // (기존 변수들은 그대로)
    public string functionName;
    public Sprite functionIcon;
    [TextArea(3, 10)]
    public string functionDescription;
    
    public bool isEquipped;
    
    // (참고: 이 데이터에는 'isNew'가 없었으므로 추가하지 않았습니다.)
}