using UnityEngine;

public class RecordPanelController : MonoBehaviour, ITabContent
{
    public void OnShow()
    {
        Debug.Log("기록물 탭이 열렸습니다. 지금까지 발견한 기록물 목록을 UI에 표시합니다.");
        // TODO: 수집한 기록물 데이터를 불러와 목록 형태로 UI에 표시하는 로직 구현
    }

    public void OnHide()
    {
        Debug.Log("기록물 탭이 닫혔습니다.");
    }
}