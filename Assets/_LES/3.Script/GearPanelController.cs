using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;

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
    [SerializeField] private TextMeshProUGUI detailEnhanceText;
    [SerializeField] private CostMeterUI detailCostMeter;
    [SerializeField] private GameObject detailModulePanel;
    [Header("내비게이션")]
    [SerializeField] private Selectable mainTabButton;

    [Header("알림 UI")]
    [SerializeField] private NotificationUI notificationUI;

    private int _currentEquippedCost = 0;

    private GearData _currentSelectedGear;

    private void Start()
    {
        // 언어가 바뀌면 OnLocaleChanged 함수를 실행해라! 라고 등록
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        
        // (기존 Start 내용이 있다면 여기에 유지...)
    }

    private void OnDestroy()
    {
        // 게임이 꺼지거나 패널이 사라질 때 이벤트 연결 해제 (메모리 누수 방지)
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    // 언어가 바뀌면 자동으로 호출되는 함수
    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        // 패널이 켜져 있고, 선택된 기어가 있다면 화면을 새로고침
        if (gameObject.activeInHierarchy && _currentSelectedGear != null)
        {
            ShowSelectedGearDetails(_currentSelectedGear);
        }
    }

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
        // 현재 선택된 기어를 변수에 저장 (언어 바뀔 때 다시 쓰려고)
        _currentSelectedGear = gear; 

        if (gear != null)
        {
            detailGearName.text = gear.localizedName;
            detailGearImage.sprite = gear.gearIcon;
            detailGearImage.gameObject.SetActive(gear.gearIcon != null);

            // [수정된 부분] ---------------------------------------------------
            // 기존의 "값이 있으면 그냥 쓴다"는 if문을 지웠습니다.
            // 대신 항상 Localization 시스템에 "지금 언어로 줘!"라고 요청합니다.
            
            if (detailGearDescription != null)
            {
                if (!gear.gearDescription.IsEmpty)
                {
                    // 비동기로 텍스트 요청 (항상 실행)
                    gear.gearDescription.GetLocalizedStringAsync().Completed += (op) => 
                    {
                        // 로딩이 끝나면 UI에 반영 (UI가 여전히 켜져있는지 체크)
                        if (detailGearDescription != null && gameObject.activeInHierarchy) 
                        {
                            detailGearDescription.text = op.Result;
                            // 필요하다면 변수에도 업데이트 (선택 사항)
                            gear.localizedNormalDescription = op.Result; 
                        }
                    };
                }
                else
                {
                    detailGearDescription.text = ""; 
                }
            }
            // ------------------------------------------------------------------

            // 2. 강화 효과 텍스트 (이건 원래 잘 작동했음)
            if (detailEnhanceText != null)
            {
                // GetEffectText 함수 내부에서 현재 언어를 체크하므로 잘 작동함
                string effectText = gear.GetEffectText(gear.currentLevel);
                detailEnhanceText.text = $"<color=#FFD700>{effectText}</color>";
            }

            // 비용 미터기 설정 (기존 유지)
            detailCostMeter.SetMaxCost(detailCostMeter.GetTotalPipCount());
            detailCostMeter.SetCost(gear.cost);
        }
        else
        {
            // 선택 해제 시 초기화
            detailGearName.text = "기어 슬롯";
            detailGearImage.sprite = null;
            detailGearImage.gameObject.SetActive(false);
            
            if (detailGearDescription != null) detailGearDescription.text = "";
            if (detailEnhanceText != null) detailEnhanceText.text = "기어를 선택하세요.";

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