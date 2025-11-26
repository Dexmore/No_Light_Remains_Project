using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // ISelectHandler와 IPointerEnterHandler가 모두 여기 있습니다.

// [수정] IPointerEnterHandler 인터페이스를 추가합니다.
public class ItemSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    private ItemData _currentItem;
    private ItemPanelController _controller;

    [Header("슬롯 UI 요소")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private GameObject newIndicator;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button?.onClick.AddListener(OnSlotClicked);
    }

    // --- (SetItem, ClearSlot 함수는 기존과 동일합니다) ---
    public void SetItem(ItemData itemData, ItemPanelController controller)
    {
        _currentItem = itemData;
        _controller = controller;

        if (_currentItem == null) { ClearSlot(); return; }

        if (itemIcon != null)
        {
            itemIcon.sprite = _currentItem.icon;
            itemIcon.gameObject.SetActive(_currentItem.icon != null);
        }
        itemNameText.text = _currentItem.itemName;
        itemNameText.gameObject.SetActive(true);
        if (newIndicator != null) newIndicator.SetActive(_currentItem.isNew);
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
        // 정보 표시 함수를 호출합니다.
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
            if (_currentItem.isNew)
            {
                _currentItem.isNew = false;
                int find = DBManager.I.cashingItems.FindIndex(x => x.itemName == _currentItem.itemName);
                if(find != -1)
                {
                    DBManager.I.cashingItems[find].isNew = false;
                }
                find = DBManager.I.currentCharData.itemDatas.FindIndex(x => x.Name == _currentItem.name);
                if(find != -1)
                {
                    CharacterData.ItemData citd = DBManager.I.currentCharData.itemDatas[find];
                    citd.isNew = false;
                    DBManager.I.currentCharData.itemDatas[find] = citd;
                }
                if (newIndicator != null) newIndicator.SetActive(false);
            }
        }
    }
}