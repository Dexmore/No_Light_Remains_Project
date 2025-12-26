using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LanternSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private Image functionIcon;
    [SerializeField] private Image slotBackground; // [추가] 배경 이미지 연결 필요
    [SerializeField] private GameObject newIndicator;

    [Header("배경 색상 설정 (아이템 유무)")]
    [Tooltip("아이템이 들어있을 때의 배경색")]
    [SerializeField] private Color hasItemBgColor = Color.white; 
    
    [Tooltip("비어있을 때의 배경색")]
    [SerializeField] private Color emptyBgColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("비장착 상태 밝기")]
    [Range(0f, 1f)]
    [SerializeField] private float dimFactor = 0.5f;

    private LanternFunctionData _myData;
    private LanternPanelController _controller;
    private Button _button;
    private Color _originalIconColor;

    public LanternFunctionData MyData { get { return _myData; } }

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (functionIcon != null) _originalIconColor = functionIcon.color;
        _button?.onClick.AddListener(HandleInteraction);
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

        // [추가] 배경색 변경 (아이템 있음)
        if (slotBackground != null)
        {
            slotBackground.color = hasItemBgColor;
        }

        if (_button != null) _button.interactable = true;
        
        UpdateEquipVisual();
        
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

        // [추가] 배경색 변경 (비어있음)
        if (slotBackground != null)
        {
            slotBackground.color = emptyBgColor;
        }

        if (_button != null) _button.interactable = false;
        if (newIndicator != null) newIndicator.SetActive(false);
    }

    public void UpdateEquipVisual()
    {
        if (_myData == null || functionIcon == null) return;
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
        if (_myData != null && _controller != null)
        {
            _controller.ShowFunctionDetails(_myData);
        }
    }

    private void HandleInteraction()
    {
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