using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // [필수] Input System 사용

public class WorkbenchUI : MonoBehaviour
{
    public System.Action OnClose;

    [Header("1. 패널 및 기본 설정")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;

    [Header("2. 슬롯 리스트")]
    [SerializeField] private Transform slotContent; 

    [Header("3. 중앙 정보")]
    [SerializeField] private TextMeshProUGUI gearTitleText;       
    [SerializeField] private TextMeshProUGUI currentEffectText;   
    [SerializeField] private TextMeshProUGUI nextEffectText;      
    [SerializeField] private Image targetGearImage;

    [Header("4. 재료 및 버튼")]
    [SerializeField] private TextMeshProUGUI[] costTexts; 
    [SerializeField] private Button enhanceButton; 
    [SerializeField] private TextMeshProUGUI buttonText;  

    private GearData _selectedGearData;
    private string _selectedGearName;
    private List<WorkbenchSlotUI> _preplacedSlots = new List<WorkbenchSlotUI>();

    private void Start()
    {
        if(closeButton != null) closeButton.onClick.AddListener(Close);
        if(enhanceButton != null) enhanceButton.onClick.AddListener(OnClickEnhance);
        if(panelRoot != null) panelRoot.SetActive(false);

        if (slotContent != null)
        {
            foreach (Transform child in slotContent)
            {
                var slotScript = child.GetComponent<WorkbenchSlotUI>();
                if (slotScript != null)
                {
                    _preplacedSlots.Add(slotScript);
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    private void Update()
    {
        // [수정] 구버전 Input.GetKeyDown 제거 -> 신버전 Input System 사용
        // 이 부분이 에러의 핵심 원인이었습니다.
        if (panelRoot.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void Open()
    {
        if(panelRoot != null) panelRoot.SetActive(true);
        RefreshSlotList(); 
        ClearSelection();

        // [Navigation] UI가 열릴 때 첫 번째 슬롯에 강제로 포커스를 줍니다.
        if (_preplacedSlots.Count > 0 && _preplacedSlots[0].gameObject.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(_preplacedSlots[0].gameObject);
        }
    }

    public void Close()
    {
        if(panelRoot != null) panelRoot.SetActive(false);
        // 닫힐 때 이벤트 발생 -> WorkbenchObject가 받아서 플레이어 상태 복구
        OnClose?.Invoke();
    }

    private void RefreshSlotList()
    {
        if (DBManager.I == null || DBManager.I.currData.gearDatas == null) return;

        var userGears = DBManager.I.currData.gearDatas;
        int count = Mathf.Min(userGears.Count, _preplacedSlots.Count);

        for (int i = 0; i < count; i++)
        {
            var userGear = userGears[i];
            GearData gearData = DBManager.I.itemDatabase.FindGearByName(userGear.Name);

            if (gearData != null)
            {
                _preplacedSlots[i].gameObject.SetActive(true);
                _preplacedSlots[i].Setup(gearData, userGear.Name, this);
            }
            else
            {
                _preplacedSlots[i].gameObject.SetActive(false);
            }
        }

        for (int i = count; i < _preplacedSlots.Count; i++)
            _preplacedSlots[i].gameObject.SetActive(false);
    }

    public void SelectGear(string gearName, GearData data)
    {
        _selectedGearName = gearName;
        _selectedGearData = data;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_selectedGearData == null) return;

        if (gearTitleText != null) gearTitleText.text = $"[ {_selectedGearData.localizedName} ]";
        if (targetGearImage != null)
        {
            targetGearImage.sprite = _selectedGearData.gearIcon;
            targetGearImage.gameObject.SetActive(true);
        }

        int currentLevel = DBManager.I.GetGearLevel(_selectedGearName);

        string normalText = string.IsNullOrEmpty(_selectedGearData.localizedNormalEffect) ? _selectedGearData.localizedEnhancedEffect : _selectedGearData.localizedNormalEffect;
        string enhanceText = string.IsNullOrEmpty(_selectedGearData.localizedEnhancedEffect) ? "강화 효과 없음" : _selectedGearData.localizedEnhancedEffect;

        if (currentEffectText != null) currentEffectText.text = $"> {normalText}"; 
        if (nextEffectText != null) nextEffectText.text = $"> {enhanceText}";

        if (currentLevel >= 1)
        {
            SetInteractable(false, "강화 완료", "-", "-");
        }
        else
        {
            EnhancementManager.LevelInfo info = GetCostInfo(_selectedGearData);
            
            string matStr = "";
            bool isEnough = true;

            bool enoughGold = DBManager.I.currData.gold >= info.goldCost;
            if(!enoughGold) isEnough = false;

            if (info.requiredMaterials != null)
            {
                foreach (var mat in info.requiredMaterials)
                {
                    if (mat.item == null) continue;
                    DBManager.I.HasItem(mat.item.name, out int hasCount);
                    
                    if (hasCount < mat.count) isEnough = false;
                    matStr += $"{mat.item.localizedName} ( {hasCount} / {mat.count} )\n"; 
                }
            }
            if (string.IsNullOrEmpty(matStr)) matStr = "재료 없음";

            SetInteractable(isEnough, "강화하기", matStr, $"{info.goldCost} G");
        }
    }

    private EnhancementManager.LevelInfo GetCostInfo(GearData data)
    {
        if (data.specificEnhancementSettings != null && data.specificEnhancementSettings.Length > 0)
        {
            return data.specificEnhancementSettings[0];
        }
        return default; 
    }

    private void SetInteractable(bool interactable, string btnText, string matText, string goldText)
    {
        if (enhanceButton != null) enhanceButton.interactable = interactable;
        if (buttonText != null) buttonText.text = btnText;
        if (costTexts.Length > 0) costTexts[0].text = matText;
        if (costTexts.Length > 1) costTexts[1].text = goldText;
    }

    private void ClearSelection()
    {
        _selectedGearData = null;
        if (gearTitleText != null) gearTitleText.text = "[ 선택 대기 ]";
        if (targetGearImage != null) targetGearImage.gameObject.SetActive(false);
        if (currentEffectText != null) currentEffectText.text = "";
        if (nextEffectText != null) nextEffectText.text = "";
        
        foreach (var t in costTexts) t.text = "-";
        
        if (enhanceButton != null) enhanceButton.interactable = false;
        if (buttonText != null) buttonText.text = "강화하기";
    }

    private void OnClickEnhance()
    {
        if (_selectedGearData == null) return;
        var result = EnhancementManager.I.TryEnhance(_selectedGearName, _selectedGearData);

        if (result == EnhancementManager.EnhancementResult.Success)
        {
            FindObjectOfType<NotificationUI>()?.ShowMessage("강화 성공!");
            UpdateUI(); 
            // 강화 후 버튼 비활성화로 포커스가 잃지 않도록, 현재 슬롯 다시 선택
             if (_preplacedSlots.Count > 0) 
                 EventSystem.current.SetSelectedGameObject(_preplacedSlots[0].gameObject);
        }
        else if (result == EnhancementManager.EnhancementResult.MaxLevel)
        {
            FindObjectOfType<NotificationUI>()?.ShowMessage("이미 강화된 장비입니다.");
        }
        else
        {
            FindObjectOfType<NotificationUI>()?.ShowMessage("비용이 부족합니다.");
        }
    }
}