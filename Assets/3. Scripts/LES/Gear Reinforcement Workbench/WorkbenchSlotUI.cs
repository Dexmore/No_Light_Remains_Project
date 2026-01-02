using UnityEngine;
using UnityEngine.UI;

public class WorkbenchSlotUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;
    //[SerializeField] private GameObject equipIndicator; // 장착 중인지 표시 (선택)

    private GearData _data;      // 원본 데이터 (SO)
    private string _dbName;      // DB 저장 키값
    private WorkbenchUI _parentUI; // 메인 UI 참조

    public void Setup(GearData data, string dbName, bool isEquipped, WorkbenchUI parentUI)
    {
        _data = data;
        _dbName = dbName;
        _parentUI = parentUI;

        // 아이콘 설정
        if (iconImage != null) iconImage.sprite = data.gearIcon;
        
        // 장착 표시 (필요하다면)
        //if (equipIndicator != null) equipIndicator.SetActive(isEquipped);

        // 버튼 클릭 이벤트 연결
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickSlot);
    }

    private void OnClickSlot()
    {
        // 메인 UI에게 "나 선택됐어!"라고 알림
        _parentUI.SelectGear(_dbName, _data);
    }
}