using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;          // [추가됨] Locale 클래스 사용을 위해 필수
using UnityEngine.Localization.Settings; // 언어 감지용

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

    [Header("강화 UI")]
    [SerializeField] private TextMeshProUGUI[] costTexts;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private NotificationUI notificationUI;

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
        
        // 언어 변경 감지 이벤트 연결
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (IsUIActive() && _targetGearData != null)
        {
            UpdateInfoUI(_targetGearData); // 언어 변경 시 텍스트 즉시 갱신
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
                else 
                {
                    notificationUI.gameObject.SetActive(true);
                    notificationUI.ShowMessage("조건이 부족합니다.");
                }
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
    }

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

        int currentLevel = DBManager.I.GetGearLevel(data.name);

        // [유지] GearData의 함수를 사용하여 로컬라이징(영어/한글) 완벽 동기화
        string textLv0 = data.GetEffectText(0);
        string textLv1 = data.GetEffectText(1);

        if (currentLevel >= 1)
        {
            if (currentEffectText) currentEffectText.text = $"> {textLv1}";
            
            bool isKR = LocalizationSettings.SelectedLocale.Identifier.Code.Contains("ko");
            if (nextEffectText) nextEffectText.text = isKR ? "> 강화 최종 단계" : "> Max Level Reached";
            
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
                SetInteractable(false, "선택(Enter)", "-", "-", "-");
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
        // 핵심 수정: 재료가 부족해도 알림을 띄워야 하므로 버튼은 항상 켜둡니다.
        if (enhanceButton != null) enhanceButton.interactable = true; 

        // (옵션) 만약 재료 부족 시 버튼 색을 흐리게 하고 싶다면 여기서 Image 색상을 변경하는 로직 추가 가능
        // 예: enhanceButton.image.color = isPossible ? Color.white : Color.gray;

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
            notificationUI.ShowMessage("강화 성공!");
            UpdateInfoUI(_targetGearData);
            if (_selectedSlotUI != null) 
                 EventSystem.current.SetSelectedGameObject(_selectedSlotUI.gameObject);
        }
        else if (result == EnhancementManager.EnhancementResult.MaxLevel)
            notificationUI.ShowMessage("이미 강화된 장비입니다.");
        else
            notificationUI.ShowMessage("비용이 부족합니다.");
    }
}