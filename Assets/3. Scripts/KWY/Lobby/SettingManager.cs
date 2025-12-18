using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance = null;
    public SettingData setting;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
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