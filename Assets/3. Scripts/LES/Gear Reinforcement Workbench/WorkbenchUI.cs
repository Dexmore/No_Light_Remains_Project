using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class WorkbenchUI : MonoBehaviour
{
    public System.Action OnClose;

    [Header("UI 연결")]
    [SerializeField] private GameObject panelRoot; 
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform slotContent;

    [Header("부팅 연출")]
    [SerializeField] private BootTerminal bootTerminal;       
    [SerializeField] private GameObject mainContentRoot;      

    [Header("정보 표시 (타이핑 대상)")]
    [SerializeField] private TextMeshProUGUI gearTitleText;
    [SerializeField] private TextMeshProUGUI currentEffectText;
    [SerializeField] private TextMeshProUGUI nextEffectText;
    [SerializeField] private Image targetGearImage;

    [Header("타이핑 설정")]
    [SerializeField] private float typingSpeed = 0.02f; // 글자당 속도

    [Header("강화 UI")]
    [SerializeField] private TextMeshProUGUI[] costTexts; // 0:재료1, 1:재료2, 2:골드
    [SerializeField] private Button enhanceButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private NotificationUI notificationUI;

    private GearData _targetGearData;
    private WorkbenchSlotUI _selectedSlotUI;
    private List<WorkbenchSlotUI> _preplacedSlots = new List<WorkbenchSlotUI>();

    private Dictionary<TextMeshProUGUI, Coroutine> _activeTypingCoroutines = new Dictionary<TextMeshProUGUI, Coroutine>();

    public bool IsUIActive()
    {
        if (panelRoot != null) return panelRoot.activeSelf;
        return false;
    }

    private void Start()
    {
        if(closeButton != null) closeButton.onClick.AddListener(Close);
        if(enhanceButton != null) enhanceButton.onClick.AddListener(OnClickEnhance);
        
        if(panelRoot != null) panelRoot.SetActive(false);

        if (slotContent != null)
        {
            foreach (Transform child in slotContent)
            {
                var slot = child.GetComponent<WorkbenchSlotUI>();
                if (slot != null) _preplacedSlots.Add(slot);
            }
        }
        
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnDisable()
    {
        StopAllTyping();
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (IsUIActive() && _targetGearData != null)
        {
            UpdateInfoUI(_targetGearData); 
        }
    }
    
    private void Update()
    {
        if (!IsUIActive() || Keyboard.current == null) return;
        if (mainContentRoot != null && !mainContentRoot.activeSelf) return;

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
        
        if (bootTerminal != null)
        {
            if (mainContentRoot != null) mainContentRoot.SetActive(false);
            bootTerminal.PlayBootSequence(() => 
            {
                if (mainContentRoot != null) mainContentRoot.SetActive(true);
                InitWorkbenchLogic();
            });
        }
        else
        {
            if (mainContentRoot != null) mainContentRoot.SetActive(true);
            InitWorkbenchLogic();
        }
    }
    
    private void InitWorkbenchLogic()
    {
        RefreshSlotList(); 
        ClearInfo();
        _targetGearData = null;
        StartCoroutine(SelectFirstSlot());
    }

    public void Close()
    {
        if(panelRoot != null) panelRoot.SetActive(false);
        StopAllTyping();
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
        // 1. 선택 취소(토글)
        if (_selectedSlotUI == slotUI)
        {
            slotUI.SetSelectedState(false);
            _selectedSlotUI = null;
            _targetGearData = null;
            ClearInfo(); 
            return;
        }

        // 2. 새로운 선택
        if (_selectedSlotUI != null) _selectedSlotUI.SetSelectedState(false);
        
        _selectedSlotUI = slotUI;
        _targetGearData = slotUI.Data;
        
        if (_selectedSlotUI != null) _selectedSlotUI.SetSelectedState(true);
        UpdateInfoUI(_targetGearData);
    }

    private void ClearInfo()
    {
        SetTextInstant(gearTitleText, "[ 기어를 선택하세요 ]");
        
        if (targetGearImage != null) targetGearImage.gameObject.SetActive(false);
        
        SetTextInstant(currentEffectText, "");
        SetTextInstant(nextEffectText, "");

        if (costTexts.Length > 0) SetTextInstant(costTexts[0], "-");
        if (costTexts.Length > 1) SetTextInstant(costTexts[1], "-");
        if (costTexts.Length > 2) SetTextInstant(costTexts[2], "-");

        UpdateEnhanceButtonState(false, "선택 필요");
    }

    private void UpdateInfoUI(GearData data)
    {
        if (data == null) { ClearInfo(); return; }

        SetTextTyping(gearTitleText, $"[ {data.localizedName} ]");

        if (targetGearImage != null)
        {
            targetGearImage.sprite = data.gearIcon;
            targetGearImage.gameObject.SetActive(true);
        }

        int currentLevel = DBManager.I.GetGearLevel(data.name);
        string textLv0 = data.GetEffectText(0);
        string textLv1 = data.GetEffectText(1);

        if (currentLevel >= 1)
        {
            SetTextTyping(currentEffectText, $"> {textLv1}");
            bool isKR = LocalizationSettings.SelectedLocale.Identifier.Code.Contains("ko");
            SetTextTyping(nextEffectText, isKR ? "> 강화 최종 단계" : "> Max Level Reached");
            
            if (costTexts.Length > 0) SetTextTyping(costTexts[0], "-");
            if (costTexts.Length > 1) SetTextTyping(costTexts[1], "-");
            if (costTexts.Length > 2) SetTextTyping(costTexts[2], "-");

            UpdateEnhanceButtonState(false, "강화 완료");
        }
        else
        {
            SetTextTyping(currentEffectText, $"> {textLv0}");
            SetTextTyping(nextEffectText, $"> {textLv1}");

            // [핵심 변경] 선택 여부와 관계없이 재료 정보는 항상 계산해서 보여줍니다.
            EnhancementManager.LevelInfo info = GetCostInfo(data);
            bool isSelected = (_targetGearData == data);
            
            CheckCostAndEnableButton(info, isSelected);
        }
    }

    // ================= [타이핑 효과 로직] =================

    private void SetTextTyping(TextMeshProUGUI target, string content)
    {
        if (target == null) return;
        
        // [수정됨] 이미 같은 내용이 적혀있다면 타이핑을 다시 시작하지 않음 (선택 시 재출력 방지)
        if (target.text == content) return;

        if (_activeTypingCoroutines.ContainsKey(target) && _activeTypingCoroutines[target] != null)
        {
            StopCoroutine(_activeTypingCoroutines[target]);
        }

        _activeTypingCoroutines[target] = StartCoroutine(TypewriterRoutine(target, content));
    }

    private void SetTextInstant(TextMeshProUGUI target, string content)
    {
        if (target == null) return;
        
        if (_activeTypingCoroutines.ContainsKey(target) && _activeTypingCoroutines[target] != null)
        {
            StopCoroutine(_activeTypingCoroutines[target]);
            _activeTypingCoroutines.Remove(target);
        }

        target.text = content;
        target.maxVisibleCharacters = int.MaxValue; 
    }

    private IEnumerator TypewriterRoutine(TextMeshProUGUI tmp, string content)
    {
        tmp.text = content;
        tmp.maxVisibleCharacters = 0; 
        
        tmp.ForceMeshUpdate(); 
        
        int totalVisibleCharacters = tmp.textInfo.characterCount; 
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            int visibleCount = counter % (totalVisibleCharacters + 1);
            tmp.maxVisibleCharacters = visibleCount; 
            yield return new WaitForSeconds(typingSpeed); 
            counter++;
        }
        
        tmp.maxVisibleCharacters = int.MaxValue; 
        _activeTypingCoroutines.Remove(tmp);
    }

    private void StopAllTyping()
    {
        foreach (var coroutine in _activeTypingCoroutines.Values)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        _activeTypingCoroutines.Clear();
    }

    // =======================================================

    private void CheckCostAndEnableButton(EnhancementManager.LevelInfo info, bool isSelected)
    {
        bool isEnough = true;
        if (DBManager.I.currData.gold < info.goldCost) isEnough = false;
        string goldText = $"{info.goldCost} G";
        string mat1Text = "-";
        string mat2Text = "-";

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
        
        // [변경] 프리뷰 상태여도 재료 텍스트는 보여줍니다.
        if (costTexts.Length > 0) SetTextTyping(costTexts[0], mat1Text);
        if (costTexts.Length > 1) SetTextTyping(costTexts[1], mat2Text);
        if (costTexts.Length > 2) SetTextTyping(costTexts[2], goldText);
        
        // 버튼 텍스트는 선택 여부에 따라 다르게 표시
        if (isSelected)
        {
            UpdateEnhanceButtonState(isEnough, isEnough ? "강화하기" : "비용 부족");
        }
        else
        {
            UpdateEnhanceButtonState(true, "선택(Enter)"); // 아직 선택 안 함
        }
    }

    private EnhancementManager.LevelInfo GetCostInfo(GearData data)
    {
        if (data.specificEnhancementSettings != null && data.specificEnhancementSettings.Length > 0)
            return data.specificEnhancementSettings[0];
        return default;
    }

    private void UpdateEnhanceButtonState(bool interactable, string btnText)
    {
        if (enhanceButton != null) enhanceButton.interactable = true; 
        if (buttonText != null) buttonText.text = btnText;
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