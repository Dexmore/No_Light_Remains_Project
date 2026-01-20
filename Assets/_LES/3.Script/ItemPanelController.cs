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
    [Header("UI 내비게이션 설정 (필수 연결)")]
    [Tooltip("현재 탭 버튼 (예: 재료)")]
    [SerializeField] private Selectable mainTabButton;

    [Tooltip("왼쪽 탭 버튼 (없으면 비워두세요)")]
    [SerializeField] private Button prevTabButton; // [추가] 왼쪽 연결용

    [Tooltip("오른쪽 탭 버튼 (예: 랜턴 탭)")]
    [SerializeField] private Button nextTabButton; // [추가] 오른쪽 연결용

    [Header("스크롤 제어")]
    [SerializeField] private AutoScroll autoScroll;
    [SerializeField] private ScrollRect scrollRect;

    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("아이템 슬롯 관리")]
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform contentTransform;

    [Header("아이템 정보(Info) 창")]
    [SerializeField] private TextMeshProUGUI detailItemNameText;
    [SerializeField] private Image detailItemImage;
    [SerializeField] private TextMeshProUGUI detailItemDescriptionText;
    [SerializeField] private GameObject infoPanelRoot;

    private List<ItemSlotUI> _spawnedSlots = new List<ItemSlotUI>();
    private Coroutine _initCoroutine;

    private void OnEnable()
    {
        UpdateMoneyText();
    }

    private void Update()
    {
        // 슬롯에서 위로 탈출하는 로직
        if (_spawnedSlots.Count == 0 || scrollRect == null) return;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (_spawnedSlots[0] == null) return;

        Button firstSlotBtn = _spawnedSlots[0].GetComponent<Button>();

        if (currentSelected == firstSlotBtn.gameObject)
        {
            Navigation nav = firstSlotBtn.navigation;
            bool isAtTop = false;

            if (autoScroll != null && autoScroll.IsScrolledToTop) isAtTop = true;
            else if (contentTransform.GetComponent<RectTransform>().rect.height <= scrollRect.viewport.rect.height) isAtTop = true;

            // 맨 위라면 -> 위 키는 메인 탭 버튼
            if (isAtTop) nav.selectOnUp = mainTabButton;
            else nav.selectOnUp = null;
            
            firstSlotBtn.navigation = nav;
        }
    }

    public void OnShow()
    {
        StartMasterCoroutine(true);
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
    
    private void StartMasterCoroutine(bool isInitialLoad)
    {
        if (_initCoroutine != null) StopCoroutine(_initCoroutine);
        _initCoroutine = StartCoroutine(InitializePanelCoroutine(isInitialLoad));
    }

    private IEnumerator InitializePanelCoroutine(bool isInitialLoad)
    {
        ClearAllSpawnedSlots();

        List<InventoryItem> allItems = new List<InventoryItem>();
        if (DBManager.I != null && DBManager.I.currData.itemDatas != null)
        {
            for(int i=0; i < DBManager.I.currData.itemDatas.Count; i++)
            {
                CharacterData.ItemData cd = DBManager.I.currData.itemDatas[i];
                int find = DBManager.I.itemDatabase.allItems.FindIndex(x => x.name == cd.Name);
                if(find == -1) continue;
                
                ItemData d = Instantiate(DBManager.I.itemDatabase.allItems[find]);
                d.name = DBManager.I.itemDatabase.allItems[find].name;
                d.isNew = cd.isNew;
                InventoryItem inventoryItem = new InventoryItem(d, cd.count);
                allItems.Add(inventoryItem);
            }
        }
        
        List<InventoryItem> filteredList = allItems
            .Where(item => item.data != null && item.data.type == ItemData.ItemType.Material)
            .ToList();

        filteredList.Sort((a, b) => a.data.itemName.GetLocalizedString().CompareTo(b.data.itemName.GetLocalizedString()));

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

        if (filteredList.Count > 0) ShowItemDetails(filteredList[0]);
        else ShowItemDetails(null);

        if (isInitialLoad)
        {
            if (_spawnedSlots.Count > 0) EventSystem.current.SetSelectedGameObject(_spawnedSlots[0].gameObject);
            else if (mainTabButton != null) EventSystem.current.SetSelectedGameObject(mainTabButton.gameObject);
        }

        _initCoroutine = null;
    }
    
    private void UpdateMoneyText()
    {
        if (moneyText != null && DBManager.I != null)
            moneyText.text = DBManager.I.currData.gold.ToString("N0");
    }

    public void ShowItemDetails(InventoryItem item)
    {
        if (infoPanelRoot != null) infoPanelRoot.SetActive(true);

        if (item != null && item.data != null)
        {
            detailItemNameText.text = item.data.itemName.GetLocalizedString();
            detailItemImage.sprite = item.data.icon;
            detailItemDescriptionText.text = item.data.itemDescription.GetLocalizedString();
            detailItemImage.gameObject.SetActive(item.data.icon != null);
        }
        else
        {
            if(SettingManager.I.setting.locale == 0)
            {
                detailItemNameText.text = "Currently No Item.";
                detailItemDescriptionText.text = "No gear enhancement materials detected in inventory.";
            }
            else if(SettingManager.I.setting.locale == 1)
            {
                detailItemNameText.text = "아이템 미소지.";       
                detailItemDescriptionText.text = "보유 중인 기어 강화 재료 아이템이 없습니다.";
            }
            detailItemImage.sprite = null;
            detailItemImage.gameObject.SetActive(false); 
        }
    }
    
    private void ClearAllSpawnedSlots()
    {
        foreach (ItemSlotUI slot in _spawnedSlots) if(slot != null) Destroy(slot.gameObject);
        _spawnedSlots.Clear();
    }

    // [최종 수정] 좌우/아래 모든 방향을 확실하게 연결
    private void SetupSlotNavigation()
    {
        // 1. 메인 탭 버튼 연결 (탭 -> 리스트, 탭 <-> 탭)
        if (mainTabButton != null)
        {
            Navigation customNav = mainTabButton.navigation;
            customNav.mode = Navigation.Mode.Explicit; // 수동 모드 설정

            // [아래] 슬롯이 있으면 0번 슬롯, 없으면 막음
            if (_spawnedSlots.Count > 0)
                customNav.selectOnDown = _spawnedSlots[0].GetComponent<Button>();
            else
                customNav.selectOnDown = null;

            // [좌/우] 인스펙터에서 연결한 버튼으로 강제 설정
            customNav.selectOnLeft = prevTabButton;  // Q 방향
            customNav.selectOnRight = nextTabButton; // 랜턴 방향

            mainTabButton.navigation = customNav;
        }

        if (_spawnedSlots.Count == 0) return;
        
        // 2. 슬롯 리스트 연결 (리스트 <-> 리스트, 리스트 -> 탭)
        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            Button button = _spawnedSlots[i].GetComponent<Button>();
            if (button == null) continue;

            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            // 위: 첫 슬롯은 메인 탭으로
            if (i == 0) nav.selectOnUp = mainTabButton;
            else nav.selectOnUp = _spawnedSlots[i - 1]?.GetComponent<Button>();

            // 아래: 다음 슬롯으로
            nav.selectOnDown = (i == _spawnedSlots.Count - 1) ? null : _spawnedSlots[i + 1]?.GetComponent<Button>();
            
            nav.selectOnLeft = null;
            nav.selectOnRight = null;

            button.navigation = nav;
        }
    }
}