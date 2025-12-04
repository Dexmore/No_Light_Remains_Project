using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// [수정] ISubmitHandler 제거
public class GearSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private Image gearIcon;
    [SerializeField] private Image slotBackground;
    [SerializeField] private GameObject newIndicator;

    [Header("비장착 상태 밝기")]
    [Range(0f, 1f)]
    [SerializeField] private float dimFactor = 0.5f;

    private GearData _myData;
    private GearPanelController _controller;
    private Button _button;
    private Color _originalIconColor;

    public GearData MyData { get { return _myData; } }

    private void Awake()
    {
        _button = GetComponent<Button>();
        
        if (gearIcon != null) _originalIconColor = gearIcon.color;

        // Button의 onClick이 마우스 클릭과 엔터 키 입력을 모두 처리해줍니다.
        _button?.onClick.AddListener(HandleInteraction);
    }
    
    // ... (SetData, ClearSlot, UpdateEquipVisual 함수는 기존 그대로 유지) ...
    public void SetData(GearData data, GearPanelController controller)
    {
        _myData = data;
        _controller = controller;
        if (gearIcon != null) { gearIcon.sprite = _myData.gearIcon; gearIcon.gameObject.SetActive(true); }
        if (_button != null) _button.interactable = true;
        UpdateEquipVisual();
        if (newIndicator != null) newIndicator.SetActive(_myData.isNew);
    }

    public void ClearSlot()
    {
        _myData = null;
        _controller = null;
        if (gearIcon != null) { gearIcon.sprite = null; gearIcon.gameObject.SetActive(false); }
        if (_button != null) _button.interactable = false;
        if (gearIcon != null) gearIcon.color = _originalIconColor * dimFactor;
        if (newIndicator != null) newIndicator.SetActive(false);
    }

    public void UpdateEquipVisual()
    {
        if (_myData == null || gearIcon == null) return;
        gearIcon.color = _myData.isEquipped ? _originalIconColor : (_originalIconColor * dimFactor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_myData != null && _controller != null)
        {
            _controller.ShowSelectedGearDetails(_myData);
            if (_button.interactable) _button.Select();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (_myData != null && _controller != null)
        {
            _controller.ShowSelectedGearDetails(_myData);
        }
    }
    
    // [삭제] OnSubmit 함수 전체 삭제 (중복 실행 원인)
    // public void OnSubmit(BaseEventData eventData) { ... }

    private void HandleInteraction()
    {
        if (_myData != null && _controller != null)
        {
            if (_myData.isNew)
            {
                _myData.isNew = false;
                if (newIndicator != null) newIndicator.SetActive(false);
            }
            
            _controller.ToggleEquipGear(_myData);
        }
    }
}