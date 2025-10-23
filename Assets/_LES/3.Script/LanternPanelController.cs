using UnityEngine;

public class LanternPanelController : MonoBehaviour, ITabContent
{
    public void OnShow()
    {
        Debug.Log("랜턴 탭이 열렸습니다. 현재 랜턴의 연료, 밝기 등을 UI에 표시합니다.");
        // TODO: 랜턴 관련 데이터(연료량, 업그레이드 상태 등)를 UI에 표시하는 로직 구현
    }

    public void OnHide()
    {
        Debug.Log("랜턴 탭이 닫혔습니다.");
    }

    
}