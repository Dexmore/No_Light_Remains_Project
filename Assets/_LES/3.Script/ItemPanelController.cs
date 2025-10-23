using UnityEngine;
using UnityEngine.EventSystems; // EventSystem을 사용하기 위해 필수!
using UnityEngine.UI; // Button과 같은 UI 요소를 사용하기 위해!
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해
using System.Linq; // Linq (필터링)을 사용하기 위해
using TMPro;

public class ItemPanelController : MonoBehaviour, ITabContent
{
    [Header("UI 내비게이션 설정")]
    [Tooltip("이 탭이 열릴 때 가장 먼저 선택될 UI 요소 (보통 첫 번째 아이템 슬롯)")]
    [SerializeField]
    private Selectable firstSelectable; // Button, Toggle 등 선택 가능한 모든 것

    [Header("서브-탭 (필터) 버튼")]
    [SerializeField] private Button equipmentButton; // 장비 버튼
    [SerializeField] private Button materialButton; // 재료 버튼

    // [수정] 서브-탭 선택 시 색상 (필요 없다면 삭제 가능)
    [SerializeField] private Color subTabActiveColor = Color.white;
    [SerializeField] private Color subTabIdleColor = Color.gray;

    [Header("아이템 슬롯 관리")]
    [Tooltip("아이템 슬롯으로 사용할 프리팹 (ItemSlotUI 스크립트 포함)")]
    [SerializeField]
    private GameObject itemSlotPrefab; // [수정] 리스트 대신 프리팹 1개

    [Tooltip("프리팹이 생성될 부모 Transform (Scroll View의 Content 오브젝트)")]
    [SerializeField]
    private Transform contentTransform; // [수정] 슬롯이 생성될 위치

    // [추가] 아이템 정보(Info) 창 UI 요소들
    [Header("아이템 정보(Info) 창")]
    [SerializeField] private TextMeshProUGUI detailItemNameText;
    [SerializeField] private Image detailItemImage;
    [SerializeField] private TextMeshProUGUI detailItemDescriptionText;
    [SerializeField] private GameObject infoPanelRoot; // 정보창 전체를 묶는 부모 (선택사항)

    private List<ItemData> _playerInventory = new List<ItemData>();

    // 현재 생성된 슬롯 UI들을 관리하는 리스트
    private List<ItemSlotUI> _spawnedSlots = new List<ItemSlotUI>();

    private ItemData.ItemType _currentFilter = ItemData.ItemType.Equipment;

    private void Awake()
    {
        // 서브-탭 버튼에 클릭 리스너 연결
        equipmentButton?.onClick.AddListener(() => OnFilterChanged(ItemData.ItemType.Equipment));
        materialButton?.onClick.AddListener(() => OnFilterChanged(ItemData.ItemType.Material));
        
        // [테스트용] 임시 아이템 데이터 생성
        CreateTestData();

        // [추가] 처음에는 정보창을 비워둠
        ShowItemDetails(null);
    }

    public void OnShow()
    {
        Debug.Log("소지템 탭이 열렸습니다. UI 갱신 및 포커스 설정.");
        // TODO: 실제 인벤토리 아이템 목록을 UI에 표시하는 로직 구현

        // 탭이 열릴 때마다 기본 필터('장비')로 목록을 새로고침
        OnFilterChanged(ItemData.ItemType.Equipment);
        
        // [핵심 로직 1]
        // 이 탭이 보이게 되면, EventSystem에게 'firstSelectable'을 선택하라고 명령합니다.
        // null이 아닐 경우에만 실행
        if (firstSelectable != null)
        {
            // 한 프레임 대기해야 CanvasGroup 페이드인이 끝난 후 정상적으로 선택될 수 있습니다.
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
        }
    }

    public void OnHide()
    {
        Debug.Log("소지템 탭이 닫혔습니다.");

        // [핵심 로직 2]
        // 탭이 닫힐 때, 현재 선택된 것을 해제하여
        // 다른 탭으로 포커스가 넘어가는 것을 방지합니다.
        if (EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.transform.IsChildOf(this.transform))
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // [추가] 탭이 닫힐 때 생성된 슬롯들을 정리 (선택사항)
        ClearAllSpawnedSlots();
    }

    /// <summary>
    /// '장비' 또는 '재료' 버튼을 눌렀을 때 호출됩니다.
    /// </summary>
    private void OnFilterChanged(ItemData.ItemType newFilter)
    {
        _currentFilter = newFilter;
        
        // 서브-탭 버튼 색상 업데이트
        UpdateSubTabButtons();
        
        // 인벤토리 목록을 다시 그림
        UpdateInventoryList();
    }

    /// <summary>
    /// 현재 필터에 맞춰 인벤토리 슬롯 목록을 갱신합니다.
    /// </summary>
    private void UpdateInventoryList()
    {
        // 1. 기존에 생성된 슬롯들을 모두 제거
        ClearAllSpawnedSlots();

        // 2. 현재 필터에 맞는 아이템만 골라냄
        List<ItemData> filteredList = _playerInventory
            .Where(item => item.type == _currentFilter)
            .ToList();

        if (filteredList.Count == 0)
        {
            ShowItemDetails(null);
            return;
        }

        // 3. 아이템 개수만큼 슬롯 프리팹을 Content에 생성
        for (int i = 0; i < filteredList.Count; i++)
        {
            GameObject slotGO = Instantiate(itemSlotPrefab, contentTransform);
            ItemSlotUI slotUI = slotGO.GetComponent<ItemSlotUI>();
            
            
            slotUI.SetItem(filteredList[i], this);
            _spawnedSlots.Add(slotUI);
        }
        
        // 4. (중요) 새로 생성된 슬롯들의 내비게이션을 자동으로 연결
        SetupSlotNavigation();

        // [추가] 목록 갱신 시, 첫 번째 아이템 정보를 자동으로 표시
        ShowItemDetails(filteredList[0]);
    }

    /// <summary>
    /// [공개] ItemSlotUI가 호출할 함수. 아이템 상세 정보를 오른쪽에 표시합니다.
    /// </summary>
    public void ShowItemDetails(ItemData data)
    {
        if (data != null)
        {
            // 정보 패널 활성화
            if (infoPanelRoot != null) infoPanelRoot.SetActive(true);
            
            detailItemNameText.text = data.itemName;
            detailItemImage.sprite = data.icon;
            detailItemDescriptionText.text = data.itemDescription;

            // 아이콘이 없으면 이미지를 비활성화 (중요)
            detailItemImage.gameObject.SetActive(data.icon != null);
        }
        else
        {
            // 빈 슬롯이거나 아이템이 없을 때 정보창을 비움
            if (infoPanelRoot != null) infoPanelRoot.SetActive(false); // 아예 숨길 수도 있음
            
            detailItemNameText.text = "아이템 선택";
            detailItemImage.sprite = null;
            detailItemImage.gameObject.SetActive(false);
            detailItemDescriptionText.text = "목록에서 아이템을 선택하세요.";
        }
    }

    #region (기존 함수들 - 수정 없음)
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
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = (i == 0) ? equipmentButton : _spawnedSlots[i - 1].GetComponent<Button>();
            nav.selectOnDown = (i == _spawnedSlots.Count - 1) ? button : _spawnedSlots[i + 1].GetComponent<Button>();
            nav.selectOnLeft = null;
            nav.selectOnRight = null; // [수정] 오른쪽 정보창으로 포커스 이동을 원하면 여기를 수정
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
    // UpdateSubTabButtons에서 사용할 간단한 색상 반환 헬퍼
    private Color ExampleColor(bool isActive) => isActive ? subTabActiveColor : subTabIdleColor;
    #endregion
    
    // --- 테스트용 임시 데이터 ---
    private void CreateTestData()
    {
        // 아이콘(null), 설명, 새 아이템 여부(isNew) 추가
        _playerInventory.Add(new ItemData("쓸만한 검", null, ItemData.ItemType.Equipment, "기본적인 검입니다.", true));
        _playerInventory.Add(new ItemData("가죽 갑옷", null, ItemData.ItemType.Equipment, "질긴 가죽으로 만든 갑옷입니다.", true));
        _playerInventory.Add(new ItemData("철광석", null, ItemData.ItemType.Material, "무기나 도구를 만들 수 있는 광석입니다.", true));
        _playerInventory.Add(new ItemData("붉은 약초", null, ItemData.ItemType.Material, "회복 물약의 재료로 쓰입니다.", true));
        _playerInventory.Add(new ItemData("빛나는 헬멧", null, ItemData.ItemType.Equipment, "어둠 속에서 희미한 빛을 냅니다.", true));
        _playerInventory.Add(new ItemData("질긴 가죽", null, ItemData.ItemType.Material, "갑옷을 강화하는 데 사용됩니다.", true));
    }
}