using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LanternSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private Image functionIcon;
    [SerializeField] private GameObject newIndicator;

    [Header("비장착 상태 밝기 (플라즈마에선 사용 안 함)")]
    [Range(0f, 1f)]
    [SerializeField] private float dimFactor = 0.5f;

    private LanternFunctionData _myData;
    private LanternPanelController _controller;
    private Button _button;
    
    // [추가] 내 슬롯에 붙어있는 플라즈마 제어기
    private PlasmaInteract _myPlasma;

    public LanternFunctionData MyData { get { return _myData; } }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button?.onClick.AddListener(HandleInteraction);
        
        // [추가] 아이콘에 붙어있는 PlasmaInteract 찾기
        if (functionIcon != null)
        {
            _myPlasma = functionIcon.GetComponent<PlasmaInteract>();
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
            
            // 쉐이더를 쓰므로 색상은 항상 흰색(원본 밝기)으로 둡니다.
            functionIcon.color = Color.white;

            // [핵심] 데이터에 있는 색상을 내 플라즈마 쉐이더에 주입!
            if (_myPlasma != null)
            {
                _myPlasma.SetThemeColor(_myData.coreColor, _myData.glowColor);
            }
        }
        
        if (_button != null) _button.interactable = true;
        
        // UpdateEquipVisual(); // 플라즈마 쉐이더를 쓰면 밝기 조절 방식이 달라지므로 일단 주석 처리해도 됩니다.
        
        if (newIndicator != null) newIndicator.SetActive(_myData.isNew);
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
        if (newIndicator != null) newIndicator.SetActive(false);
    }

    public void UpdateEquipVisual()
    {
        // 플라즈마 쉐이더를 사용하는 경우, 
        // 장착/미장착 구분을 위해 'GlowColor'의 강도를 조절하거나 테두리를 띄우는 것이 좋습니다.
        // 현재는 색상 변경 로직과 충돌할 수 있으므로 기본 로직은 패스하거나,
        // 필요하다면 여기서 _myPlasma.SetThemeColor(...)를 다시 호출하여 색을 흐리게 만들 수 있습니다.
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_myData != null && _controller != null)
        {
            _controller.ShowFunctionDetails(_myData);
            if (_button.interactable) _button.Select();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.I?.PlaySFX("InventoryUI_button1");
        if (_myData != null && _controller != null)
        {
            _controller.ShowFunctionDetails(_myData);
        }
    }

    private void HandleInteraction()
    {
        AudioManager.I?.PlaySFX("InventoryUI_button1");
        if (_myData == null || _controller == null) return;

        if (_myData.isNew)
        {
            _myData.isNew = false;
            if (newIndicator != null) newIndicator.SetActive(false);

            int find = DBManager.I.currData.lanternDatas.FindIndex(x => x.Name == _myData.name);
            if (find != -1)
            {
                CharacterData.LanternData cd = DBManager.I.currData.lanternDatas[find];
                cd.isNew = false;
                DBManager.I.currData.lanternDatas[find] = cd;
            }
        }

        _controller.ToggleEquipFunction(_myData);
    }
}