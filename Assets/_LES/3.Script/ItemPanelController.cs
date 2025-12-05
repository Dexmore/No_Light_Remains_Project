using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using NaughtyAttributes;

public class ItemPanelController : MonoBehaviour, ITabContent
{
    [Header("UI 내비게이션 설정")]
    [SerializeField] private Selectable firstSelectable;
    [SerializeField] private Selectable mainTabButton;

    [Header("스크롤 제어")]
    [SerializeField] private AutoScroll autoScroll; // [추가] 변수 선언 누락 수정
    [SerializeField] private ScrollRect scrollRect; // [추가] ScrollRect 변수 선언

    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI moneyText;

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

    private List<ItemSlotUI> _spawnedSlots = new List<ItemSlotUI>();
    private ItemData.ItemType _currentFilter = ItemData.ItemType.Equipment;
    private Coroutine _initCoroutine;

    private void OnEnable()
    {
        equipmentButton?.onClick.AddListener(() => OnFilterChanged(ItemData.ItemType.Equipment));
        materialButton?.onClick.AddListener(() => OnFilterChanged(ItemData.ItemType.Material));

        if (InventoryDataManager.Instance != null)
        {
            InventoryDataManager.Instance.OnInventoryChanged += RefreshUIFromEvent;
        }
        UpdateMoneyText();
    }

    private void OnDisable()
    {
        equipmentButton?.onClick.RemoveAllListeners();
        materialButton?.onClick.RemoveAllListeners();

        if (InventoryDataManager.Instance != null)
        {
            InventoryDataManager.Instance.OnInventoryChanged -= RefreshUIFromEvent;
        }
    }

    // [추가] Update 함수 추가 (상단 탭 이동 제어용)
    private void Update()
    {
        if (_spawnedSlots.Count == 0 || scrollRect == null) return;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (_spawnedSlots[0] == null) return;

        Button firstSlotBtn = _spawnedSlots[0].GetComponent<Button>();

        if (currentSelected == firstSlotBtn.gameObject)
        {
            Navigation nav = firstSlotBtn.navigation;

            bool isAtTop = false;

            // 1. AutoScroll이 있고 맨 위에 도착했으면 true
            if (autoScroll != null && autoScroll.IsScrolledToTop)
            {
                isAtTop = true;
            }
            // 2. 내용이 적어서 스크롤이 필요 없으면 true
            else if (contentTransform.GetComponent<RectTransform>().rect.height <= scrollRect.viewport.rect.height)
            {
                isAtTop = true;
            }

            if (isAtTop)
            {
                if (_currentFilter == ItemData.ItemType.Equipment) nav.selectOnUp = equipmentButton;
                else nav.selectOnUp = materialButton;
            }
            else
            {
                nav.selectOnUp = null; // 아직 올라가는 중이면 위쪽 막음
            }
            
            firstSlotBtn.navigation = nav;
        }
    }

    private void RefreshUIFromEvent()
    {
        StartMasterCoroutine(_currentFilter, false); 
        UpdateMoneyText();
    }

    public void OnShow()
    {
        StartMasterCoroutine(ItemData.ItemType.Equipment, true);
        UpdateMoneyText();
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
        StartMasterCoroutine(newFilter, false);
    }
    
    private void StartMasterCoroutine(ItemData.ItemType filter, bool isInitialLoad)
    {
        if (_initCoroutine != null)
        {
            StopCoroutine(_initCoroutine);
        }
        _initCoroutine = StartCoroutine(InitializePanelCoroutine(filter, isInitialLoad));
    }

    private IEnumerator InitializePanelCoroutine(ItemData.ItemType filter, bool isInitialLoad)
    {
        _currentFilter = filter;
        UpdateSubTabButtons();
        ClearAllSpawnedSlots();


        //////////
        List<InventoryItem> allItems = new List<InventoryItem>();
        for(int i=0; i<DBManager.I.currData.itemDatas.Count; i++)
        {
            CharacterData.ItemData cd = DBManager.I.currData.itemDatas[i];
            int find = DBManager.I.itemDatabase.allItems.FindIndex(x => x.itemName == cd.Name);
            if(find == -1) continue;
            ItemData d = DBManager.I.itemDatabase.allItems[find];
            InventoryItem inventoryItem = new InventoryItem(d, cd.count);
            allItems.Add(inventoryItem);
        }
        //////////
        Debug.Log(allItems.Count);

        
        List<InventoryItem> filteredList = allItems
            .Where(item => item.data != null && item.data.type == _currentFilter)
            .ToList();

        if (_currentFilter == ItemData.ItemType.Material)
        {
            filteredList.Sort((a, b) => a.data.itemName.CompareTo(b.data.itemName));
        }

        if (filteredList.Count > 0)
        {
            for (int i = 0; i < filteredList.Count; i++)
            {
                GameObject slotGO = Instantiate(itemSlotPrefab, contentTransform);
                ItemSlotUI slotUI = slotGO.GetComponent<ItemSlotUI>();
                slotUI.SetItem(filteredList[i], this); 
                _spawnedSlots.Add(slotUI);
            }
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform.GetComponent<RectTransform>());

        yield return new WaitForEndOfFrame();

        SetupSlotNavigation();

        if (filteredList.Count > 0)
        {
            ShowItemDetails(filteredList[0]);
        }
        else
        {
            ShowItemDetails(null);
        }

        if (isInitialLoad && firstSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
        }

        _initCoroutine = null;
    }
    
    private void UpdateMoneyText()
    {
        if (moneyText != null)
        {
            moneyText.text = DBManager.I.currData.gold.ToString("N0");
        }
    }

    public void ShowItemDetails(InventoryItem item)
    {
        if (infoPanelRoot != null) infoPanelRoot.SetActive(true);

        if (item != null && item.data != null)
        {
            detailItemNameText.text = item.data.itemName;
            detailItemImage.sprite = item.data.icon;
            detailItemDescriptionText.text = item.data.itemDescription;
            detailItemImage.gameObject.SetActive(item.data.icon != null);
        }
        else
        {
            detailItemNameText.text = "아이템 선택";
            detailItemImage.sprite = null;
            detailItemDescriptionText.text = "목록에서 아이템을 선택하세요.";
            detailItemImage.gameObject.SetActive(false); 
        }
    }
    
    private void ClearAllSpawnedSlots()
    {
        foreach (ItemSlotUI slot in _spawnedSlots) Destroy(slot.gameObject);
        _spawnedSlots.Clear();
    }

    private void SetupSlotNavigation()
    {
        if (_spawnedSlots.Count == 0)
        {
            SetSubTabNavigation(equipmentButton, mainTabButton, null, null, materialButton);
            SetSubTabNavigation(materialButton, mainTabButton, equipmentButton, null, null);
            return;
        }
        
        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            Button button = _spawnedSlots[i].GetComponent<Button>();
            if (button == null) continue;

            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (i == 0) 
                nav.selectOnUp = null; 
            else 
                nav.selectOnUp = _spawnedSlots[i - 1]?.GetComponent<Button>();

            nav.selectOnDown = (i == _spawnedSlots.Count - 1) ? null : _spawnedSlots[i + 1]?.GetComponent<Button>();
            nav.selectOnLeft = null;
            nav.selectOnRight = null;

            button.navigation = nav;
        }
        
        Button firstSlot = _spawnedSlots[0].GetComponent<Button>();
        
        SetSubTabNavigation(equipmentButton, mainTabButton, null, firstSlot, materialButton); 
        SetSubTabNavigation(materialButton, mainTabButton, equipmentButton, firstSlot, null);
    }

    private void SetSubTabNavigation(Button target, Selectable up, Button left, Button down, Button right)
    {
        Navigation nav = target.navigation;
        nav.mode = Navigation.Mode.Explicit;
        
        if (up != null) nav.selectOnUp = up;
        if (left != null) nav.selectOnLeft = left;
        if (down != null) nav.selectOnDown = down;
        if (right != null) nav.selectOnRight = right;
        
        target.navigation = nav;
    }

    private void UpdateSubTabButtons()
    {
        equipmentButton.GetComponentInChildren<TextMeshProUGUI>().color = ExampleColor(_currentFilter == ItemData.ItemType.Equipment);
        materialButton.GetComponentInChildren<TextMeshProUGUI>().color = ExampleColor(_currentFilter == ItemData.ItemType.Material);
    }

    private Color ExampleColor(bool isActive) => isActive ? subTabActiveColor : subTabIdleColor;
    
    #region 테스트용 코드

    [Header("테스트용")]
    [SerializeField] private ItemData testItemToAdd;

    [Button("Test: 아이템 추가 (장비/재료)")]
    private void TestAddItem()
    {
        if (testItemToAdd == null)
        {
            Debug.LogWarning("테스트할 아이템을 인스펙터 'Test Item To Add' 필드에 할당해주세요!");
            return;
        }
        
        InventoryDataManager.Instance.AddItem(testItemToAdd);
    }

    #endregion
}