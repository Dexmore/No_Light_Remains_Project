using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GearSlotUI : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private Image gearIcon;
    [SerializeField] private Image slotBackground; // 이 필드는 이제 사용 안 하지만, 연결은 해두세요.

    [Header("비장착 상태 밝기")]
    [Range(0f, 1f)]
    [SerializeField] private float dimFactor = 0.5f;

    private GearData _myData;
    private GearPanelController _controller;
    private Button _button;
    
    private Color _originalIconColor; // [수정] 아이콘의 원본 색상

    public GearData MyData { get { return _myData; } }

    private void Awake()
    {
        _button = GetComponent<Button>();
        
        // [수정] 아이콘의 원본 색상을 저장합니다.
        if (gearIcon != null)
        {
            _originalIconColor = gearIcon.color;
        }
    }
    
    public void SetData(GearData data, GearPanelController controller)
    {
        _myData = data;
        _controller = controller;

        if (gearIcon != null) // null 체크
        {
            gearIcon.sprite = _myData.gearIcon;
            gearIcon.gameObject.SetActive(true);
        }
        
        if (_button != null) _button.interactable = true;
        
        UpdateEquipVisual();
    }
    
    public void ClearSlot()
    {
        _myData = null;
        _controller = null;

        if (gearIcon != null) // null 체크
        {
            gearIcon.sprite = null;
            gearIcon.gameObject.SetActive(false);
        }
        
        if (_button != null) _button.interactable = false;
        
        // [수정] 빈 슬롯일 때도 아이콘을 어둡게 합니다.
        if (gearIcon != null)
        {
            gearIcon.color = _originalIconColor * dimFactor;
        }
    }
    
    public void UpdateEquipVisual()
    {
        if (_myData == null || gearIcon == null) return;

        // [수정된 핵심 로직]
        // 장착 시: 100% 밝기의 원본 아이콘 색상
        // 비장착 시: 원본 아이콘 색상 * 어둡기 비율
        gearIcon.color = _myData.isEquipped 
            ? _originalIconColor 
            : (_originalIconColor * dimFactor);
    }
    
    // (OnSelect, OnSubmit 함수는 변경 없음)
    public void OnSelect(BaseEventData eventData)
    {
        if (_myData != null && _controller != null)
        {
            _controller.ShowSelectedGearDetails(_myData);
        }
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        if (_myData != null && _controller != null)
        {
            _controller.ToggleEquipGear(_myData);
        }
    }
}