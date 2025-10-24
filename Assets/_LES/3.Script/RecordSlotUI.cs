using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecordSlotUI : MonoBehaviour
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private TextMeshProUGUI recordTitleText;
    [SerializeField] private GameObject newIndicator; // "New" 알림 아이콘 (선택 사항)

    private RecordData _currentRecord;
    private RecordPanelController _controller; //부모 컨트롤러
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button?.onClick.AddListener(OnSlotClicked);
    }

    // 이 슬롯에 기록물 데이터를 할당합니다.
    public void SetData(RecordData data, RecordPanelController controller)
    {
        _currentRecord = data;
        _controller = controller;

        recordTitleText.text = _currentRecord.recordTitle;
        if (newIndicator != null)
        {
            newIndicator.SetActive(_currentRecord.isNew);
        }
    }

    //이 슬롯이 클릭되었을 때의 동작
    public void OnSlotClicked()
    {
        if (_currentRecord != null && _controller != null)
        {
            // 부모 컨트롤러에게 "이 기록물의 상세 내용을 보여줘"라고 알림
            _controller.ShowRecordDetails(_currentRecord);
            
            // 한 번 클릭하면 'New' 표시 제거
            if (newIndicator != null)
            {
                _currentRecord.isNew = false;
                newIndicator.SetActive(false);
            }
        }
    }
}