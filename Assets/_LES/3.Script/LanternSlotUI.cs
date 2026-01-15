using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// [수정] ISubmitHandler 제거
public class LanternSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private Image functionIcon;
    [SerializeField] private GameObject newIndicator;

    [Header("비장착 상태 밝기")]
    [Range(0f, 1f)]
    [SerializeField] private float dimFactor = 0.5f;

    private LanternFunctionData _myData;
    private LanternPanelController _controller;
    private Button _button;
    
    // [수정] 원본 색상을 Color.white로 고정 (투명도 문제 방지)
    private Color _originalIconColor = Color.white;

    public LanternFunctionData MyData { get { return _myData; } }

    private void Awake()
    {
        _button = GetComponent<Button>();
        // Awake에서 색상을 가져오지 않고, 그냥 흰색(보이는 색)을 기준으로 잡습니다.
        // if (functionIcon != null) _originalIconColor = functionIcon.color; 

        // Button의 onClick이 엔터 키와 클릭을 모두 처리함
        _button?.onClick.AddListener(HandleInteraction);
    }

    // ... (SetData, ClearSlot, UpdateEquipVisual 함수는 기존 그대로 유지) ...
    public void SetData(LanternFunctionData data, LanternPanelController controller)
    {
        _myData = data;
        _controller = controller;
        
        if (functionIcon != null) 
        { 
            functionIcon.sprite = _myData.functionIcon; 
            
            // [핵심 수정] 데이터를 세팅할 때 무조건 '흰색(완전 불투명)'으로 초기화합니다.
            // 기존에 투명했던 설정이 남아있지 않도록 합니다.
            functionIcon.color = Color.white; 
            
            functionIcon.gameObject.SetActive(true); 
        }
        
        if (_button != null) _button.interactable = true;
        
        UpdateEquipVisual(); // 여기서 장착 여부에 따라 어둡게/밝게 조절됨
        
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
        if (_myData == null || functionIcon == null) return;
        
        // 장착되었으면 밝게(흰색), 아니면 어둡게(흰색 * 0.5)
        functionIcon.color = _myData.isEquipped ? _originalIconColor : (_originalIconColor * dimFactor);
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

            /////
            int find = DBManager.I.currData.lanternDatas.FindIndex(x => x.Name == _myData.name);
            if (find != -1)
            {
                CharacterData.LanternData cd = DBManager.I.currData.lanternDatas[find];
                cd.isNew = false;
                DBManager.I.currData.lanternDatas[find] = cd;
            }
            /////
        }

        _controller.ToggleEquipFunction(_myData);
    }
}