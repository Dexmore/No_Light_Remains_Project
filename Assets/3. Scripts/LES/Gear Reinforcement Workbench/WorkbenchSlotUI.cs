using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WorkbenchSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("UI 요소")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject selectedBorder; // [추가] 선택 표시용 테두리 오브젝트

    private GearData _myData;
    private WorkbenchUI _parentUI;
    private Button _button;

    public GearData Data => _myData; // 외부에서 데이터 확인용 프로퍼티

    private void Awake()
    {
        _button = GetComponent<Button>();
        // Button의 OnClick은 마우스 클릭과 엔터 키 입력을 모두 처리합니다.
        _button.onClick.AddListener(OnClickSlot);
    }

    public void Setup(GearData data, WorkbenchUI parentUI)
    {
        _myData = data;
        _parentUI = parentUI;

        if (iconImage != null)
        {
            iconImage.sprite = data.gearIcon;
            iconImage.gameObject.SetActive(true);
        }
        
        // 초기화 시 선택 테두리는 끔
        if (selectedBorder != null) selectedBorder.SetActive(false);
        _button.interactable = true;
    }

    public void SetupEmpty(WorkbenchUI parentUI)
    {
        _myData = null;
        _parentUI = parentUI;

        if (iconImage != null) iconImage.gameObject.SetActive(false);
        if (selectedBorder != null) selectedBorder.SetActive(false);
        _button.interactable = true;
    }

    // [핵심] 외부(WorkbenchUI)에서 이 슬롯을 '선택 상태'로 만들 때 호출
    public void SetSelectedState(bool isSelected)
    {
        if (selectedBorder != null)
        {
            selectedBorder.SetActive(isSelected);
        }
    }

    // 마우스/키보드로 '포커스'만 되었을 때 (미리보기용)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.delta.sqrMagnitude > 0 && _button.interactable)
        {
            _button.Select();
            _parentUI.ShowPreview(_myData); // 미리보기만 보여줌
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        _parentUI.ShowPreview(_myData); // 미리보기만 보여줌
    }

    // [핵심] 클릭하거나 엔터를 쳤을 때 -> "나를 강화 대상으로 확정해줘!"
    private void OnClickSlot()
    {
        if (_myData != null)
        {
            _parentUI.ConfirmSelection(this); // 확정 선택 요청
            AudioManager.I?.PlaySFX("UIClick"); // 선택음 재생
        }
    }
}