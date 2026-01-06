using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Localization.Settings;
using NUnit.Framework.Interfaces;

public class LobbySettingPanel : MonoBehaviour
{
    Transform content;
    [SerializeField] InputActionAsset inputActions;
    [Header("Audio Setting")]
    [SerializeField] private AudioMixer audioMixer;
    private Scrollbar masterVolume;
    private Scrollbar bgmVolume;
    private Scrollbar sfxVolume;
    [HideInInspector] public Image brightnessPanel;
    private Scrollbar brightnessSlider;
    private const float MIN_BRIGHTNESS = 0.06f;
    private TMP_Dropdown localeDropdown;
    private Button[] keymapButtons;
    private Toggle fullscreenToggle;
    private TMP_Dropdown resolutionDropdown;
    TMP_Text tipText;
    void Awake()
    {
        content = transform.Find("ScrollView/Viewport/Content");
        masterVolume = content.Find("Audio/Master").GetComponentInChildren<Scrollbar>(true);
        bgmVolume = content.Find("Audio/BGM").GetComponentInChildren<Scrollbar>(true);
        sfxVolume = content.Find("Audio/SFX").GetComponentInChildren<Scrollbar>(true);
        brightnessSlider = content.Find("Screen/Brightness").GetComponentInChildren<Scrollbar>(true);
        localeDropdown = content.Find("Language").GetComponentInChildren<TMP_Dropdown>(true);
        keymapButtons = content.Find("Keymap").GetComponentsInChildren<Button>(true);
        fullscreenToggle = content.Find("Screen/FullScreen").GetComponentInChildren<Toggle>(true);
        resolutionDropdown = content.Find("Screen/Resolution").GetComponentInChildren<TMP_Dropdown>(true);
        tipText = content.Find("Screen/TipText(TMP)").GetComponent<TMP_Text>();
    }
    void OnEnable()
    {
        masterVolume.onValueChanged.AddListener(SetMasterVolume);
        bgmVolume.onValueChanged.AddListener(SetBGMVolume);
        sfxVolume.onValueChanged.AddListener(SetSFXVolume);
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        localeDropdown.onValueChanged.AddListener(SetLocale);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        StartCoroutine(nameof(Init));
    }
    void OnDisable()
    {
        masterVolume.onValueChanged.RemoveListener(SetMasterVolume);
        bgmVolume.onValueChanged.RemoveListener(SetBGMVolume);
        sfxVolume.onValueChanged.RemoveListener(SetSFXVolume);
        brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
        localeDropdown.onValueChanged.RemoveListener(SetLocale);
        fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
        resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
    }
    private IEnumerator Init()
    {
        yield return null;
        LoadSettingsToUI();
        Scrollbar scrollbar = transform.Find("ScrollView/Scrollbar Vertical").GetComponent<Scrollbar>();
        scrollbar.value = 1f;
        yield return YieldInstructionCache.WaitForSeconds(0.3f);
        if (brightnessPanel == null)
            brightnessPanel = GameManager.I.transform.Find("BrightnessCanvas").GetComponentInChildren<Image>();
        
    }
    public void ApplyAndSaveChanges()
    {
        SettingManager.I.SaveSettings();
    }
    public void OnClickConfirmReset()
    {
        inputActions.RemoveAllBindingOverrides();
        SettingManager.I.setting.keyBindingOverrides = "";
        SettingManager.I.setting = new SettingData();
        LoadSettingsToUI();
        ApplyAndSaveChanges();
        SettingManager.I.ApplyAllSettings();
    }
    public void LoadSettingsToUI()
    {
        SetupResolutions();
        SettingData settings = SettingManager.I.setting;
        tipText.color = new Color(tipText.color.r, tipText.color.g, tipText.color.b , 0.5f * Mathf.Clamp01(settings.brightness - 0.75f));
        SetMasterVolume(settings.masterVolume);
        SetBGMVolume(settings.bgmVolume);
        SetSFXVolume(settings.sfxVolume);
        float loadedBrightness = Mathf.Max(settings.brightness, MIN_BRIGHTNESS);
        SetBrightness(loadedBrightness);
        SetLocale(settings.locale);
        // int savedResolutionIndex = settings.resolutionIndex;
        // if (savedResolutionIndex >= resolutions.Length || savedResolutionIndex < 0)
        //     savedResolutionIndex = 0;
        // if (resolutions.Length > 0)
        // {
        //     var resolution = resolutions[savedResolutionIndex];
        //     Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        // }
        // Screen.fullScreenMode = settings.fullscreenMode;
        UpdateKeymapTexts();


        //////////////////////


        if (masterVolume)
            masterVolume.value = settings.masterVolume;
        if (bgmVolume)
            bgmVolume.value = settings.bgmVolume;
        if (sfxVolume)
            sfxVolume.value = settings.sfxVolume;
        if (brightnessSlider)
            brightnessSlider.value = loadedBrightness;
        if (localeDropdown)
        {
            int currentLocaleIndex = 0;
            var selectedLocale = LocalizationSettings.SelectedLocale;
            var locales = LocalizationSettings.AvailableLocales.Locales;
            for (int i = 0; i < locales.Count; i++)
            {
                if (locales[i] == selectedLocale)
                {
                    currentLocaleIndex = i;
                    break;
                }
            }
            localeDropdown.value = currentLocaleIndex;
        }
        if (fullscreenToggle)
        {
            fullscreenToggle.isOn = settings.fullscreenMode == FullScreenMode.FullScreenWindow;
        }
        if (resolutionDropdown)
        {
            int savedResolutionIndex = settings.resolutionIndex;
            if (savedResolutionIndex != -1 && savedResolutionIndex < resolutions.Length)
            {
                resolutionDropdown.value = savedResolutionIndex;
            }
            else
            {
                int currentResIndex = resolutions.ToList().FindIndex(res => res.width == Screen.width && res.height == Screen.height);
                if (currentResIndex != -1) resolutionDropdown.value = currentResIndex;
            }
            resolutionDropdown.RefreshShownValue();
        }
        //////////////////////

        ResetColor();
        currentKeymapButton = -1;


    }

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        SettingManager.I.setting.masterVolume = value;
    }

    public void SetBGMVolume(float value)
    {
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20);
        SettingManager.I.setting.bgmVolume = value;
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20 + 12);
        SettingManager.I.setting.sfxVolume = value;
    }
    public void ClickSound()
    {
        AudioManager.I.PlaySFX("UIClick");
    }
    public void SetBrightness(float value)
    {
        if (brightnessPanel != null)
        {
            float _value = Mathf.Clamp(1 - value, 0, 1 - MIN_BRIGHTNESS);
            float alpha = Mathf.Lerp(0f, 0.66511f, _value);
            brightnessPanel.color = new Color(0, 0, 0, alpha);
            tipText.color = new Color(tipText.color.r, tipText.color.g, tipText.color.b , 0.5f * Mathf.Clamp01(value - 0.75f));
        }
        SettingManager.I.setting.brightness = Mathf.Clamp(value, MIN_BRIGHTNESS, 1f);
    }

    // 언어 드롭다운에서 선택 시 호출될 함수
    private bool isLocalDropdownActive = false;
    public void SetLocale(int localeID)
    {
        if (isLocalDropdownActive) return;
        _ = SetLocaleAsync(localeID);
    }
    async Task SetLocaleAsync(int localeID)
    {
        try
        {
            isLocalDropdownActive = true;
            // 로컬라이제이션 시스템 초기화 대기
            // OperationHandle을 Task로 변환하여 await 합니다.
            await LocalizationSettings.InitializationOperation.Task;
            // 유효한 인덱스인지 확인 (방어적 프로그래밍)
            var locales = LocalizationSettings.AvailableLocales.Locales;
            if (localeID >= 0 && localeID < locales.Count)
            {
                LocalizationSettings.SelectedLocale = locales[localeID];
            }
            SettingManager.I.setting.locale = localeID;
        }
        finally
        {
            // 예외가 발생하더라도 다시 드롭다운을 사용할 수 있도록 false 처리
            isLocalDropdownActive = false;
        }
    }
    public void SetFullscreen(bool isFullscreen)
    {
        FullScreenMode mode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreenMode = mode;
        SettingManager.I.setting.fullscreenMode = mode;
    }
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        SettingManager.I.setting.resolutionIndex = resolutionIndex;
    }
    Resolution[] resolutions = new Resolution[0];
    private void SetupResolutions()
    {
        float targetAspectRatio = 16f / 9f;
        var filteredResolutions = Screen.resolutions
            .Where(res => Mathf.Abs((float)res.width / res.height - targetAspectRatio) < 0.01f)
            .Select(res => new { res.width, res.height }) // 해상도 수치만 뽑음
            .Distinct() // 중복된 해상도 제거
            .OrderByDescending(res => res.width) // 가로 너비 기준 내림차순 (고해상도가 위로)
            .ToList();
        resolutions = new Resolution[filteredResolutions.Count];
        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            var resData = filteredResolutions[i];
            resolutions[i] = Screen.resolutions.First(r => r.width == resData.width && r.height == resData.height);
            options.Add($"{resData.width} x {resData.height}");
        }
        if (resolutionDropdown)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
        }
    }
    #region KeyMap
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    private Color buttonColor = new Color(0.066f, 0.066f, 0.066f, 1f);
    private Color selectedButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    private int currentKeymapButton = -1;

    public void KeymapButton(int index)
    {
        ResetColor();
        currentKeymapButton = index;

        if (keymapButtons != null && index < keymapButtons.Length)
        {
            Image image = keymapButtons[index].GetComponent<Image>();
            image.color = selectedButtonColor;
            StartRebinding(index);
        }
    }
    private void ResetColor()
    {
        if (keymapButtons == null || keymapButtons.Length == 0)
        {
            content = transform.Find("ScrollView/Viewport/Content");
            keymapButtons = content.Find("Keymap").GetComponentsInChildren<Button>(true);
        }
        foreach (var btn in keymapButtons)
        {
            if (btn != null) btn.GetComponent<Image>().color = buttonColor;
        }
    }
    private void StartRebinding(int index)
    {
        string[] actionNames = {
            "Move", "Move", "Attack", "Jump", "Lantern",
            "Parry", "Potion", "Interaction", "Inventory", "Move"
        };

        var action = inputActions.FindAction(actionNames[index]);
        if (action == null) return;

        // [중요] 액션이 활성화되어 있다면 리바인딩 전에 꺼야 합니다.
        bool wasEnabled = action.enabled;
        if (wasEnabled) action.Disable();

        rebindingOperation?.Cancel();
        CleanUpOperation();

        var buttonText = keymapButtons[index].GetComponentInChildren<TextMeshProUGUI>();
        string originalText = buttonText.text;
        buttonText.text = "...";

        int bindingIndex = 0;
        if (action.name == "Move")
        {
            if (index == 0)
            {
                bindingIndex = 3;
            }
            else if (index == 1)
            {
                bindingIndex = 4;
            }
            else if (index == 9)
            {
                bindingIndex = 2;
            }
        }
        else
        {
            bindingIndex = action.bindings.ToList().FindIndex(b => !b.isComposite);
        }

        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/delta")
            .WithControlsExcluding("<Pointer>/position")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                // 1. UI 텍스트 업데이트 (화살표 변환 포함)
                buttonText.text = GetReadableKeyName(action, bindingIndex);

                // 2. 현재 새롭게 설정된 키의 경로(Path) 저장
                string newPath = action.bindings[bindingIndex].effectivePath;

                // 3. Dash 액션 동기화 (Move 변경 시)
                if (action.name == "Move")
                {
                    string dashActionName = (index == 0) ? "LeftDash" : "RightDash";
                    var dashAction = inputActions.FindAction(dashActionName);

                    if (dashAction != null)
                    {
                        bool dashWasEnabled = dashAction.enabled;
                        if (dashWasEnabled) dashAction.Disable();

                        // 이동 키와 동일한 경로로 대시 키 덮어쓰기
                        dashAction.ApplyBindingOverride(0, newPath);

                        if (dashWasEnabled) dashAction.Enable();
                    }
                }

                // 4. LanternInteraction 액션 동기화 (Lantern 변경 시)
                if (action.name == "Lantern")
                {
                    string targetName = "LanternInteraction";
                    var interactionAction = inputActions.FindAction(targetName);

                    if (interactionAction != null)
                    {
                        bool interactionWasEnabled = interactionAction.enabled;
                        if (interactionWasEnabled) interactionAction.Disable();

                        // 랜턴 키와 동일한 경로로 랜턴 상호작용 키 덮어쓰기
                        interactionAction.ApplyBindingOverride(0, newPath);

                        if (interactionWasEnabled) interactionAction.Enable();
                    }
                }

                // [중요] 메인 액션 다시 활성화
                if (wasEnabled) action.Enable();

                OnKeyBindingChanged(); // 변경 사항 JSON 저장
                CleanUpOperation();
                ResetColor();
            })
            .OnCancel(operation =>
            {
                buttonText.text = originalText;

                // [중요] 취소 시에도 액션 다시 활성화
                if (wasEnabled) action.Enable();

                CleanUpOperation();
                ResetColor();
            });

        rebindingOperation.Start();
    }

    private void CleanUpOperation()
    {
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
            rebindingOperation = null;
        }
    }

    public void OnKeyBindingChanged()
    {
        var overrides = inputActions.SaveBindingOverridesAsJson();
        SettingManager.I.setting.keyBindingOverrides = overrides;
        // 필요 시 여기서 즉시 파일 저장 가능
        // SettingManager.I.SaveSettings();
    }

    public void UpdateKeymapTexts()
    {
        if (keymapButtons == null || keymapButtons.Length == 0) return;
        string[] actionNames = { "Move", "Move", "Attack", "Jump", "Lantern", "Parry", "Potion", "Interaction", "Inventory", "Move" };

        for (int i = 0; i < keymapButtons.Length; i++)
        {
            var action = inputActions.FindAction(actionNames[i]);
            if (action == null) continue;

            int bIndex = 0;
            if (action.name == "Move")
            {
                if (i == 0)
                {
                    bIndex = 3;
                }
                else if (i == 1)
                {
                    bIndex = 4;
                }
                else if (i == 9)
                {
                    bIndex = 2;
                }
            }
            else
            {
                bIndex = action.bindings.ToList().FindIndex(b => !b.isComposite);
            }

            var txt = keymapButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = GetReadableKeyName(action, bIndex);
            }
        }
    }

    private string GetReadableKeyName(InputAction action, int bindingIndex)
    {
        // 입력 장치 이름을 제외한 키 이름 가져오기
        string keyName = InputControlPath.ToHumanReadableString(
            action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        // 방향키 특수문자 치환
        return keyName switch
        {
            "Left Arrow" or "LeftArrow" => "←",
            "Right Arrow" or "RightArrow" => "→",
            "Up Arrow" or "UpArrow" => "↑",
            "Down Arrow" or "DownArrow" => "↓",
            _ => keyName
        };
    }
    #endregion




}



