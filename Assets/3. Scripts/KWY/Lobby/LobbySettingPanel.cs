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

    void Awake()
    {
        content = transform.Find("ScrollView/Viewport/Content");
        masterVolume = content.Find("Audio/Master").GetComponentInChildren<Scrollbar>(true);
        bgmVolume = content.Find("Audio/BGM").GetComponentInChildren<Scrollbar>(true);
        sfxVolume = content.Find("Audio/SFX").GetComponentInChildren<Scrollbar>(true);
        brightnessSlider = content.Find("Screen/Brightness").GetComponentInChildren<Scrollbar>(true);
        localeDropdown = content.Find("Language").GetComponentInChildren<TMP_Dropdown>(true);
        keymapButtons = content.Find("Keymap").GetComponentsInChildren<Button>(true);
    }
    void OnEnable()
    {
        masterVolume.onValueChanged.AddListener(SetMasterVolume);
        bgmVolume.onValueChanged.AddListener(SetBGMVolume);
        sfxVolume.onValueChanged.AddListener(SetSFXVolume);
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        localeDropdown.onValueChanged.AddListener(SetLocale);
        // resolutionDropdown.onValueChanged.AddListener(SetResolution);
        // fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }
    void OnDisable()
    {
        masterVolume.onValueChanged.RemoveListener(SetMasterVolume);
        bgmVolume.onValueChanged.RemoveListener(SetBGMVolume);
        sfxVolume.onValueChanged.RemoveListener(SetSFXVolume);
        brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
        localeDropdown.onValueChanged.RemoveListener(SetLocale);
    }
    private IEnumerator Start()
    {
        yield return null;
        // SetupResolutions();
        // SetupKeyRemappingUI();
        LoadSettingsToUI();

        yield return YieldInstructionCache.WaitForSeconds(0.3f);
        if (brightnessPanel == null)
            brightnessPanel = GameManager.I.transform.Find("BrightnessCanvas").GetComponentInChildren<Image>();
    }
    public void ApplyAndSaveChanges()
    {
        SettingManager.Instance.SaveSettings();
    }
    public void OnClickConfirmReset()
    {
        inputActions.RemoveAllBindingOverrides();
        SettingManager.Instance.setting.keyBindingOverrides = "";
        SettingManager.Instance.setting = new SettingData();
        LoadSettingsToUI();
        ApplyAndSaveChanges();
    }
    public void LoadSettingsToUI()
    {
        SettingData settings = SettingManager.Instance.setting;
        SetMasterVolume(settings.masterVolume);
        SetBGMVolume(settings.bgmVolume);
        SetSFXVolume(settings.sfxVolume);
        float loadedBrightness = Mathf.Max(settings.brightness, MIN_BRIGHTNESS);
        SetBrightness(loadedBrightness);
        SetLocale(settings.locale);

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

        ResetButtonsColor();
        currentKeymapButton = -1;



        // fullscreenToggle.isOn = settings.fullscreenMode == FullScreenMode.FullScreenWindow;
        // Screen.fullScreenMode = settings.fullscreenMode;
        // int savedResolutionIndex = settings.resolutionIndex;
        // if (savedResolutionIndex != -1 && savedResolutionIndex < resolutions.Length)
        // {
        //     resolutionDropdown.value = savedResolutionIndex;
        // }
        // else
        // {
        //     int currentResIndex = resolutions.ToList().FindIndex(res => res.width == Screen.width && res.height == Screen.height);
        //     if (currentResIndex != -1) resolutionDropdown.value = currentResIndex;
        // }
        // resolutionDropdown.RefreshShownValue();
        // 
        // 
        // 

        // foreach (var remapper in keyRemappers)
        // {
        //     remapper.UpdateBindingDisplay();
        // }

    }

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        SettingManager.Instance.setting.masterVolume = value;
    }

    public void SetBGMVolume(float value)
    {
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20);
        SettingManager.Instance.setting.bgmVolume = value;
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        SettingManager.Instance.setting.sfxVolume = value;
    }
    public void ClickSound()
    {
        AudioManager.I.PlaySFX("UIClick");
    }
    public void SetBrightness(float value)
    {
        if (brightnessPanel != null)
        {
            brightnessPanel.color = new Color(0, 0, 0, Mathf.Clamp(1 - value, 0, 1 - MIN_BRIGHTNESS));
        }
        SettingManager.Instance.setting.brightness = Mathf.Clamp(value, MIN_BRIGHTNESS, 1f);
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
        }
        finally
        {
            // 예외가 발생하더라도 다시 드롭다운을 사용할 수 있도록 false 처리
            isLocalDropdownActive = false;
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        //Resolution resolution = resolutions[resolutionIndex];

        //Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        SettingManager.Instance.setting.resolutionIndex = resolutionIndex;
    }
    private void SetupResolutions()
    {
        // float targetAspectRatio = 16f / 9f;
        // resolutions = Screen.resolutions
        //     .Where(res => Mathf.Abs((float)res.width / res.height - targetAspectRatio) < 0.01f)
        //     .Distinct().ToArray();

        // resolutionDropdown.ClearOptions();
        // List<string> options = new List<string>();
        // for (int i = 0; i < resolutions.Length; i++)
        // {
        //     options.Add($"{resolutions[i].width} x {resolutions[i].height}");
        // }
        // resolutionDropdown.AddOptions(options);
    }
    public void SetFullscreen(bool isFullscreen)
    {
        FullScreenMode mode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreenMode = mode;
        SettingManager.Instance.setting.fullscreenMode = mode;
    }














    //private List<KeyRemapper_KWY> keyRemappers = new List<KeyRemapper_KWY>();
    Color buttonColor = new Color(0.066f, 0.066f, 0.066f, 1f);
    Color selectedButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    int currentKeymapButton = -1;
    public void KeymapButton(int index)
    {
        ResetButtonsColor();
        currentKeymapButton = index;
        Image image = keymapButtons[index].GetComponent<Image>();
        image.color = selectedButtonColor;
    }
    void ResetButtonsColor()
    {
        for (int i = 0; i < keymapButtons.Length; i++)
        {
            Image image = keymapButtons[i].GetComponent<Image>();
            image.color = buttonColor;
        }
    }







}





//     private void SetupKeyRemappingUI()
//     {
//         foreach (Transform child in keyRemappingContainerLeft.transform) { Destroy(child.gameObject); }
//         foreach (Transform child in keyRemappingContainerRight.transform) { Destroy(child.gameObject); }
//         keyRemappers.Clear();

//         var actionsLeft = new Dictionary<string, string>
//         {
//             { "Move", "이동" },
//             { "Jump", "점프" },
//             { "Attack", "공격" }
//         };

//         var actionsRight = new Dictionary<string, string>
//         {
//             { "Lantern", "랜턴" },
//             { "Potion", "물약" },
//             { "Interaction", "상호작용" },
//             { "Inventory", "인벤토리" }
//         };

//         var actionMap = inputActions.FindActionMap("Player");
//         if (actionMap == null) { Debug.LogError("Input Actions에서 'Player' 액션 맵을 찾을 수 없습니다."); return; }

//         PopulateKeyRemappingPanel(actionMap, actionsLeft, keyRemappingContainerLeft);
//         PopulateKeyRemappingPanel(actionMap, actionsRight, keyRemappingContainerRight);
//     }

//     private void PopulateKeyRemappingPanel(InputActionMap actionMap, Dictionary<string, string> actionsToDisplay, GameObject container)
//     {
//         foreach (var actionName in actionsToDisplay.Keys)
//         {
//             var action = actionMap.FindAction(actionName);
//             if (action == null) { Debug.LogWarning($"액션 '{actionName}'을(를) 찾을 수 없습니다."); continue; }

//             for (int i = 0; i < action.bindings.Count; i++)
//             {
//                 var binding = action.bindings[i];
//                 if ((binding.isComposite && !binding.isPartOfComposite) || !string.IsNullOrEmpty(binding.processors)) { continue; }
//                 if (!binding.path.Contains("<Keyboard>") && !binding.path.Contains("<Mouse>")) { continue; }

//                 string displayName = actionsToDisplay[actionName];
//                 if (binding.isPartOfComposite)
//                 {
//                     string bindingName = binding.name.ToUpper() switch { "UP" => "위", "DOWN" => "아래", "LEFT" => "왼쪽", "RIGHT" => "오른쪽", _ => binding.name };
//                     displayName += $" ({bindingName})";
//                 }

//                 KeyRemapper_KWY remapper = Instantiate(keyRemapperPrefab, container.transform);
//                 remapper.Initialize(action, i, displayName);
//                 keyRemappers.Add(remapper);
//             }
//         }
//     }



//     private void LoadKeyBindingOverrides()
//     {
//         string overrides = SettingManager.Instance.setting.keyBindingOverrides;
//         if (!string.IsNullOrEmpty(overrides))
//         {
//             inputActions.LoadBindingOverridesFromJson(overrides);
//         }
//     }

//     public void OnKeyBindingChanged()
//     {
//         var overrides = inputActions.SaveBindingOverridesAsJson();
//         SettingManager.Instance.setting.keyBindingOverrides = overrides;
//     }


