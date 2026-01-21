using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LanternSlotUI : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private Image functionIcon;
    [SerializeField] private GameObject newIndicator;
    [Header("선택 효과")]
    [SerializeField] private GameObject selectionOutline;

    [Header("비장착 상태 밝기")]
    [Range(0f, 1f)]
    [SerializeField] private float dimFactor = 0.5f;

    private LanternFunctionData _myData;
    private LanternPanelController _controller;
    private Button _button;
    private PlasmaInteract _myPlasma;
    
    // 현재 포커스 여부
    private bool _isFocused = false;

    public LanternFunctionData MyData { get { return _myData; } }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button?.onClick.AddListener(HandleInteraction);
        
        if (functionIcon != null)
        {
            _myPlasma = functionIcon.GetComponent<PlasmaInteract>();
        }
        
        // 시작 시 아웃라인 끄기
        if (selectionOutline != null) selectionOutline.SetActive(false);
    }

    // 색상 업데이트 함수 (포커스 여부에 따라 밝기 조절)
    private void UpdateVisualState()
    {
        if (functionIcon == null || _myData == null) return;

        Color targetColor = Color.white;

        if (_isFocused)
        {
            // 1. 포커스 상태: 무조건 밝게 + 아웃라인 켜기
            targetColor = Color.white;
            if (selectionOutline != null) selectionOutline.SetActive(true);
        }
        else
        {
            // 2. 포커스 아님 (탭으로 이동했거나 다른 슬롯 선택): 어둡게 + 아웃라인 끄기
            // (장착 여부와 상관없이, 선택 안됐으면 어둡게 해서 위치 구분)
            targetColor = Color.white * dimFactor;
            
            // 알파값은 유지 (투명해지지 않게)
            targetColor.a = 1f; 

            if (selectionOutline != null) selectionOutline.SetActive(false);
        }

        // 플라즈마 쉐이더가 있으면 색상 전달, 없으면 이미지 색상 변경
        if (_myPlasma != null)
        {
             // 쉐이더의 경우 Core/Glow 색상을 어둡게 조절해서 전달
             Color core = _myData.coreColor * (_isFocused ? 1f : dimFactor);
             Color glow = _myData.glowColor * (_isFocused ? 1f : dimFactor);
             // 알파값 보정
             core.a = 1f; glow.a = 1f;
             
             _myPlasma.SetThemeColor(core, glow);
        }
        else
        {
            functionIcon.color = targetColor;
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
            
            // 초기 색상 설정
            if (_myPlasma != null) _myPlasma.SetThemeColor(_myData.coreColor, _myData.glowColor);
            else functionIcon.color = Color.white;
        }
        
        if (_button != null) _button.interactable = true;
        
        // 데이터 세팅 시 포커스 초기화
        _isFocused = (EventSystem.current.currentSelectedGameObject == gameObject);
        UpdateVisualState();

        if (newIndicator != null) newIndicator.SetActive(_myData.isNew);
    }

    public void ClearSlot()
    {
        _myData = null;
        _controller = null;
        _isFocused = false;
        
        if (functionIcon != null) { functionIcon.sprite = null; functionIcon.gameObject.SetActive(false); }
        if (_button != null) _button.interactable = false;
        if (newIndicator != null) newIndicator.SetActive(false);
        if (selectionOutline != null) selectionOutline.SetActive(false);
    }
    
    // 외부에서 장착 상태 갱신 시 호출
    public void UpdateEquipVisual()
    {
        // 여기선 포커스 상태가 바뀌지 않으므로 색상만 재계산
        UpdateVisualState();
    }

    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.I?.PlaySFX("InventoryUI_button1");
        _isFocused = true;
        UpdateVisualState(); // 밝아짐 + 아웃라인 ON

        if (_myData != null && _controller != null)
        {
            _controller.ShowFunctionDetails(_myData);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _isFocused = false;
        UpdateVisualState(); // 어두워짐 + 아웃라인 OFF
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_myData != null && _controller != null && _button.interactable)
        {
            _button.Select(); // OnSelect 호출됨
        }
    }

    private void HandleInteraction()
    {
        AudioManager.I?.PlaySFX("InventoryUI_button1");
        if (_myData == null || _controller == null) return;
        
        // New 상태 해제 로직 등 기존 유지...
        if (_myData.isNew) { _myData.isNew = false; if (newIndicator != null) newIndicator.SetActive(false); }
        
        _controller.ToggleEquipFunction(_myData);
    }
}