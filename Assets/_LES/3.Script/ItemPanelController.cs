using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ItemPanelController : MonoBehaviour, ITabContent
{
    [Header("UI 내비게이션 설정")]
    [SerializeField] private Selectable firstSelectable;

    [Header("서브-탭 (필터) 버튼")]
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button materialButton;
    [SerializeField] private Color subTabActiveColor = Color.white;
    [SerializeField] private Color subTabIdleColor = Color.gray;

    [Header("아이템 슬롯 관리")]
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform contentTransform;

    [Header("아이템 정보(Info) 창")]
    [SerializeField] private TextMeshProUGUI detailItemNameText;
    [SerializeField] private Image detailItemImage;
    [SerializeField] private TextMeshProUGUI detailItemDescriptionText;
    [SerializeField] private GameObject infoPanelRoot;

    // [수정] 인스펙터에서 직접 데이터를 관리하도록 변경
    [Header("플레이어 인벤토리 (데이터)")]
    [SerializeField] 
    private List<ItemData> _playerInventory;

    // 현재 생성된 슬롯 UI들을 관리하는 리스트
    private List<ItemSlotUI> _spawnedSlots = new List<ItemSlotUI>();

    private ItemData.ItemType _currentFilter = ItemData.ItemType.Equipment;

    private void Awake()
    {
        // 서브-탭 버튼에 클릭 리스너 연결
        equipmentButton?.onClick.AddListener(() => OnFilterChanged(ItemData.ItemType.Equipment));
        materialButton?.onClick.AddListener(() => OnFilterChanged(ItemData.ItemType.Material));
        
        // [삭제] CreateTestData();
        
        // 처음에는 정보창을 비워둠
        ShowItemDetails(null);
    }

    public void OnShow()
    {
        // 탭이 열릴 때마다 기본 필터('장비')로 목록을 새로고침
        OnFilterChanged(ItemData.ItemType.Equipment);
        
        // [수정] EventSystem이 UI를 인지할 수 있도록 한 프레임 뒤에 포커스를 설정합니다.
        StartCoroutine(SetInitialFocus());
    }

    public void OnHide()
    {
        if (EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.transform.IsChildOf(this.transform))
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        ClearAllSpawnedSlots();
    }
    
    private void OnFilterChanged(ItemData.ItemType newFilter)
    {
        _currentFilter = newFilter;
        UpdateSubTabButtons();
        UpdateInventoryList();
    }
    
    private void UpdateInventoryList()
    {
        ClearAllSpawnedSlots();

        // [수정] 인스펙터에서 받아온 _playerInventory를 사용
        List<ItemData> filteredList = _playerInventory
            .Where(item => item != null && item.type == _currentFilter) // null 체크 추가
            .ToList();

        if (filteredList.Count == 0)
        {
            ShowItemDetails(null);
            return;
        }
        
        for (int i = 0; i < filteredList.Count; i++)
        {
            GameObject slotGO = Instantiate(itemSlotPrefab, contentTransform);
            ItemSlotUI slotUI = slotGO.GetComponent<ItemSlotUI>();
            slotUI.SetItem(filteredList[i], this);
            _spawnedSlots.Add(slotUI);
        }
        
        SetupSlotNavigation();

        // 목록 갱신 시, 첫 번째 아이템 정보를 자동으로 표시
        ShowItemDetails(filteredList[0]);
    }
    
    /// <summary>
    /// UI가 활성화된 다음 프레임에 첫 번째 요소에 포커스를 설정합니다.
    /// </summary>
    private IEnumerator SetInitialFocus()
    {
        // EventSystem이 UI 활성화를 완전히 인지할 수 있도록 한 프레임 대기
        yield return null; 

        if (firstSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
        }
    }
    
    // --- (ShowItemDetails, ClearAllSpawnedSlots, SetupSlotNavigation, UpdateSubTabButtons 함수는 기존과 동일) ---
    #region (수정 없는 함수들)
    public void ShowItemDetails(ItemData data)
    {
        if (data != null)
        {
            if (infoPanelRoot != null) infoPanelRoot.SetActive(true);
            detailItemNameText.text = data.itemName;
            detailItemImage.sprite = data.icon; // 이제 null이 아닌 실제 스프라이트가 들어옵니다.
            detailItemDescriptionText.text = data.itemDescription;
            detailItemImage.gameObject.SetActive(data.icon != null);
        }
        else
        {
            if (infoPanelRoot != null) infoPanelRoot.SetActive(false);
            detailItemNameText.text = "아이템 선택";
            detailItemImage.sprite = null;
            detailItemImage.gameObject.SetActive(false);
            detailItemDescriptionText.text = "목록에서 아이템을 선택하세요.";
        }
    }
    
    private void ClearAllSpawnedSlots()
    {
        foreach (ItemSlotUI slot in _spawnedSlots) Destroy(slot.gameObject);
        _spawnedSlots.Clear();
    }

    private void SetupSlotNavigation()
    {
        if (_spawnedSlots.Count == 0) return;
        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            Button button = _spawnedSlots[i].GetComponent<Button>();
            if (button == null) continue; // null 체크
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = (i == 0) ? equipmentButton : _spawnedSlots[i - 1]?.GetComponent<Button>();
            nav.selectOnDown = (i == _spawnedSlots.Count - 1) ? button : _spawnedSlots[i + 1]?.GetComponent<Button>();
            nav.selectOnLeft = null;
            nav.selectOnRight = null;
            button.navigation = nav;
        }
        Navigation eqNav = equipmentButton.navigation;
        eqNav.selectOnDown = _spawnedSlots.Count > 0 ? _spawnedSlots[0].GetComponent<Button>() : null;
        equipmentButton.navigation = eqNav;
        Navigation matNav = materialButton.navigation;
        matNav.selectOnDown = _spawnedSlots.Count > 0 ? _spawnedSlots[0].GetComponent<Button>() : null;
        materialButton.navigation = matNav;
    }

    private void UpdateSubTabButtons()
    {
        equipmentButton.GetComponentInChildren<TextMeshProUGUI>().color = ExampleColor(_currentFilter == ItemData.ItemType.Equipment);
        materialButton.GetComponentInChildren<TextMeshProUGUI>().color = ExampleColor(_currentFilter == ItemData.ItemType.Material);
    }
    
    private Color ExampleColor(bool isActive) => isActive ? subTabActiveColor : subTabIdleColor;
    #endregion
}