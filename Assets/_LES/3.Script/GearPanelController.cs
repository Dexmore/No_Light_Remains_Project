using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;

public class GearPanelController : MonoBehaviour, ITabContent
{
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

    [Header("알림 UI")]
    [SerializeField] private NotificationUI notificationUI;

    private int _currentEquippedCost = 0;

    public void OnShow()
    {
        RefreshPanel(); 

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

        List<GearData> playerGears = new List<GearData>();
        for (int i = 0; i < DBManager.I.currData.gearDatas.Count; i++)
        {
            CharacterData.GearData cd = DBManager.I.currData.gearDatas[i];
            int find = DBManager.I.itemDatabase.allGears.FindIndex(x => x.name == cd.Name);
            if (find == -1) continue;
            GearData d = Instantiate(DBManager.I.itemDatabase.allGears[find]);
            d.name = DBManager.I.itemDatabase.allGears[find].name;
            d.isNew = cd.isNew;
            d.isEquipped = cd.isEquipped;
            playerGears.Add(d);
        }

        for (int i = 0; i < gridSlots.Count; i++)
        {
            if (i < playerGears.Count && playerGears[i] != null) // 이름 체크 조건 완화 (데이터가 확실하다면)
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
                gridSlots[i].ClearSlot(); 
            }
        }

        totalCostMeter.SetMaxCost(DBManager.I.currData.maxGearCost);
        totalCostMeter.SetCost(_currentEquippedCost);

        SetupIndexBasedNavigation();

        GearData firstAvailableGear = playerGears.FirstOrDefault(gear => gear != null);
        ShowSelectedGearDetails(firstAvailableGear);
    }

    private void SetupIndexBasedNavigation()
    {
        if (gridSlots.Count != 12) return;

        bool[] interactableMap = new bool[12];
        for (int i = 0; i < 12; i++)
        {
            interactableMap[i] = gridSlots[i].GetComponent<Button>().interactable;
        }

        for (int i = 0; i < 12; i++)
        {
            Button currentButton = gridSlots[i].GetComponent<Button>();

            if (!interactableMap[i])
            {
                Navigation nav = currentButton.navigation;
                nav.mode = Navigation.Mode.None;
                currentButton.navigation = nav;
                continue;
            }

            Navigation newNav = currentButton.navigation;
            newNav.mode = Navigation.Mode.Explicit;

            newNav.selectOnUp = FindTarget(i, interactableMap, Vector2.down, false) ?? mainTabButton;
            newNav.selectOnDown = FindTarget(i, interactableMap, Vector2.up, true);
            newNav.selectOnLeft = FindTarget(i, interactableMap, Vector2.left, true);
            newNav.selectOnRight = FindTarget(i, interactableMap, Vector2.right, true);

            currentButton.navigation = newNav;
        }
    }

    private Button FindTarget(int startIndex, bool[] map, Vector2 direction, bool wrap)
    {
        int row = startIndex / 4;
        int col = startIndex % 4;

        for (int j = 1; j < 12; j++)
        {
            int nextIndex = -1;

            if (direction == Vector2.right) 
            {
                int nextCol = col + j;
                if (!wrap && nextCol >= 4) break; 
                nextIndex = row * 4 + (nextCol % 4);
            }
            else if (direction == Vector2.left) 
            {
                int nextCol = col - j;
                if (!wrap && nextCol < 0) break; 
                nextIndex = row * 4 + ((nextCol % 4 + 4) % 4); 
            }
            else if (direction == Vector2.up) 
            {
                int nextRow = row + j;
                if (!wrap && nextRow >= 3) break; 
                nextIndex = (nextRow % 3) * 4 + col;
            }
            else if (direction == Vector2.down) 
            {
                int nextRow = row - j;
                if (!wrap && nextRow < 0) break; 
                nextIndex = ((nextRow % 3 + 3) % 3) * 4 + col;
            }

            if (nextIndex != startIndex && map[nextIndex])
            {
                return gridSlots[nextIndex].GetComponent<Button>();
            }

            if (direction.y != 0 && j >= 2) break; 
            if (direction.x != 0 && j >= 3) break; 
        }

        return null;
    }

    // [핵심 수정] 상세 정보 표시 로직 업데이트
    public void ShowSelectedGearDetails(GearData gear)
    {
        if (gear != null)
        {
            // 1. 이름 (로컬라이징)
            detailGearName.text = gear.gearName.GetLocalizedString();
            
            // 2. 아이콘
            detailGearImage.sprite = gear.gearIcon;
            detailGearImage.gameObject.SetActive(gear.gearIcon != null);

            // 3. 설명 (0강/1강 및 한글/영어 분기 처리)
            int currentLevel = DBManager.I.GetGearLevel(gear.name); // 현재 레벨 확인
            int locale = SettingManager.I.setting.locale; // 0:En, 1:Kr

            string description = "";

            if (currentLevel >= 1)
            {
                // 1강 상태 -> Sub 텍스트 (강화 효과) 표시
                description = (locale == 1) ? gear.upgradeSub_KR : gear.upgradeSub_EN;
            }
            else
            {
                // 0강 상태 -> Main 텍스트 (기본 효과) 표시
                description = (locale == 1) ? gear.upgradeMain_KR : gear.upgradeMain_EN;
            }

            detailGearDescription.text = description;

            // 4. 코스트 미터
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
            if (newCost <= DBManager.I.currData.maxGearCost)
            {
                gear.isEquipped = true;
                _currentEquippedCost = newCost;
                int find = DBManager.I.currData.gearDatas.FindIndex(x => x.Name == gear.name);
                if (find != -1)
                {
                    var temp = DBManager.I.currData.gearDatas[find];
                    temp.isEquipped = true;
                    DBManager.I.currData.gearDatas[find] = temp;
                }

                AudioManager.I?.PlaySFX("GearEquip");
            }
            else
            {
                Debug.Log("코스트가 부족하여 장착할 수 없습니다!");
                if (notificationUI != null) notificationUI.ShowMessage("코스트가 부족합니다.");
                AudioManager.I?.PlaySFX("AccessDenied");
                return;
            }
        }
        else
        {
            gear.isEquipped = false;
            _currentEquippedCost -= gear.cost;
            int find = DBManager.I.currData.gearDatas.FindIndex(x => x.Name == gear.name);
            if (find != -1)
            {
                var temp = DBManager.I.currData.gearDatas[find];
                temp.isEquipped = false;
                DBManager.I.currData.gearDatas[find] = temp;
            }

            AudioManager.I?.PlaySFX("GearUnequip");
        }

        totalCostMeter.SetCost(_currentEquippedCost);
        FindSlotForData(gear)?.UpdateEquipVisual();
    }

    private GearSlotUI FindSlotForData(GearData gear)
    {
        return gridSlots.FirstOrDefault(slot => slot.MyData == gear);
    }
}