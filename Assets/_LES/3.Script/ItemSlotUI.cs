using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // ISelectHandler와 IPointerEnterHandler가 모두 여기 있습니다.

// [수정] IPointerEnterHandler 인터페이스를 추가합니다.
public class ItemSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    private InventoryItem _currentItem;
    private ItemPanelController _controller;

    [Header("슬롯 UI 요소")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private GameObject newIndicator;
    [SerializeField] private TextMeshProUGUI quantityText;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button?.onClick.AddListener(OnSlotClicked);
    }

    // --- (SetItem, ClearSlot 함수는 기존과 동일합니다) ---
    public void SetItem(InventoryItem item, ItemPanelController controller)
    {
        _currentItem = item;
        _controller = controller;

        if (_currentItem == null || _currentItem.data == null)
        {
            ClearSlot();
            return; 
        }

        if (itemIcon != null)
        {
            itemIcon.sprite = _currentItem.data.icon;
            itemIcon.gameObject.SetActive(_currentItem.data.icon != null);
        }
        itemNameText.text = _currentItem.data.name;
        itemNameText.gameObject.SetActive(true);

        // [추가] 수량 표시 로직
        if (quantityText != null)
        {
            // 1개보다 많을 때만 "x5" 처럼 표시, 1개면 숨김
            if (_currentItem.quantity > 1)
            {
                quantityText.text = $"x{_currentItem.quantity}";
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }

        if (newIndicator != null) newIndicator.SetActive(_currentItem.data.isNew);
        if (_button != null) _button.interactable = true;
    }

    public void ClearSlot()
    {
        _currentItem = null;
        _controller = null;
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.gameObject.SetActive(false);
        }
        itemNameText.text = "";
        itemNameText.gameObject.SetActive(false);
        if (newIndicator != null) newIndicator.SetActive(false);
        if (_button != null) _button.interactable = false;
    }
    // --- (여기까지 동일) ---


    /// <summary>
    /// [추가] 2번 요청 - 슬롯에 '마우스가 진입'했을 때 호출됩니다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button != null && _button.interactable)
        {
            // [핵심] 마우스를 올리면 이 버튼을 '선택' 상태로 만듭니다.
            // 이렇게 하면 UIFocusManager가 감지하고 셀렉터를 이쪽으로 이동시킵니다.
            _button.Select();
        }

        // 정보 표시
        ShowDetails();
    }

    /// <summary>
    /// 1번, 2번 - 슬롯이 '선택'될 때 (방향키/클릭) 호출됩니다.
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        // 정보 표시 함수를 호출합니다.
        ShowDetails();
    }

    /// <summary>
    /// [추가] 중복되는 정보 표시 로직을 하나로 묶습니다.
    /// </summary>
    private void ShowDetails()
    {
        if (_currentItem != null && _controller != null)
        {
            _controller.ShowItemDetails(_currentItem);
        }
    }

    /// <summary>
    /// 슬롯이 '클릭'될 때 (Enter/Space/마우스 클릭) 호출됩니다.
    /// </summary>
    public void OnSlotClicked()
    {
        // 정보 표시는 OnSelect/OnPointerEnter가 처리하므로, 여기서는 'New' 마크 제거만 합니다.
        if (_currentItem != null && _controller != null)
        {
            if (_currentItem.data.isNew)
            {
                _currentItem.data.isNew = false;
                if (newIndicator != null) newIndicator.SetActive(false);

                /////
                int find = DBManager.I.itemDatabase.allItems.FindIndex(x => x.itemName == _currentItem.data.itemName);
                if(find != -1)
                {
                    CharacterData.ItemData citd = DBManager.I.currData.itemDatas[find];
                    citd.isNew = false;
                    DBManager.I.currData.itemDatas[find] = citd;
                }
                /////

            }
        }
    }
}