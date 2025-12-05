using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using NaughtyAttributes;

public class GearPanelController : MonoBehaviour, ITabContent
{
    // --- (인스펙터 변수들은 기존과 동일) ---
    [Header("그리드 설정 (12칸)")]
    [SerializeField] private List<GearSlotUI> gridSlots;
    [Header("총 코스트 (왼쪽 위)")]
    [SerializeField] private CostMeterUI totalCostMeter;

    [Header("상세 정보 패널 (오른쪽)")]
    [SerializeField] private TextMeshProUGUI detailGearName;
    [SerializeField] private Image detailGearImage;
    [SerializeField] private TextMeshProUGUI detailGearDescription;
    [SerializeField] private CostMeterUI detailCostMeter;
    [SerializeField] private GameObject detailModulePanel;
    [Header("내비게이션")]
    [SerializeField] private Selectable mainTabButton;

    private int _currentEquippedCost = 0;
    
    // [추가] 이벤트 구독
    private void OnEnable()
    {
        if (InventoryDataManager.Instance != null)
        {
            InventoryDataManager.Instance.OnGearsChanged += RefreshPanel;
        }
    }

    private void OnDisable()
    {
        if (InventoryDataManager.Instance != null)
        {
            InventoryDataManager.Instance.OnGearsChanged -= RefreshPanel;
        }
    }

    public void OnShow()
    {
        RefreshPanel(); // 패널 새로고침 (이 안에서 내비게이션도 설정됨)
        
        // 활성화된 첫 번째 슬롯을 찾아 선택
        GearSlotUI firstInteractableSlot = gridSlots.FirstOrDefault(slot => slot.GetComponent<Button>().interactable);
        
        if (firstInteractableSlot != null)
        {
            firstInteractableSlot.GetComponent<Button>().Select();
        }
        else
        {
            mainTabButton?.Select();
        }
    }

    public void OnHide()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
    
    private void RefreshPanel()
    {
        if (InventoryDataManager.Instance == null) return;

        _currentEquippedCost = 0;

        //////////
        List<GearData> playerGears = new List<GearData>();
        for(int i=0; i<DBManager.I.currData.gearDatas.Count; i++)
        {
            CharacterData.GearData cd = DBManager.I.currData.gearDatas[i];
            int find = DBManager.I.itemDatabase.allRecords.FindIndex(x => x.recordTitle == cd.Name);
            if(find == -1) continue;
            GearData d = DBManager.I.itemDatabase.allGears[find];
            playerGears.Add(d);
        }
        //////////
        Debug.Log(playerGears.Count);
        
        // 1. 그리드 슬롯 채우기 (활성화/비활성화 결정)
        for (int i = 0; i < gridSlots.Count; i++)
        {
            if (i < playerGears.Count && playerGears[i] != null && !string.IsNullOrEmpty(playerGears[i].gearName))
            {
                GearData data = playerGears[i];
                gridSlots[i].SetData(data, this);
                if (data.isEquipped)
                {
                    _currentEquippedCost += data.cost;
                }
            }
            else
            {
                gridSlots[i].ClearSlot(); // 이 함수가 버튼을 not interactable로 만듦
            }
        }
        
        // 2. 코스트 미터 업데이트
        totalCostMeter.SetMaxCost(InventoryDataManager.Instance.MaxGearCost);
        totalCostMeter.SetCost(_currentEquippedCost);

        // 3. [신규] 인덱스 기반의 스마트 내비게이션 설정
        SetupIndexBasedNavigation();

        // 4. 상세 정보창 업데이트
        GearData firstAvailableGear = playerGears.FirstOrDefault(gear => gear != null && !string.IsNullOrEmpty(gear.gearName));
        ShowSelectedGearDetails(firstAvailableGear);
    }

    /// <summary>
    /// [신규] 인덱스(0-11) 기반으로 활성화된 슬롯끼리 내비게이션을 연결합니다.
    /// </summary>
    private void SetupIndexBasedNavigation()
    {
        if (gridSlots.Count != 12) return;

        // 1. 12칸 그리드의 활성화 상태 맵을 만듭니다. (빠른 조회용)
        bool[] interactableMap = new bool[12];
        for (int i = 0; i < 12; i++)
        {
            interactableMap[i] = gridSlots[i].GetComponent<Button>().interactable;
        }

        // 2. 12칸을 모두 순회
        for (int i = 0; i < 12; i++)
        {
            Button currentButton = gridSlots[i].GetComponent<Button>();
            
            // 3. 비활성화된 슬롯이면, 내비게이션을 'None'으로 설정하고 건너뜀
            if (!interactableMap[i])
            {
                Navigation nav = currentButton.navigation;
                nav.mode = Navigation.Mode.None;
                currentButton.navigation = nav;
                continue;
            }
            
            // 4. 활성화된 슬롯이면, 상/하/좌/우 대상을 검색
            Navigation newNav = currentButton.navigation;
            newNav.mode = Navigation.Mode.Explicit;

            // 'Up'은 래핑(looping)하지 않고 탭으로 탈출
            newNav.selectOnUp = FindTarget(i, interactableMap, Vector2.down, false) ?? mainTabButton;
            // 'Down'은 래핑
            newNav.selectOnDown = FindTarget(i, interactableMap, Vector2.up, true);
            // 'Left'는 래핑
            newNav.selectOnLeft = FindTarget(i, interactableMap, Vector2.left, true);
            // 'Right'는 래핑
            newNav.selectOnRight = FindTarget(i, interactableMap, Vector2.right, true);

            currentButton.navigation = newNav;
        }
    }

    /// <summary>
    /// [신규 헬퍼] 현재 인덱스(startIndex)에서 특정 방향으로 활성화된(map) 다음 타겟을 찾습니다.
    /// </summary>
    private Button FindTarget(int startIndex, bool[] map, Vector2 direction, bool wrap)
    {
        int row = startIndex / 4;
        int col = startIndex % 4;
        
        // 1. 한 칸씩 순차적으로 검색
        for (int j = 1; j < 12; j++)
        {
            int nextIndex = -1;

            if (direction == Vector2.right) // 오른쪽
            {
                int nextCol = col + j;
                if (!wrap && nextCol >= 4) break; // 래핑 안 함
                nextIndex = row * 4 + (nextCol % 4);
            }
            else if (direction == Vector2.left) // 왼쪽
            {
                int nextCol = col - j;
                if (!wrap && nextCol < 0) break; // 래핑 안 함
                nextIndex = row * 4 + ((nextCol % 4 + 4) % 4); // C#의 % 연산자 보정
            }
            else if (direction == Vector2.up) // 위쪽
            {
                int nextRow = row + j;
                if (!wrap && nextRow >= 3) break; // 래핑 안 함
                nextIndex = (nextRow % 3) * 4 + col;
            }
            else if (direction == Vector2.down) // 아래쪽
            {
                int nextRow = row - j;
                if (!wrap && nextRow < 0) break; // 래핑 안 함
                nextIndex = ((nextRow % 3 + 3) % 3) * 4 + col;
            }

            // 2. 자기 자신이 아니고 활성화되어있으면 즉시 반환
            if (nextIndex != startIndex && map[nextIndex])
            {
                return gridSlots[nextIndex].GetComponent<Button>();
            }
            
            // 3. 한 줄/열을 다 돌았는데 래핑이 아니면 중지
            if (direction.y != 0 && j >= 2) break; // 상/하 (3줄)
            if (direction.x != 0 && j >= 3) break; // 좌/우 (4줄)
        }
        
        // 4. 대상을 못 찾음
        return null;
    }

    // --- (ShowSelectedGearDetails, ToggleEquipGear, FindSlotForData 함수는 기존과 동일) ---
    // (함수 시그니처가 약간 변경되어 전체를 다시 붙여넣습니다)
    #region (수정 없는 함수들)
    public void ShowSelectedGearDetails(GearData gear)
    {
        if (gear != null)
        {
            detailGearName.text = gear.gearName;
            detailGearImage.sprite = gear.gearIcon;
            detailGearImage.gameObject.SetActive(gear.gearIcon != null);
            detailGearDescription.text = gear.gearDescription;

            detailCostMeter.SetMaxCost(detailCostMeter.GetTotalPipCount());
            detailCostMeter.SetCost(gear.cost);
        }
        else
        {
            detailGearName.text = "기어 슬롯";
            detailGearImage.sprite = null;
            detailGearImage.gameObject.SetActive(false);
            detailGearDescription.text = "선택한 기어의 정보가 표시됩니다.";
            detailCostMeter.SetMaxCost(detailCostMeter.GetTotalPipCount());
            detailCostMeter.SetCost(0);
        }
    }
    
    public void ToggleEquipGear(GearData gear)
    {
        if (!gear.isEquipped)
        {
            int newCost = _currentEquippedCost + gear.cost;
            if (newCost <= InventoryDataManager.Instance.MaxGearCost)
            {
                gear.isEquipped = true;
                _currentEquippedCost = newCost;
            }
            else
            {
                Debug.Log("코스트가 부족하여 장착할 수 없습니다!");
                return;
            }
        }
        else
        {
            gear.isEquipped = false;
            _currentEquippedCost -= gear.cost;
        }
        
        totalCostMeter.SetCost(_currentEquippedCost);
        FindSlotForData(gear)?.UpdateEquipVisual();
    }

    private GearSlotUI FindSlotForData(GearData gear)
    {
        return gridSlots.FirstOrDefault(slot => slot.MyData == gear);
    }
    #endregion
    
    #region 테스트용 코드

    // [추가] 인스펙터에서 테스트용으로 추가할 기어를 미리 할당
    [Header("테스트용")]
    [SerializeField] private GearData testGearToAdd;

    [Button("Test: 기어 추가")]
    private void TestAddGear()
    {
        if (testGearToAdd == null)
        {
            Debug.LogWarning("테스트할 기어를 인스펙터 'Test Gear To Add' 필드에 할당해주세요!");
            return;
        }

        // 1. 데이터 리스트에 기어를 추가합니다.
        InventoryDataManager.Instance.AddItem(testGearToAdd);

        // 2. UI를 새로고침합니다.
        OnShow(); 

        Debug.Log($"{testGearToAdd.gearName} 기어 추가 테스트 완료.");
    }

    #endregion
}