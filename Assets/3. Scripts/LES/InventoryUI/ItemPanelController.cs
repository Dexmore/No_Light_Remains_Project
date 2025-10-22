using UnityEngine;

public class ItemPanelController : MonoBehaviour, ITabContent
{
    public void OnShow()
    {
        Debug.Log("소지템 탭이 열렸습니다. 플레이어 인벤토리 데이터를 불러와 UI를 갱신합니다.");
        // TODO: 실제 인벤토리 아이템 목록을 UI에 표시하는 로직 구현
    }

    public void OnHide()
    {
        Debug.Log("소지템 탭이 닫혔습니다.");
    }
}