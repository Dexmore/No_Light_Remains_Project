using System.IO;
using UnityEngine;


public class GameSettingDataManager_KWY : MonoBehaviour
{
    public static GameSettingDataManager_KWY Instance = null;

    public GameSetting_KWY setting;

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
            setting = JsonUtility.FromJson<GameSetting_KWY>(settingJson);
        }
        else
        {
            setting = new GameSetting_KWY();
        }
    }

    public void SaveSettings()
    {
        string settingJson = JsonUtility.ToJson(setting, true);
        PlayerPrefs.SetString("GameSettings", settingJson);
        PlayerPrefs.Save();
    }
}
