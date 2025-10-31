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
    [SerializeField] private Selectable firstSelectable; // '장비' 버튼

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

    [Header("플레이어 인벤토리 (데이터)")]
    [SerializeField] 
    private List<ItemData> _playerInventory;

    private List<ItemSlotUI> _spawnedSlots = new List<ItemSlotUI>();
    private ItemData.ItemType _currentFilter = ItemData.ItemType.Equipment;
    private Coroutine _initCoroutine; // [추가] UI 초기화 코루틴을 제어하기 위함

    private void Awake()
    {
        // OnClick 리스너는 여기서 한 번만 설정
        equipmentButton?.onClick.AddListener(() => OnFilterChanged(ItemData.ItemType.Equipment));
        materialButton?.onClick.AddListener(() => OnFilterChanged(ItemData.ItemType.Material));
        ShowItemDetails(null); // 처음엔 정보창 비우기
    }

    /// <summary>
    /// 탭이 열릴 때 호출됩니다.
    /// </summary>
    public void OnShow()
    {
        // [수정] '장비' 탭을 기준으로, '초기 로드'임을 알리며 마스터 코루틴 실행
        StartMasterCoroutine(ItemData.ItemType.Equipment, true);

        // [추가] 돈 텍스트 갱신
        if (moneyText != null)
        {
            // TODO: '12345'를 실제 플레이어의 돈 데이터로 교체하세요.
            // (예: GameManager.Instance.PlayerMoney)
            int currentPlayerMoney = 12345; 

            // "N0" 포맷은 숫자에 1,234,567 처럼 콤마(,)를 찍어줍니다.
            moneyText.text = currentPlayerMoney.ToString("N0");
        }
    }
    
    /// <summary>
    /// 탭이 닫힐 때 호출됩니다.
    /// </summary>
    public void OnHide()
    {
        // 선택 해제
        if (EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.transform.IsChildOf(this.transform))
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        // 생성된 슬롯 모두 삭제
        ClearAllSpawnedSlots();
    }
    
    /// <summary>
    /// '장비' 또는 '재료' 버튼을 클릭했을 때 호출됩니다.
    /// </summary>
    private void OnFilterChanged(ItemData.ItemType newFilter)
    {
        // [수정] '초기 로드'가 아님(false)을 알리며 마스터 코루틴 실행
        StartMasterCoroutine(newFilter, false);
    }
    
    /// <summary>
    /// [신규] 기존에 실행 중인 코루틴이 있다면 중지하고 새 코루틴을 시작합니다.
    /// </summary>
    private void StartMasterCoroutine(ItemData.ItemType filter, bool isInitialLoad)
    {
        // 이전에 실행되던 UI 갱신 코루틴이 있다면 즉시 중지 (연속 클릭 시 꼬임 방지)
        if (_initCoroutine != null)
        {
            StopCoroutine(_initCoroutine);
        }
        // 모든 로직을 순차적으로 실행하는 '마스터 코루틴' 시작
        _initCoroutine = StartCoroutine(InitializePanelCoroutine(filter, isInitialLoad));
    }

    /// <summary>
    /// [신규] 모든 UI 생성, 갱신, 내비게이션, 포커스 설정을 '순서대로' 처리하는 마스터 코루틴
    /// </summary>
    private IEnumerator InitializePanelCoroutine(ItemData.ItemType filter, bool isInitialLoad)
    {
        // 1. 필터 및 서브탭 버튼 색상 설정
        _currentFilter = filter;
        UpdateSubTabButtons();
        
        // 2. 기존 슬롯 삭제
        ClearAllSpawnedSlots();

        // 3. 데이터 필터링
        List<ItemData> filteredList = _playerInventory
            .Where(item => item != null && item.type == _currentFilter)
            .ToList();

        // 4. 새 슬롯 생성 (Instantiate)
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
        
        // 5. [핵심] VerticalLayoutGroup이 슬롯 위치를 계산할 때까지 단 1 프레임 대기
        yield return new WaitForEndOfFrame(); 
        
        // 6. '모든 슬롯이 제자리를 찾은 후'에 내비게이션 설정
        SetupSlotNavigation();

        // 7. 첫 번째 아이템 정보 표시
        if (filteredList.Count > 0)
        {
            ShowItemDetails(filteredList[0]);
        }
        else
        {
            ShowItemDetails(null);
        }

        // 8. '초기 로드'일 경우에만 '장비' 버튼에 포커스
        if (isInitialLoad && firstSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
        }
        
        _initCoroutine = null; // 코루틴 완료
    }
    
    // [삭제] UpdateInventoryList() 함수 (모든 기능이 InitializePanelCoroutine으로 이동)
    // [삭제] SetInitialFocus() 코루틴 (모든 기능이 InitializePanelCoroutine으로 통합)

    #region (수정 없는 함수들)
    
    public void ShowItemDetails(ItemData data)
    {
        if (data != null)
        {
            if (infoPanelRoot != null) infoPanelRoot.SetActive(true);
            detailItemNameText.text = data.itemName;
            detailItemImage.sprite = data.icon;
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
        if (_spawnedSlots.Count == 0)
        {
            // [추가] 슬롯이 0개일 때도 장비/재료 버튼의 '아래'를 막아야 함
            Navigation eqNav = equipmentButton.navigation;
            eqNav.selectOnDown = null;
            equipmentButton.navigation = eqNav;
            
            Navigation matNav = materialButton.navigation;
            matNav.selectOnDown = null;
            materialButton.navigation = matNav;
            return;
        }
        
        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            Button button = _spawnedSlots[i].GetComponent<Button>();
            if (button == null) continue;
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = (i == 0) ? equipmentButton : _spawnedSlots[i - 1]?.GetComponent<Button>();
            nav.selectOnDown = (i == _spawnedSlots.Count - 1) ? button : _spawnedSlots[i + 1]?.GetComponent<Button>();
            nav.selectOnLeft = null;
            nav.selectOnRight = null;
            button.navigation = nav;
        }
        
        Navigation eqNav_Btn = equipmentButton.navigation;
        eqNav_Btn.selectOnDown = _spawnedSlots[0].GetComponent<Button>();
        equipmentButton.navigation = eqNav_Btn;
        
        Navigation matNav_Btn = materialButton.navigation;
        matNav_Btn.selectOnDown = _spawnedSlots[0].GetComponent<Button>();
        materialButton.navigation = matNav_Btn;
    }

    private void UpdateSubTabButtons()
    {
        equipmentButton.GetComponentInChildren<TextMeshProUGUI>().color = ExampleColor(_currentFilter == ItemData.ItemType.Equipment);
        materialButton.GetComponentInChildren<TextMeshProUGUI>().color = ExampleColor(_currentFilter == ItemData.ItemType.Material);
    }
    
    private Color ExampleColor(bool isActive) => isActive ? subTabActiveColor : subTabIdleColor;
    
    #endregion
}