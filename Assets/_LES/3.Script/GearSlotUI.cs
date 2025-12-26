using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GearSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private Image gearIcon;
    [SerializeField] private Image slotBackground; // 여기에 배경 이미지(BG_0) 연결 필수
    [SerializeField] private GameObject newIndicator;

    [Header("배경 색상 설정 (아이템 유무)")]
    [Tooltip("아이템이 들어있을 때의 배경색")]
    [SerializeField] private Color hasItemBgColor = Color.white; 
    
    [Tooltip("비어있을 때의 배경색")]
    [SerializeField] private Color emptyBgColor = new Color(0.3f, 0.3f, 0.3f, 1f); 

    [Header("비장착 상태 밝기 (아이콘)")]
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
        _button?.onClick.AddListener(HandleInteraction);
    }
    
    public void SetData(GearData data, GearPanelController controller)
    {
        _myData = data;
        _controller = controller;

        // 1. 아이콘 설정
        if (gearIcon != null) 
        { 
            gearIcon.sprite = _myData.gearIcon; 
            gearIcon.gameObject.SetActive(true); 
        }

        // 2. [추가] 배경색 변경 (아이템 있음)
        if (slotBackground != null)
        {
            slotBackground.color = hasItemBgColor;
        }

        if (_button != null) _button.interactable = true;
        
        UpdateEquipVisual(); // 아이콘 밝기 조절 (장착/비장착)

        if (newIndicator != null) newIndicator.SetActive(_myData.isNew);
    }

    public void ClearSlot()
    {
        _myData = null;
        _controller = null;

        // 1. 아이콘 숨기기
        if (gearIcon != null) 
        { 
            gearIcon.sprite = null; 
            gearIcon.gameObject.SetActive(false); 
        }

        // 2. [추가] 배경색 변경 (비어있음)
        if (slotBackground != null)
        {
            slotBackground.color = emptyBgColor;
        }

        if (_button != null) _button.interactable = false;
        
        // 비어있을 때 아이콘 색상 초기화 (안전장치)
        if (gearIcon != null) gearIcon.color = _originalIconColor;
        
        if (newIndicator != null) newIndicator.SetActive(false);
    }

    // 아이콘 밝기 조절 (원상복구 된 버전)
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

    private void HandleInteraction()
    {
        if (_myData != null && _controller != null)
        {
            if (_myData.isNew)
            {
                _myData.isNew = false;
                if (newIndicator != null) newIndicator.SetActive(false);

                int find = DBManager.I.currData.gearDatas.FindIndex(x => x.Name == _myData.name);
                if(find != -1)
                {
                    CharacterData.GearData cd = DBManager.I.currData.gearDatas[find];
                    cd.isNew = false;
                    DBManager.I.currData.gearDatas[find] = cd;
                }
            }
            _controller.ToggleEquipGear(_myData);
        }
    }
}