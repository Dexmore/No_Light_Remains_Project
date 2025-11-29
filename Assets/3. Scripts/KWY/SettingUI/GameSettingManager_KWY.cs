using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using TMPro;


public class GameSettingManager_KWY : MonoBehaviour
{
    
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Key Remapping UI")]
    [SerializeField] private GameObject keyRemappingContainer;
    [SerializeField] private KeyRemapper_KWY keyRemapperPrefab;
    

    [Header("Screen Setting")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Graphics Setting")]
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Image brightnessPanel;
    [SerializeField] private TextMeshProUGUI brightnessText;

    [Header("Sound Setting")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider bgmVolume;
    [SerializeField] private Slider sfxVolume;

    private Resolution[] resolutions;
    private List<KeyRemapper_KWY> keyRemappers = new List<KeyRemapper_KWY>();

    
    private void Awake()
    {
        LoadKeyBindingOverrides();
    }
    

    private void Start()
    {
        SetupResolutions();
        SetupKeyRemappingUI();
        LoadSettingsToUI();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        masterVolume.onValueChanged.AddListener(SetMasterVolume);
        bgmVolume.onValueChanged.AddListener(SetBGMVolume);
        sfxVolume.onValueChanged.AddListener(SetSFXVolume);
    }

    private void SetupResolutions()
    {
        float targetAspectRatio = 16f / 9f;
        resolutions = Screen.resolutions
            .Where(res => Mathf.Abs((float)res.width / res.height - targetAspectRatio) < 0.01f)
            .Distinct().ToArray();

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add($"{resolutions[i].width} x {resolutions[i].height}");
        }
        resolutionDropdown.AddOptions(options);
    }


    private void SetupKeyRemappingUI()
    {
        foreach (Transform child in keyRemappingContainer.transform)
        {
            Destroy(child.gameObject);
        }
        keyRemappers.Clear();

        var actionsToDisplay = new Dictionary<string, string>
        {
            { "Move", "이동" }, { "Jump", "점프" }, { "Attack", "공격" },
            { "LeftDash", "좌측 대시" }, { "RightDash", "우측 대시" }, { "Lantern", "랜턴" },
            { "LanternSkill", "랜턴 스킬" }, { "Parry", "패링" }, { "Potion", "물약" },
            { "Interaction", "상호작용" }, { "Inventory", "인벤토리" }
        };

        var actionMap = inputActions.FindActionMap("Player");
        if (actionMap == null)
        {
            Debug.LogError("Input Actions에서 'Player' 액션 맵을 찾을 수 없습니다.");
            return;
        }

        foreach (var actionName in actionsToDisplay.Keys)
        {
            var action = actionMap.FindAction(actionName);
            if (action == null)
            {
                Debug.LogWarning($"액션 '{actionName}'을(를) 찾을 수 없습니다.");
                continue;
            }

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];

                // [수정] isProcessing -> isProcessor
                if ((binding.isComposite && !binding.isPartOfComposite) || !string.IsNullOrEmpty(binding.processors))
                {
                    continue;
                }

                if (!binding.path.Contains("<Keyboard>") && !binding.path.Contains("<Mouse>"))
                {
                    continue;
                }

                string displayName = actionsToDisplay[actionName];
                if (binding.isPartOfComposite)
                {
                    string bindingName = binding.name.ToUpper() switch
                    {
                        "UP" => "위",
                        "DOWN" => "아래",
                        "LEFT" => "왼쪽",
                        "RIGHT" => "오른쪽",
                        _ => binding.name
                    };
                    displayName += $" ({bindingName})";
                }

                KeyRemapper_KWY remapper = Instantiate(keyRemapperPrefab, keyRemappingContainer.transform);
                remapper.Initialize(action, i, displayName);
                keyRemappers.Add(remapper);
            }
        }
    }
    

    private void LoadSettingsToUI()
    {
        GameSetting_KWY settings = GameSettingDataManager_KWY.Instance.setting;

        fullscreenToggle.isOn = settings.fullscreenMode == FullScreenMode.FullScreenWindow;
        Screen.fullScreenMode = settings.fullscreenMode;

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

        brightnessSlider.value = settings.brightness;
        SetBrightness(settings.brightness);

        masterVolume.value = settings.masterVolume;
        bgmVolume.value = settings.bgmVolume;
        sfxVolume.value = settings.sfxVolume;
        SetMasterVolume(settings.masterVolume);
        SetBGMVolume(settings.bgmVolume);
        SetSFXVolume(settings.sfxVolume);

        
        foreach (var remapper in keyRemappers)
        {
            remapper.UpdateBindingDisplay();
        }
        
    }

    
    private void LoadKeyBindingOverrides()
    {
        string overrides = GameSettingDataManager_KWY.Instance.setting.keyBindingOverrides;
        if (!string.IsNullOrEmpty(overrides))
        {
            inputActions.LoadBindingOverridesFromJson(overrides);
        }
    }

    public void OnKeyBindingChanged()
    {
        var overrides = inputActions.SaveBindingOverridesAsJson();
        GameSettingDataManager_KWY.Instance.setting.keyBindingOverrides = overrides;
    }
    
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        GameSettingDataManager_KWY.Instance.setting.resolutionIndex = resolutionIndex;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        FullScreenMode mode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreenMode = mode;
        GameSettingDataManager_KWY.Instance.setting.fullscreenMode = mode;
    }

    public void SetBrightness(float value)
    {
        if (brightnessPanel != null)
        {
            brightnessPanel.color = new Color(0, 0, 0, 1 - value);
        }

        if (brightnessText != null)
        {
            brightnessText.text = value.ToString("F2");
        }

        GameSettingDataManager_KWY.Instance.setting.brightness = value;
    }

    #region Volume Functions
    public void SetMasterVolume(float value)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        GameSettingDataManager_KWY.Instance.setting.masterVolume = value;
    }

    public void SetBGMVolume(float value)
    {
        mixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20);
        GameSettingDataManager_KWY.Instance.setting.bgmVolume = value;
    }

    public void SetSFXVolume(float value)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        GameSettingDataManager_KWY.Instance.setting.sfxVolume = value;
    }
    #endregion

    public void OnClickConfirmReset()
    {
        inputActions.RemoveAllBindingOverrides();
        GameSettingDataManager_KWY.Instance.setting.keyBindingOverrides = "";

        GameSettingDataManager_KWY.Instance.setting = new GameSetting_KWY();
        LoadSettingsToUI();
        ApplyAndSaveChanges();
    }

    public void ApplyAndSaveChanges()
    {
        GameSettingDataManager_KWY.Instance.SaveSettings();
    }
}
