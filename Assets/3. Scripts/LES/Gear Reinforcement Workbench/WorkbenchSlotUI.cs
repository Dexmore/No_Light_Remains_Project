using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WorkbenchSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("UI 요소")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject selectedBorder; 

    private GearData _myData;
    private WorkbenchUI _parentUI;
    private Button _button;

    public GearData Data => _myData; 

    private void Awake()
    {
        _button = GetComponent<Button>();
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

    public void SetSelectedState(bool isSelected)
    {
        if (selectedBorder != null) selectedBorder.SetActive(isSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button.interactable)
        {
            // 호버 소리는 제거됨 (타이핑 소리로 대체)
            _button.Select();
            _parentUI.ShowPreview(_myData); 
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        _parentUI.ShowPreview(_myData); 
    }

    private void OnClickSlot()
    {
        if (_myData != null)
        {
            // [변경] 부모 UI의 AudioSource를 통해 클릭음 재생
            _parentUI.PlayClickSound(); 
            _parentUI.ConfirmSelection(this); 
        }
    }
}