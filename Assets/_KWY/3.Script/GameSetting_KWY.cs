using UnityEngine;

[System.Serializable]
public class GameSetting_KWY
{
    public int resolutionIndex = -1;
    public FullScreenMode fullscreenMode = FullScreenMode.FullScreenWindow;
    public float brightness = 1.0f;

    public float masterVolume = 1.0f;
    public float bgmVolume = 1.0f;
    public float sfxVolume = 1.0f;

    public string keyBindingOverrides = "";


    public GameSetting_KWY()
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
