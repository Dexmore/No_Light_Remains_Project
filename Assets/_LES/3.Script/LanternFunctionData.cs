using UnityEngine;

// [System.Serializable]을 추가해야 Inspector에서 편집할 수 있습니다.
[System.Serializable] 
public class LanternFunctionData
{
    public string functionName;
    public Sprite functionIcon; // 슬롯과 원형 장착부에 사용할 이미지
    [TextArea(3, 10)]
    public string functionDescription;
    
    public bool isEquipped; // 현재 장착 여부
}