using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WorkbenchUI : MonoBehaviour
{
    public System.Action OnClose;

    [Header("UI 연결")]
    [SerializeField] private GameObject panelRoot; 
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform slotContent;

    [Header("정보 표시")]
    [SerializeField] private TextMeshProUGUI gearTitleText;
    [SerializeField] private TextMeshProUGUI currentEffectText;
    [SerializeField] private TextMeshProUGUI nextEffectText;
    [SerializeField] private Image targetGearImage;

    [Header("추가 정보")]
    [SerializeField] private TextMeshProUGUI flavorTitleText; 
    [SerializeField] private TextMeshProUGUI flavorBodyText; 

    [Header("강화 UI")]
    [SerializeField] private TextMeshProUGUI[] costTexts;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    private GearData _targetGearData;
    private WorkbenchSlotUI _selectedSlotUI;
    private List<WorkbenchSlotUI> _preplacedSlots = new List<WorkbenchSlotUI>();

    public bool IsUIActive()
    {
        if (panelRoot != null)
            return panelRoot.activeSelf;
        return false;
    }

    private void Start()
    {
        if(closeButton != null) closeButton.onClick.AddListener(Close);
        if(enhanceButton != null) enhanceButton.onClick.AddListener(OnClickEnhance);
        
        if(panelRoot != null) 
        {
            panelRoot.SetActive(false);
        }

        if (slotContent != null)
        {
            foreach (Transform child in slotContent)
            {
                var slot = child.GetComponent<WorkbenchSlotUI>();
                if (slot != null) _preplacedSlots.Add(slot);
            }
        }
    }
    
    private void Update()
    {
        if (!IsUIActive() || Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame) Close();
        
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            if (EventSystem.current.currentSelectedGameObject == enhanceButton.gameObject)
            {
                if (enhanceButton.interactable) enhanceButton.onClick.Invoke();
                else FindObjectOfType<NotificationUI>()?.ShowMessage("조건이 부족합니다.");
            }
        }
    }

    public void Open()
    {
        if(panelRoot != null) panelRoot.SetActive(true);
        
        RefreshSlotList(); 
        ClearInfo();
        _targetGearData = null;
        StartCoroutine(SelectFirstSlot());
    }
    
    public void Close()
    {
        if(panelRoot != null) panelRoot.SetActive(false);
        OnClose?.Invoke();
    }

    private void RefreshSlotList()
    {
        if (DBManager.I == null) return;
        var userGears = DBManager.I.currData.gearDatas;

        for (int i = 0; i < _preplacedSlots.Count; i++)
        {
            // 모든 슬롯을 켜서 프리팹에서 설정한 네비게이션 경로가 끊기지 않게 유지
            _preplacedSlots[i].gameObject.SetActive(true); 
            _preplacedSlots[i].SetSelectedState(false); 

            if (i < userGears.Count)
            {
                var userGear = userGears[i];
                GearData gearData = DBManager.I.itemDatabase.FindGearByName(userGear.Name);
                if (gearData != null) _preplacedSlots[i].Setup(gearData, this);
                else _preplacedSlots[i].SetupEmpty(this);
            }
            else
            {
                _preplacedSlots[i].SetupEmpty(this);
            }
        }

        // [수정됨] 이 함수가 범인이었습니다. 삭제합니다!
        // ConnectNavigation(); 
    }

    // [수정됨] ConnectNavigation 함수 전체 삭제 (더 이상 코드로 네비게이션을 덮어쓰지 않음)
    /*
    private void ConnectNavigation()
    {
        // (코드로 자동 연결하던 로직 제거)
        // 이제 유니티 인스펙터(Inspector)에서 설정한 Visualize 화살표대로 작동합니다.
    }
    */

    private System.Collections.IEnumerator SelectFirstSlot()
    {
        yield return null;
        if (_preplacedSlots.Count > 0 && _preplacedSlots[0].gameObject.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(_preplacedSlots[0].gameObject);
        }
    }

    public void ShowPreview(GearData data)
    {
        if (_targetGearData == null) UpdateInfoUI(data); 
    }

    public void ConfirmSelection(WorkbenchSlotUI slotUI)
    {
        if (_selectedSlotUI != null) _selectedSlotUI.SetSelectedState(false);
        _selectedSlotUI = slotUI;
        _targetGearData = slotUI.Data;
        if (_selectedSlotUI != null) _selectedSlotUI.SetSelectedState(true);
        UpdateInfoUI(_targetGearData);
    }

    private void ClearInfo()
    {
        if (gearTitleText != null) gearTitleText.text = "[ 기어를 선택하세요 ]";
        if (targetGearImage != null) targetGearImage.gameObject.SetActive(false);
        if (currentEffectText != null) currentEffectText.text = "";
        if (nextEffectText != null) nextEffectText.text = "";
        if (flavorTitleText != null) flavorTitleText.text = "";
        if (flavorBodyText != null) flavorBodyText.text = "";
        SetInteractable(false, "선택 필요", "-", "-", "-");
    }

    private void UpdateInfoUI(GearData data)
    {
        if (data == null) { ClearInfo(); return; }

        if (gearTitleText != null) gearTitleText.text = $"[ {data.localizedName} ]";
        if (targetGearImage != null)
        {
            targetGearImage.sprite = data.gearIcon;
            targetGearImage.gameObject.SetActive(true);
        }

        if (flavorTitleText != null) flavorTitleText.text = "상세 정보";
        if (flavorBodyText != null) flavorBodyText.text = ""; 

        int currentLevel = DBManager.I.GetGearLevel(data.name);
        int locale = SettingManager.I.setting.locale;
        string textLv0 = (locale == 1) ? data.upgradeMain_KR : data.upgradeMain_EN;
        string textLv1 = (locale == 1) ? data.upgradeSub_KR : data.upgradeSub_EN;

        if (currentLevel >= 1)
        {
            if (currentEffectText) currentEffectText.text = $"> {textLv1}";
            if (nextEffectText) nextEffectText.text = (locale == 1) ? "> 강화 최종 단계" : "> Max Level Reached";
            SetInteractable(false, "강화 완료", "-", "-", "-");
        }
        else
        {
            if (currentEffectText) currentEffectText.text = $"> {textLv0}";
            if (nextEffectText) nextEffectText.text = $"> {textLv1}";

            if (_targetGearData == data)
            {
                EnhancementManager.LevelInfo info = GetCostInfo(data);
                CheckCostAndEnableButton(info);
            }
            else
            {
                SetInteractable(false, "선택(Enter)하여 확정", "-", "-", "-");
            }
        }
    }

    private void CheckCostAndEnableButton(EnhancementManager.LevelInfo info)
    {
        bool isEnough = true;
        if (DBManager.I.currData.gold < info.goldCost) isEnough = false;
        string goldText = $"{info.goldCost} G";
        string mat1Text = "";
        string mat2Text = "";

        if (info.requiredMaterials != null)
        {
            for (int i = 0; i < info.requiredMaterials.Count; i++)
            {
                if (i >= 2) break;
                var mat = info.requiredMaterials[i];
                if (mat.item == null) continue;
                string targetName = mat.item.name;
                int totalCount = 0;
                if (DBManager.I.currData.itemDatas != null)
                {
                    foreach (var dbItem in DBManager.I.currData.itemDatas)
                        if (dbItem.Name == targetName) totalCount += dbItem.count;
                }
                if (totalCount < mat.count) isEnough = false;
                string color = (totalCount >= mat.count) ? "white" : "red";
                string matString = $"<color={color}>{mat.item.localizedName} ( {totalCount} / {mat.count} )</color>";
                if (i == 0) mat1Text = matString;
                else if (i == 1) mat2Text = matString;
            }
        }
        if (string.IsNullOrEmpty(mat1Text)) mat1Text = "-";
        if (string.IsNullOrEmpty(mat2Text)) mat2Text = "-";
        SetInteractable(isEnough, isEnough ? "강화하기" : "비용 부족", mat1Text, mat2Text, goldText);
    }

    private EnhancementManager.LevelInfo GetCostInfo(GearData data)
    {
        if (data.specificEnhancementSettings != null && data.specificEnhancementSettings.Length > 0)
            return data.specificEnhancementSettings[0];
        return default;
    }

    private void SetInteractable(bool interactable, string btnText, string info1, string info2, string info3)
    {
        if (enhanceButton != null) enhanceButton.interactable = interactable;
        if (buttonText != null) buttonText.text = btnText;
        if (costTexts.Length > 0) costTexts[0].text = info1;
        if (costTexts.Length > 1) costTexts[1].text = info2;
        if (costTexts.Length > 2) costTexts[2].text = info3;
    }

    private void OnClickEnhance()
    {
        if (_targetGearData == null) return; 
        var result = EnhancementManager.I.TryEnhance(_targetGearData.name, _targetGearData);
        if (result == EnhancementManager.EnhancementResult.Success)
        {
            FindObjectOfType<NotificationUI>()?.ShowMessage("강화 성공!");
            UpdateInfoUI(_targetGearData);
            if (_selectedSlotUI != null) 
                 EventSystem.current.SetSelectedGameObject(_selectedSlotUI.gameObject);
        }
        else if (result == EnhancementManager.EnhancementResult.MaxLevel)
            FindObjectOfType<NotificationUI>()?.ShowMessage("이미 강화된 장비입니다.");
        else
            FindObjectOfType<NotificationUI>()?.ShowMessage("비용이 부족합니다.");
    }
}