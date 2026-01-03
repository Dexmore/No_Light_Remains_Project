using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// [전략 반영] Button 컴포넌트와 함께 작동하며 Navigation 시스템에 대응합니다.
[RequireComponent(typeof(Button))] 
public class WorkbenchSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("UI 요소 연결")]
    [SerializeField] private Image iconImage; 

    private GearData _data;
    private string _dbName;
    private WorkbenchUI _parentUI;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        // 버튼 클릭 시 실행할 함수 연결
        _button.onClick.AddListener(OnClickSlot);
    }

    public void Setup(GearData data, string dbName, WorkbenchUI parentUI)
    {
        _data = data;
        _dbName = dbName;
        _parentUI = parentUI;

        if (iconImage != null)
        {
            iconImage.sprite = data.gearIcon;
            iconImage.gameObject.SetActive(true);
        }
        
        // 버튼 활성화
        if(_button != null) _button.interactable = true;
    }

    // [핵심 1] 키보드/패드로 포커스가 이 슬롯으로 이동했을 때 실행
    public void OnSelect(BaseEventData eventData)
    {
        UpdateMainUI();
        // 효과음 필요 시: AudioManager.I?.PlaySFX("UI_Select");
    }

    // [핵심 2] 마우스가 올라갔을 때 실행
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스 오버 시 해당 슬롯을 'Select' 상태로 만들어 키보드와 연동성을 유지합니다.
        if (_button.interactable)
        {
            _button.Select(); 
            UpdateMainUI();
        }
    }

    // [핵심 3] 클릭/엔터 입력 시 실행 (강화 시도는 메인 UI 버튼에서 하므로 여기선 선택만)
    private void OnClickSlot()
    {
        UpdateMainUI();
    }

    private void UpdateMainUI()
    {
        if (_parentUI != null && _data != null)
        {
            _parentUI.SelectGear(_dbName, _data);
        }
    }
}