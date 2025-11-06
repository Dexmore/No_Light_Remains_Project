using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LanternSlotUI : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private Image functionIcon; // 1번 요청 (이미지 표시)

    [Header("비장착 상태 밝기")]
    [Range(0f, 1f)]
    [SerializeField] private float dimFactor = 0.5f; // 2번 요청 (어둡게)

    private LanternFunctionData _myData;
    private LanternPanelController _controller;
    private Button _button;
    private Color _originalIconColor;

    // 컨트롤러가 이 슬롯의 데이터를 읽을 수 있게 해주는 프로퍼티
    public LanternFunctionData MyData { get { return _myData; } }

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (functionIcon != null)
        {
            _originalIconColor = functionIcon.color;
        }
    }
    
    public void SetData(LanternFunctionData data, LanternPanelController controller)
    {
        _myData = data;
        _controller = controller;

        if (functionIcon != null)
        {
            functionIcon.sprite = _myData.functionIcon;
            functionIcon.gameObject.SetActive(true);
        }
        if (_button != null) _button.interactable = true;
        
        UpdateEquipVisual(); // 2번 요청
    }

    public void ClearSlot()
    {
        _myData = null;
        _controller = null;

        if (functionIcon != null)
        {
            functionIcon.sprite = null;
            functionIcon.gameObject.SetActive(false);
        }
        if (_button != null) _button.interactable = false;
    }
    
    /// <summary>
    /// (2번 요청) 장착 상태에 따라 아이콘의 밝기를 업데이트합니다.
    /// </summary>
    public void UpdateEquipVisual()
    {
        if (_myData == null || functionIcon == null) return;

        functionIcon.color = _myData.isEquipped 
            ? _originalIconColor 
            : (_originalIconColor * dimFactor);
    }
    
    /// <summary>
    /// (3번 요청) 이 슬롯이 방향키로 '선택'되었을 때 호출됩니다.
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        if (_myData != null && _controller != null)
        {
            _controller.ShowFunctionDetails(_myData);
        }
    }

    /// <summary>
    /// 이 슬롯에서 'Submit'(Enter/Space)이 눌렸을 때 호출됩니다. (장착/해제)
    /// </summary>
    public void OnSubmit(BaseEventData eventData)
    {
        if (_myData != null && _controller != null)
        {
            _controller.ToggleEquipFunction(_myData);
        }
    }
}