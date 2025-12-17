using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using TMPro;

public class LobbySettingPanel : MonoBehaviour
{
    Transform content;
    [SerializeField] InputActionAsset inputActions;
    [Header("Audio Setting")]
    [SerializeField] private AudioMixer audioMixer;
    private Scrollbar masterVolume;
    private Scrollbar bgmVolume;
    private Scrollbar sfxVolume;
    void Awake()
    {
        content = transform.Find("ScrollView/Viewport/Content");
        masterVolume = content.Find("Audio/Master").GetComponentInChildren<Scrollbar>(true);
        bgmVolume = content.Find("Audio/BGM").GetComponentInChildren<Scrollbar>(true);
        sfxVolume = content.Find("Audio/SFX").GetComponentInChildren<Scrollbar>(true);

    }
    void OnEnable()
    {
        masterVolume.onValueChanged.AddListener(SetMasterVolume);
        bgmVolume.onValueChanged.AddListener(SetBGMVolume);
        sfxVolume.onValueChanged.AddListener(SetSFXVolume);
    }
    void OnDisable()
    {
        masterVolume.onValueChanged.RemoveListener(SetMasterVolume);
        bgmVolume.onValueChanged.RemoveListener(SetBGMVolume);
        sfxVolume.onValueChanged.RemoveListener(SetSFXVolume);
    }
    private IEnumerator Start()
    {
        yield return null;
        // SetupResolutions();
        // SetupKeyRemappingUI();
        LoadSettingsToUI();
        // resolutionDropdown.onValueChanged.AddListener(SetResolution);
        // fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        // brightnessSlider.onValueChanged.AddListener(SetBrightness);
        masterVolume.onValueChanged.AddListener(SetMasterVolume);
        bgmVolume.onValueChanged.AddListener(SetBGMVolume);
        sfxVolume.onValueChanged.AddListener(SetSFXVolume);
        yield return YieldInstructionCache.WaitForSeconds(0.5f);
        //brightnessPanel = GameManager.I.transform.Find("BrightnessCanvas").GetComponentInChildren<Image>();
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

    private void LoadSettingsToUI()
    {
        SettingData settings = SettingManager.Instance.setting;
        masterVolume.value = settings.masterVolume;
        bgmVolume.value = settings.bgmVolume;
        sfxVolume.value = settings.sfxVolume;
        SetMasterVolume(settings.masterVolume);
        SetBGMVolume(settings.bgmVolume);
        SetSFXVolume(settings.sfxVolume);

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
        // float loadedBrightness = Mathf.Max(settings.brightness, MIN_BRIGHTNESS);
        // brightnessSlider.value = loadedBrightness;
        // SetBrightness(loadedBrightness);

        // foreach (var remapper in keyRemappers)
        // {
        //     remapper.UpdateBindingDisplay();
        // }

    }
    public void OnClickConfirmReset()
    {
        inputActions.RemoveAllBindingOverrides();
        SettingManager.Instance.setting.keyBindingOverrides = "";
        SettingManager.Instance.setting = new SettingData();
        LoadSettingsToUI();
        ApplyAndSaveChanges();
    }
    public void ClickSound()
    {
        AudioManager.I.PlaySFX("UIClick");
    }

    public void ApplyAndSaveChanges()
    {
        SettingManager.Instance.SaveSettings();
    }










}
// {
//     [Header("UI Panels")]
//     [SerializeField] private GameObject basicSettingPanel;
//     [SerializeField] private GameObject keySettingPanel;

//     [Header("Input Actions")]
//     [SerializeField] private InputActionAsset inputActions;

//     [Header("Key Remapping UI")]
//     [SerializeField] private GameObject keyRemappingContainerLeft;
//     [SerializeField] private GameObject keyRemappingContainerRight;
//     [SerializeField] private KeyRemapper_KWY keyRemapperPrefab;

//     [Header("Screen Setting")]
//     [SerializeField] private TMP_Dropdown resolutionDropdown;
//     [SerializeField] private Toggle fullscreenToggle;

//     [Header("Graphics Setting")]
//     [SerializeField] private Slider brightnessSlider;
//     [SerializeField] private Image brightnessPanel;
//     [SerializeField] private TextMeshProUGUI brightnessText;

//     [Header("Sound Setting")]
//     [SerializeField] private AudioMixer mixer;
//     [SerializeField] private Slider masterVolume;
//     [SerializeField] private Slider bgmVolume;
//     [SerializeField] private Slider sfxVolume;

//     private const float MIN_BRIGHTNESS = 0.1f;
//     private Resolution[] resolutions;
//     private List<KeyRemapper_KWY> keyRemappers = new List<KeyRemapper_KWY>();


//     private void Awake()
//     {
//         LoadKeyBindingOverrides();
//     }

//     private void OnEnable()
//     {
//         if (basicSettingPanel != null)
//             basicSettingPanel.SetActive(true);

//         if (keySettingPanel != null)
//             keySettingPanel.SetActive(false);
//     }

//     private IEnumerator Start()
//     {
//         yield return null;
//         SetupResolutions();
//         SetupKeyRemappingUI();
//         LoadSettingsToUI();
//         resolutionDropdown.onValueChanged.AddListener(SetResolution);
//         fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
//         brightnessSlider.onValueChanged.AddListener(SetBrightness);
//         masterVolume.onValueChanged.AddListener(SetMasterVolume);
//         bgmVolume.onValueChanged.AddListener(SetBGMVolume);
//         sfxVolume.onValueChanged.AddListener(SetSFXVolume);
//         yield return YieldInstructionCache.WaitForSeconds(0.5f);
//         brightnessPanel = GameManager.I.transform.Find("BrightnessCanvas").GetComponentInChildren<Image>();
//     }

//     public bool OnEscPressed()
//     {
//         if (keySettingPanel != null && keySettingPanel.activeSelf)
//         {
//             CancelKeysAndClosePanel();
//             return true;
//         }

//         return false;
//     }

//     public void OpenKeySettingPanel()
//     {
//         if (basicSettingPanel != null) 
//             basicSettingPanel.SetActive(false);

//         if (keySettingPanel != null) 
//             keySettingPanel.SetActive(true);
//     }

//     public void ApplyKeysAndClosePanel()
//     {
//         ApplyAndSaveChanges();
//         CloseKeySettingPanel();
//     }

//     public void CancelKeysAndClosePanel()
//     {
//         LoadKeyBindingOverrides();
//         foreach (var remapper in keyRemappers)
//         {
//             remapper.UpdateBindingDisplay();
//         }

//         CloseKeySettingPanel();
//     }

//     public void CloseKeySettingPanel()
//     {
//         if (basicSettingPanel != null) 
//             basicSettingPanel.SetActive(true);

//         if (keySettingPanel != null) 
//             keySettingPanel.SetActive(false);
//     }

//     public void OnClickResetKeysOnly()
//     {
//         inputActions.RemoveAllBindingOverrides();
//         SettingManager.Instance.setting.keyBindingOverrides = "";

//         foreach (var remapper in keyRemappers)
//         {
//             remapper.UpdateBindingDisplay();
//         }
//     }

//     private void SetupResolutions()
//     {
//         float targetAspectRatio = 16f / 9f;
//         resolutions = Screen.resolutions
//             .Where(res => Mathf.Abs((float)res.width / res.height - targetAspectRatio) < 0.01f)
//             .Distinct().ToArray();

//         resolutionDropdown.ClearOptions();
//         List<string> options = new List<string>();
//         for (int i = 0; i < resolutions.Length; i++)
//         {
//             options.Add($"{resolutions[i].width} x {resolutions[i].height}");
//         }
//         resolutionDropdown.AddOptions(options);
//     }

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

//     public void SetResolution(int resolutionIndex)
//     {
//         Resolution resolution = resolutions[resolutionIndex];
//         Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
//         SettingManager.Instance.setting.resolutionIndex = resolutionIndex;
//     }

//     public void SetFullscreen(bool isFullscreen)
//     {
//         FullScreenMode mode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
//         Screen.fullScreenMode = mode;
//         SettingManager.Instance.setting.fullscreenMode = mode;
//     }

//     public void SetBrightness(float value)
//     {
//         if (brightnessPanel != null)
//         {
//             brightnessPanel.color = new Color(0, 0, 0, 1 - value);
//         }

//         if (brightnessText != null)
//         {
//             brightnessText.text = value.ToString("F2");
//         }

//         SettingManager.Instance.setting.brightness = value;
//     }


// }
