using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // [필수] 인터페이스 사용을 위해 추가

// [수정] ISelectHandler, IPointerEnterHandler 인터페이스 추가
public class RecordSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private TextMeshProUGUI recordTitleText;
    [SerializeField] private GameObject newIndicator; // "New" 알림 아이콘

    private RecordData _currentRecord;
    private RecordPanelController _controller; // 부모 컨트롤러
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

        recordTitleText.text = _currentRecord.recordTitle.GetLocalizedString();
        if (newIndicator != null)
        {
            newIndicator.SetActive(_currentRecord.isNew);
        }
    }

    // [추가] 마우스가 슬롯 위에 올라왔을 때 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("마우스가 슬롯 위에 올라왔음!");
        ShowDetails();

        if (_button != null && _button.interactable)
        {
            _button.Select();
        }
    }

    // [추가] 키보드/컨트롤러로 슬롯이 선택되었을 때 호출
    public void OnSelect(BaseEventData eventData)
    {
        ShowDetails();
    }

    // [추가] 상세 정보 표시 요청 (중복 방지용 헬퍼 함수)
    private void ShowDetails()
    {
        // 1. 데이터가 없는지 확인
        if (_currentRecord == null)
        {
            Debug.LogError($"[{gameObject.name}] 오류: 기록물 데이터(_currentRecord)가 비어있습니다!");
            return;
        }

        // 2. 컨트롤러가 연결 안 됐는지 확인
        if (_controller == null)
        {
            Debug.LogError($"[{gameObject.name}] 오류: 컨트롤러(_controller)가 연결되지 않았습니다!");
            return;
        }

        // 3. 정상이라면 요청
        Debug.Log($"[{gameObject.name}] 상세 정보 표시 요청 보냄 -> {_currentRecord.recordTitle}");
        _controller.ShowRecordDetails(_currentRecord);
    }

    // 이 슬롯이 클릭되었을 때의 동작 (Enter 키 포함)
    public void OnSlotClicked()
    {
        // 정보 표시는 이미 OnPointerEnter/OnSelect가 처리했으므로,
        // 여기서는 'New' 마크 제거만 처리합니다.
        if (_currentRecord != null && _controller != null)
        {
            // 한 번 클릭하면 'New' 표시 제거
            if (_currentRecord.isNew)
            {
                _currentRecord.isNew = false;
                if (newIndicator != null)
                {
                    newIndicator.SetActive(false);
                }

                /////
                int find = DBManager.I.currData.recordDatas.FindIndex(x => x.Name == _currentRecord.name);
                if (find != -1)
                {
                    CharacterData.RecordData cd = DBManager.I.currData.recordDatas[find];
                    cd.isNew = false;
                    DBManager.I.currData.recordDatas[find] = cd;
                }
                /////
                
            }
        }
    }
}