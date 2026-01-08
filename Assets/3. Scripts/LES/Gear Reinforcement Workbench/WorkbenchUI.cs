using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[System.Serializable]
public class WorkbenchStringData
{
    [Header("버튼 상태 텍스트")]
    public LocalizedString btn_Enhance;
    public LocalizedString btn_NotEnough;
    public LocalizedString btn_MaxLevel;
    public LocalizedString btn_Select;
    public LocalizedString btn_SelectNeed;

    [Header("알림 메시지")]
    public LocalizedString msg_Success;
    public LocalizedString msg_AlreadyMax;
    public LocalizedString msg_NotEnough;
    public LocalizedString msg_Condition;

    [Header("기타 라벨")]
    public LocalizedString label_MaxLevelDesc;
    public LocalizedString label_EmptyTitle;
}

public class WorkbenchUI : MonoBehaviour
{
    public System.Action OnClose;

    // [튜토리얼 이벤트]
    public System.Action<GearData> OnGearSelectedEvent;
    public System.Action<EnhancementManager.EnhancementResult> OnEnhanceTryEvent;

    [Header("UI 연결")]
    [SerializeField] private GameObject panelRoot; 
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform slotContent;

    [Header("부팅 연출")]
    [SerializeField] private BootTerminal bootTerminal;       
    [SerializeField] private GameObject mainContentRoot;      

    [Header("사운드 시스템 (AudioSource 3개 필요)")]
    [SerializeField] private AudioSource sfxSource;       
    [SerializeField] private AudioSource loopSource;      
    [SerializeField] private AudioSource ambienceSource;  

    [Header("사운드 클립")]
    [SerializeField] private AudioClip bootUpClip;        
    [SerializeField] [Range(0f, 1f)] private float bootUpVolume = 1.0f;
    [SerializeField] private AudioClip ambienceClip;      
    [SerializeField] [Range(0f, 1f)] private float ambienceVolume = 0.3f;
    [SerializeField] private AudioClip dataScrollClip;    
    [SerializeField] [Range(0f, 1f)] private float dataScrollVolume = 0.5f;
    [SerializeField] private AudioClip typingLoopClip;    
    [SerializeField] [Range(0f, 1f)] private float typingVolume = 0.6f;
    [SerializeField] private AudioClip clickClip;         
    [SerializeField] [Range(0f, 1f)] private float clickVolume = 0.7f;
    [SerializeField] private AudioClip selectClip;        
    [SerializeField] [Range(0f, 1f)] private float selectVolume = 0.8f;
    [SerializeField] private AudioClip cancelClip;        
    [SerializeField] [Range(0f, 1f)] private float cancelVolume = 0.5f;
    [SerializeField] private AudioClip successClip;       
    [SerializeField] [Range(0f, 1f)] private float successVolume = 1.0f;
    [SerializeField] private AudioClip failClip;          
    [SerializeField] [Range(0f, 1f)] private float failVolume = 0.7f;
    [SerializeField] private AudioClip errorClip;         
    [SerializeField] [Range(0f, 1f)] private float errorVolume = 0.7f;
    [SerializeField] private AudioClip bootDownClip;      
    [SerializeField] [Range(0f, 1f)] private float bootDownVolume = 0.8f;

    [Header("정보 표시")]
    [SerializeField] private TextMeshProUGUI gearTitleText;
    [SerializeField] private TextMeshProUGUI currentEffectText;
    [SerializeField] private TextMeshProUGUI nextEffectText;
    [SerializeField] private Image targetGearImage;

    [Header("타이핑 설정")]
    [SerializeField] private float typingSpeed = 0.02f;

    [Header("강화 UI")]
    [SerializeField] private TextMeshProUGUI[] costTexts;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private NotificationUI notificationUI;

    [Header("로컬라이징")]
    [SerializeField] private WorkbenchStringData localizedStrings;

    private GearData _targetGearData;
    private WorkbenchSlotUI _selectedSlotUI;
    private List<WorkbenchSlotUI> _preplacedSlots = new List<WorkbenchSlotUI>();
    private Dictionary<TextMeshProUGUI, Coroutine> _activeTypingCoroutines = new Dictionary<TextMeshProUGUI, Coroutine>();

    // [튜토리얼용 변수]
    private bool _isTutorialMode = false;
    private List<GearData> _tutorialDummyGears;

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
        if (ambienceSource != null) ambienceSource.Stop();
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

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
            return;
        }

        if (mainContentRoot != null && !mainContentRoot.activeSelf) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            if (EventSystem.current.currentSelectedGameObject == enhanceButton.gameObject)
            {
                if (enhanceButton.interactable) enhanceButton.onClick.Invoke();
                else 
                {
                    notificationUI.gameObject.SetActive(true);
                    PlaySFX(failClip, failVolume);
                    notificationUI.ShowMessage(GetLocStr(localizedStrings.msg_Condition, "조건이 부족합니다."));
                }
            }
        }
    }

    public void Open()
    {
        if(panelRoot != null) panelRoot.SetActive(true);

        PlayAmbienceLoop();
        
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
        if (sfxSource != null) sfxSource.Stop();
        PlaySFX(bootDownClip, bootDownVolume);

        if (bootTerminal != null) bootTerminal.StopBootSequence();
        if(panelRoot != null) panelRoot.SetActive(false);

        StopAllTyping();
        if (ambienceSource != null) ambienceSource.Stop();

        OnClose?.Invoke();
    }

    // [튜토리얼] 시작/종료 함수
    public void BeginTutorialMode(List<GearData> dummyGears)
    {
        _isTutorialMode = true;
        _tutorialDummyGears = dummyGears;
        InitWorkbenchLogic();
    }

    public void EndTutorialMode()
    {
        _isTutorialMode = false;
        _tutorialDummyGears = null;
        InitWorkbenchLogic();
    }

    // [튜토리얼] 외부에서 UI 위치 가져오기
    public RectTransform GetSlotRect(int index)
    {
        if (index >= 0 && index < _preplacedSlots.Count)
            return _preplacedSlots[index].GetComponent<RectTransform>();
        return null;
    }
    
    public RectTransform GetEnhanceButtonRect()
    {
        return enhanceButton.GetComponent<RectTransform>();
    }

    // ================= [사운드 시스템] =================

    public void PlayBootSound() => PlaySFX(bootUpClip, bootUpVolume);
    public void PlayClickSound() => PlaySFX(clickClip, clickVolume);
    
    public void PlayDataScrollLoop() => PlayLoop(dataScrollClip, dataScrollVolume);
    public void PlayTypingLoop() => PlayLoop(typingLoopClip, typingVolume);
    
    public void StopLoopSound() 
    {
        if (_activeTypingCoroutines.Count == 0)
        {
            if (loopSource != null) loopSource.Stop();
        }
    }

    public void PauseLoopSound()
    {
        if (loopSource != null) loopSource.Pause();
    }
    public void UnPauseLoopSound()
    {
        if (loopSource != null) loopSource.UnPause();
    }

    private void PlayAmbienceLoop()
    {
        if (ambienceSource != null && ambienceClip != null)
        {
            ambienceSource.clip = ambienceClip;
            ambienceSource.loop = true;
            ambienceSource.volume = ambienceVolume;
            ambienceSource.Play();
        }
    }

    private void PlaySFX(AudioClip clip, float volume)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip, volume);
    }

    private void PlayLoop(AudioClip clip, float volume)
    {
        if (loopSource != null && clip != null)
        {
            if (loopSource.isPlaying && loopSource.clip == clip) return;
            loopSource.clip = clip;
            loopSource.loop = true;
            loopSource.volume = volume;
            loopSource.Play();
        }
    }
    // =================================================

    private void RefreshSlotList()
    {
        List<CharacterData.GearData> displayList = new List<CharacterData.GearData>();

        // [튜토리얼] 더미 데이터 처리
        if (_isTutorialMode && _tutorialDummyGears != null)
        {
            foreach(var gearSO in _tutorialDummyGears)
            {
                CharacterData.GearData dummyData = new CharacterData.GearData();
                dummyData.Name = gearSO.name;
                dummyData.level = 0;
                dummyData.isEquipped = false;
                dummyData.isNew = true;
                displayList.Add(dummyData); 
            }
        }
        else if (DBManager.I != null)
        {
            displayList = DBManager.I.currData.gearDatas;
        }

        for (int i = 0; i < _preplacedSlots.Count; i++)
        {
            _preplacedSlots[i].gameObject.SetActive(true); 
            _preplacedSlots[i].SetSelectedState(false); 

            if (i < displayList.Count)
            {
                var savedGear = displayList[i];
                GearData gearDataInfo = null;

                if (_isTutorialMode)
                {
                    gearDataInfo = _tutorialDummyGears.Find(x => x.name == savedGear.Name);
                }
                else
                {
                    gearDataInfo = DBManager.I.itemDatabase.FindGearByName(savedGear.Name);
                }

                if (gearDataInfo != null) _preplacedSlots[i].Setup(gearDataInfo, this);
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
        if (_selectedSlotUI == slotUI)
        {
            PlaySFX(cancelClip, cancelVolume);
            slotUI.SetSelectedState(false);
            _selectedSlotUI = null;
            _targetGearData = null;
            ClearInfo(); 
            return;
        }

        PlaySFX(selectClip, selectVolume);

        if (_selectedSlotUI != null) _selectedSlotUI.SetSelectedState(false);
        _selectedSlotUI = slotUI;
        _targetGearData = slotUI.Data;
        if (_selectedSlotUI != null) _selectedSlotUI.SetSelectedState(true);
        UpdateInfoUI(_targetGearData);
        
        // [튜토리얼 이벤트 호출]
        if (_selectedSlotUI != null) OnGearSelectedEvent?.Invoke(_selectedSlotUI.Data);
    }

    private void ClearInfo()
    {
        SetTextInstant(gearTitleText, GetLocStr(localizedStrings.label_EmptyTitle, "[ 기어를 선택하세요 ]"));
        if (targetGearImage != null) targetGearImage.gameObject.SetActive(false);
        SetTextInstant(currentEffectText, "");
        SetTextInstant(nextEffectText, "");
        if (costTexts.Length > 0) SetTextInstant(costTexts[0], "-");
        if (costTexts.Length > 1) SetTextInstant(costTexts[1], "-");
        if (costTexts.Length > 2) SetTextInstant(costTexts[2], "-");
        UpdateEnhanceButtonState(false, GetLocStr(localizedStrings.btn_SelectNeed, "선택 필요"));
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
        // [튜토리얼] 더미 데이터는 항상 레벨 0으로 취급 (DBManager 조회 안함)
        if (_isTutorialMode) currentLevel = 0;

        string textLv0 = data.GetEffectText(0);
        string textLv1 = data.GetEffectText(1);

        if (currentLevel >= 1)
        {
            SetTextTyping(currentEffectText, $"> {textLv1}");
            SetTextTyping(nextEffectText, $"> {GetLocStr(localizedStrings.label_MaxLevelDesc, "Max Level")}");
            if (costTexts.Length > 0) SetTextTyping(costTexts[0], "-");
            if (costTexts.Length > 1) SetTextTyping(costTexts[1], "-");
            if (costTexts.Length > 2) SetTextTyping(costTexts[2], "-");
            UpdateEnhanceButtonState(false, GetLocStr(localizedStrings.btn_MaxLevel, "강화 완료"));
        }
        else
        {
            SetTextTyping(currentEffectText, $"> {textLv0}");
            SetTextTyping(nextEffectText, $"> {textLv1}");
            EnhancementManager.LevelInfo info = GetCostInfo(data);
            bool isSelected = (_targetGearData == data);
            CheckCostAndEnableButton(info, isSelected);
        }
    }

    private void SetTextTyping(TextMeshProUGUI target, string content)
    {
        if (target == null) return;
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
        
        if (_activeTypingCoroutines.Count == 0) StopLoopSound();
    }

    private IEnumerator TypewriterRoutine(TextMeshProUGUI tmp, string content)
    {
        tmp.text = content;
        tmp.maxVisibleCharacters = 0; 
        tmp.ForceMeshUpdate(); 
        
        int totalVisibleCharacters = tmp.textInfo.characterCount; 
        int counter = 0;

        PlayTypingLoop();

        while (counter <= totalVisibleCharacters)
        {
            int visibleCount = counter;
            tmp.maxVisibleCharacters = visibleCount; 
            yield return new WaitForSeconds(typingSpeed); 
            counter++;
        }
        
        tmp.maxVisibleCharacters = int.MaxValue; 
        _activeTypingCoroutines.Remove(tmp);

        if (_activeTypingCoroutines.Count == 0)
        {
            StopLoopSound();
        }
    }

    private void StopAllTyping()
    {
        StopLoopSound(); 
        foreach (var coroutine in _activeTypingCoroutines.Values)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        _activeTypingCoroutines.Clear();
    }

    private void CheckCostAndEnableButton(EnhancementManager.LevelInfo info, bool isSelected)
    {
        bool isEnough = true;
        
        // [튜토리얼] 더미 모드에서는 재료 체크 방식을 다르게 할 수 있음 (여기선 정석대로 체크)
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
        
        if (costTexts.Length > 0) SetTextTyping(costTexts[0], mat1Text);
        if (costTexts.Length > 1) SetTextTyping(costTexts[1], mat2Text);
        if (costTexts.Length > 2) SetTextTyping(costTexts[2], goldText);
        
        if (isSelected)
        {
            string btnStr = isEnough 
                ? GetLocStr(localizedStrings.btn_Enhance, "강화하기") 
                : GetLocStr(localizedStrings.btn_NotEnough, "비용 부족");
            
            UpdateEnhanceButtonState(isEnough, btnStr);
        }
        else
        {
            UpdateEnhanceButtonState(true, GetLocStr(localizedStrings.btn_Select, "선택(Enter)")); 
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
        
        // [튜토리얼 이벤트 호출]
        OnEnhanceTryEvent?.Invoke(result);

        if (result == EnhancementManager.EnhancementResult.Success)
        {
            PlaySFX(successClip, successVolume);
            notificationUI.ShowMessage(GetLocStr(localizedStrings.msg_Success, "강화 성공!"));
            UpdateInfoUI(_targetGearData);
            if (_selectedSlotUI != null) 
                 EventSystem.current.SetSelectedGameObject(_selectedSlotUI.gameObject);
        }
        else if (result == EnhancementManager.EnhancementResult.MaxLevel)
        {
            PlaySFX(errorClip, errorVolume);
            notificationUI.ShowMessage(GetLocStr(localizedStrings.msg_AlreadyMax, "이미 강화된 장비입니다."));
        }
        else
        {
            PlaySFX(failClip, failVolume);
            notificationUI.ShowMessage(GetLocStr(localizedStrings.msg_NotEnough, "비용이 부족합니다."));
        }
    }

    private string GetLocStr(LocalizedString locString, string fallback)
    {
        if (locString == null || locString.IsEmpty) return fallback;
        return locString.GetLocalizedString();
    }

    //튜토리얼에서 재료 텍스트 위치를 알기 위한 함수
    public RectTransform GetCostTextRect(int index)
    {
        if (costTexts != null && index >= 0 && index < costTexts.Length)
        {
            return costTexts[index].GetComponent<RectTransform>();
        }
        return null;
    }

    //재료 텍스트들이 모여있는 '전체 패널' 영역을 가져오는 함수
    public RectTransform GetCostPanelRect()
    {
        if (costTexts != null && costTexts.Length > 0 && costTexts[0] != null)
        {
            // 첫 번째 텍스트의 부모(Parent)가 재료들을 담고 있는 패널이라고 가정합니다.
            return costTexts[0].transform.parent.GetComponent<RectTransform>();
        }
        return null;
    }

    //튜토리얼에서 '다시 선택하게' 만들기 위해 강제로 선택 해제하는 함수
    public void ForceDeselect()
    {
        if (_selectedSlotUI != null)
        {
            _selectedSlotUI.SetSelectedState(false);
            _selectedSlotUI = null;
        }
        _targetGearData = null;
        ClearInfo();
    }
}