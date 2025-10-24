using UnityEngine;

public class GearPanelController : MonoBehaviour, ITabContent
{
    public void OnShow()
    {
        Debug.Log("기어 탭이 열렸습니다. 장착 중인 기어 정보를 불러와 슬롯에 표시합니다.");
        // TODO: 플레이어가 장착한 기어 목록을 불러와 UI에 표시하는 로직 구현
    }

    public void OnHide()
    {
        Debug.Log("기어 탭이 닫혔습니다.");
    }
}