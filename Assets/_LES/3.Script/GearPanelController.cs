using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;

public class GearPanelController : MonoBehaviour, ITabContent
{
    // --- (인스펙터 변수들은 기존과 동일) ---
    [Header("그리드 설정 (12칸)")]
    [SerializeField] private List<GearSlotUI> gridSlots;
    [Header("총 코스트 (왼쪽 위)")]
    [SerializeField] private CostMeterUI totalCostMeter;
    [SerializeField] private int maxCost = 3;
    [Header("상세 정보 패널 (오른쪽)")]
    [SerializeField] private TextMeshProUGUI detailGearName;
    [SerializeField] private Image detailGearImage;
    [SerializeField] private TextMeshProUGUI detailGearDescription;
    [SerializeField] private CostMeterUI detailCostMeter;
    [SerializeField] private GameObject detailModulePanel;
    [Header("내비게이션")]
    [SerializeField] private Selectable mainTabButton;
    [Header("플레이어 기어 인벤토리 (데이터)")]
    [SerializeField] private List<GearData> _playerGearInventory;

    private int _currentEquippedCost = 0;
    
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
        _currentEquippedCost = 0;
        
        // 1. 그리드 슬롯 채우기 (활성화/비활성화 결정)
        for (int i = 0; i < gridSlots.Count; i++)
        {
            if (i < _playerGearInventory.Count && _playerGearInventory[i] != null && !string.IsNullOrEmpty(_playerGearInventory[i].gearName))
            {
                GearData data = _playerGearInventory[i];
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
        totalCostMeter.SetMaxCost(maxCost);
        totalCostMeter.SetCost(_currentEquippedCost);

        // 3. [신규] 인덱스 기반의 스마트 내비게이션 설정
        SetupIndexBasedNavigation();

        // 4. 상세 정보창 업데이트
        GearData firstAvailableGear = _playerGearInventory.FirstOrDefault(gear => gear != null && !string.IsNullOrEmpty(gear.gearName));
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
            if (newCost <= maxCost)
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
}