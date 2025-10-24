using UnityEngine;
using UnityEngine.UI;
using TMPro;

//아이템 데이터를 담을 클래스입니다. 
//실제 프로젝트의 아이템 데이터 클래스로 대체해야 합니다.
public class ItemData 
{
    public string itemName;
    public Sprite icon;
    public enum ItemType { Equipment, Material }
    public ItemType type;
    
    //아이템 상세 내용
    [UnityEngine.TextArea(3, 10)]
    public string itemDescription; 
    
    //새로 얻은 아이템인지 여부
    public bool isNew; 

    //생성자 수정
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
    private ItemPanelController _controller; //부모 컨트롤러 참조

    [Header("슬롯 UI 요소")]
    [Tooltip("아이템 이름을 표시할 TextMeshPro UI")]
    [SerializeField]
    private TextMeshProUGUI itemNameText; //Image 대신 TextMeshPro 사용
    [SerializeField] private GameObject newIndicator; //"New" 알림 아이콘

    //슬롯의 배경 이미지
    [SerializeField]
    private Image slotBackground;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        //OnSlotClicked 함수를 버튼 클릭 이벤트에 연결
        _button?.onClick.AddListener(OnSlotClicked);
        
        ClearSlot(); // 처음엔 비어있는 상태로 시작
    }

    //이 슬롯에 특정 아이템 데이터를 할당하고 UI를 갱신합니다.
    public void SetItem(ItemData itemData, ItemPanelController controller)
    {
        _currentItem = itemData;
        _controller = controller; //컨트롤러 저장

        if (_currentItem == null)
        {
            ClearSlot();
            return;
        }

        //데이터가 있으면 슬롯을 채웁니다.
        itemNameText.text = _currentItem.itemName; // 텍스트 설정
        itemNameText.gameObject.SetActive(true);

        //'New' 표시 업데이트
        if (newIndicator != null)
        {
            newIndicator.SetActive(_currentItem.isNew);
        }
        
        if (_button != null) _button.interactable = true;
    }

    //이 슬롯을 빈 상태로 만듭니다.
    public void ClearSlot()
    {
        _currentItem = null;
        _controller = null;
        itemNameText.text = "";//텍스트 비우기
        itemNameText.gameObject.SetActive(false);//텍스트 숨김

        if (newIndicator != null) newIndicator.SetActive(false);
        if (_button != null) _button.interactable = false;
    }

    //이 슬롯이 클릭되었을 때의 동작
    public void OnSlotClicked()
    {
        if (_currentItem != null && _controller != null)
        {
            //부모 컨트롤러에게 상세 정보 표시 요청
            _controller.ShowItemDetails(_currentItem);
            
            //클릭 시 'New' 표시 제거
            if (_currentItem.isNew)
            {
                _currentItem.isNew = false;
                if (newIndicator != null) newIndicator.SetActive(false);
            }
        }
    }

    //EventSystem이 이 슬롯을 선택(포커스)하도록 합니다.
    public void Select()
    {
        if (_button != null)
        {
            _button.Select();
        }
    }
}