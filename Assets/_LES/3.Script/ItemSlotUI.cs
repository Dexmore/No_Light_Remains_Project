using UnityEngine;
using UnityEngine.UI; // Image, Text 등을 사용하기 위해
using TMPro;

// [가정] 아이템 데이터를 담을 클래스입니다. 
// 실제 프로젝트의 아이템 데이터 클래스로 대체해야 합니다.
public class ItemData 
{
    public string itemName;
    public Sprite icon;
    public enum ItemType { Equipment, Material }
    public ItemType type;
    
    // [추가] 아이템 상세 내용
    [UnityEngine.TextArea(3, 10)]
    public string itemDescription; 
    
    // [추가] 새로 얻은 아이템인지 여부
    public bool isNew; 

    // 생성자 수정
    public ItemData(string name, Sprite sprite, ItemType type, string description, bool isNew = true)
    {
        this.itemName = name;
        this.icon = sprite;
        this.type = type;
        this.itemDescription = description;
        this.isNew = isNew;
    }
}

public class ItemSlotUI : MonoBehaviour
{
    private ItemData _currentItem;
    private ItemPanelController _controller; // [추가] 부모 컨트롤러 참조

    [Header("슬롯 UI 요소")]
    [Tooltip("아이템 이름을 표시할 TextMeshPro UI")]
    [SerializeField]
    private TextMeshProUGUI itemNameText; // Image 대신 TextMeshPro 사용
    [SerializeField] private GameObject newIndicator; // [추가] "New" 알림 아이콘

    // (선택 사항) 슬롯의 배경 이미지
    [SerializeField]
    private Image slotBackground;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        // OnSlotClicked 함수를 버튼 클릭 이벤트에 연결
        _button?.onClick.AddListener(OnSlotClicked);
        
        ClearSlot(); // 처음엔 비어있는 상태로 시작
    }

    /// <summary>
    /// 이 슬롯에 특정 아이템 데이터를 할당하고 UI를 갱신합니다.
    /// </summary>
    public void SetItem(ItemData itemData, ItemPanelController controller)
    {
        _currentItem = itemData;
        _controller = controller; // [추가] 컨트롤러 저장

        if (_currentItem == null)
        {
            ClearSlot();
            return;
        }

        // 데이터가 있으면 슬롯을 채웁니다.
        itemNameText.text = _currentItem.itemName; // 텍스트 설정
        itemNameText.gameObject.SetActive(true);

        // [추가] 'New' 표시 업데이트
        if (newIndicator != null)
        {
            newIndicator.SetActive(_currentItem.isNew);
        }
        
        if (_button != null) _button.interactable = true;
    }

    /// <summary>
    /// 이 슬롯을 빈 상태로 만듭니다.
    /// </summary>
    public void ClearSlot()
    {
        _currentItem = null;
        _controller = null;
        itemNameText.text = ""; // 텍스트 비우기
        itemNameText.gameObject.SetActive(false); // 텍스트 숨김

        if (newIndicator != null) newIndicator.SetActive(false);
        if (_button != null) _button.interactable = false;
    }

    /// <summary>
    /// 이 슬롯이 클릭되었을 때의 동작
    /// </summary>
    public void OnSlotClicked()
    {
        if (_currentItem != null && _controller != null)
        {
            // [수정] 부모 컨트롤러에게 상세 정보 표시 요청
            _controller.ShowItemDetails(_currentItem);
            
            // [추가] 클릭 시 'New' 표시 제거
            if (_currentItem.isNew)
            {
                _currentItem.isNew = false;
                if (newIndicator != null) newIndicator.SetActive(false);
            }
        }
    }

    /// <summary>
    /// EventSystem이 이 슬롯을 선택(포커스)하도록 합니다.
    /// </summary>
    public void Select()
    {
        if (_button != null)
        {
            _button.Select();
        }
    }
}