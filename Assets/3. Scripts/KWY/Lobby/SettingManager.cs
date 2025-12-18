using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;

public class SettingManager : SingletonBehaviour<SettingManager>
{
    protected override bool IsDontDestroy() => true;
    protected override void Awake()
    {
        base.Awake();
        LoadSettings();
        SetupResolutions();
        ApplyAllSettings();
    }
    public static SettingManager Instance = null;
    public SettingData setting;

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
    }
    [SerializeField] private AudioMixer audioMixer;
    public void ApplyAllSettings()
    {
        // 1. 해상도 및 전체화면 적용
        if (setting.resolutionIndex != -1)
        {
            Resolution res = resolutions[setting.resolutionIndex];
            Screen.SetResolution(res.width, res.height, setting.fullscreenMode);
        }
        else
        {
            Screen.fullScreenMode = setting.fullscreenMode;
        }

        // 2. 오디오 볼륨 적용 (Mixer 파라미터 이름 확인 필요)
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(0.0001f, setting.masterVolume)) * 20);
            audioMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Max(0.0001f, setting.bgmVolume)) * 20);
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(0.0001f, setting.sfxVolume)) * 20);
        }

        // 3. 언어 설정 적용 (Localization 패키지 사용 시)
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[setting.locale];
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("GameSettings"))
        {
            string settingJson = PlayerPrefs.GetString("GameSettings");
            setting = JsonUtility.FromJson<SettingData>(settingJson);
        }
        else
        {
            setting = new SettingData();
        }
    }
    public void SaveSettings()
    {
        string settingJson = JsonUtility.ToJson(setting, true);
        PlayerPrefs.SetString("GameSettings", settingJson);
        PlayerPrefs.Save();
    }



}
[System.Serializable]
public class SettingData
{
    public int resolutionIndex = -1;
    public FullScreenMode fullscreenMode = FullScreenMode.FullScreenWindow;
    public float brightness = 1.0f;
    public float masterVolume = 1.0f;
    public float bgmVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public string keyBindingOverrides = "";
    public int locale;
    public SettingData()
    {
        resolutionIndex = -1;
        fullscreenMode = FullScreenMode.FullScreenWindow;
        brightness = 1.0f;
        masterVolume = 1.0f;
        bgmVolume = 1.0f;
        sfxVolume = 1.0f;
        keyBindingOverrides = "";
    }
}