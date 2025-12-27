using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // ISelectHandler와 IPointerEnterHandler가 모두 여기 있습니다.
using UnityEngine.Localization.Settings;

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

    private void OnDisable()
    {
        UnsubscribeFromData();
    }

    // --- (SetItem, ClearSlot 함수는 기존과 동일합니다) ---
    public void SetItem(InventoryItem item, ItemPanelController controller)
    {
        // [추가] 기존에 구독 중이던 데이터가 있다면 해제
        UnsubscribeFromData();

        _currentItem = item;
        _controller = controller;

        if (_currentItem == null || _currentItem.data == null)
        {
            ClearSlot();
            return; 
        }

        // [추가] 데이터 로드 완료 이벤트 구독
        _currentItem.data.OnDataLoaded += UpdateNameText;

        if (itemIcon != null)
        {
            itemIcon.sprite = _currentItem.data.icon;
            itemIcon.gameObject.SetActive(_currentItem.data.icon != null);
        }

        // [수정] 직접 텍스트를 넣는 대신, 공통 함수 호출
        UpdateNameText();

        itemNameText.text = _currentItem.data.itemName.GetLocalizedString();
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

    // [추가] 텍스트를 갱신하는 공통 함수
    private void UpdateNameText()
    {
        if (_currentItem != null && _currentItem.data != null)
        {
            // 데이터 에셋에 캐싱된 번역 이름을 가져와서 적용
            itemNameText.text = _currentItem.data.localizedName;
        }
    }

    // [추가] 이벤트 구독 해제 함수 (메모리 누수 방지)
    private void UnsubscribeFromData()
    {
        if (_currentItem != null && _currentItem.data != null)
        {
            _currentItem.data.OnDataLoaded -= UpdateNameText;
        }
    }
    public void ClearSlot()
    {
        UnsubscribeFromData(); // [추가]
        
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
    
    // [추가] 2번 요청 - 슬롯에 '마우스가 진입'했을 때 호출됩니다.
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

    // 1번, 2번 - 슬롯이 '선택'될 때 (방향키/클릭) 호출됩니다.
    public void OnSelect(BaseEventData eventData)
    {
        // [소리] 커서 이동음
        AudioManager.I?.PlaySFX("InventoryUI_button1");
        ShowDetails();
    }

    // [추가] 중복되는 정보 표시 로직을 하나로 묶습니다.
    private void ShowDetails()
    {
        if (_currentItem != null && _controller != null)
        {
            _controller.ShowItemDetails(_currentItem);
        }
    }

    // 슬롯이 '클릭'될 때 (Enter/Space/마우스 클릭) 호출됩니다.
    public void OnSlotClicked()
    {
        // [소리] 버튼 클릭음
        AudioManager.I?.PlaySFX("InventoryUI_button1");

        if (_currentItem != null && _controller != null)
        {
            if (_currentItem.data.isNew)
            {
                _currentItem.data.isNew = false;
                if (newIndicator != null) newIndicator.SetActive(false);

                int find = DBManager.I.currData.itemDatas.FindIndex(x => x.Name == _currentItem.data.name);
                if(find != -1)
                {
                    CharacterData.ItemData cd = DBManager.I.currData.itemDatas[find];
                    cd.isNew = false;
                    DBManager.I.currData.itemDatas[find] = cd;
                }
            }
        }
    }
}