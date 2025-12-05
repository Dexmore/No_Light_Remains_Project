/*
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveManager_LSH
{
    private const string SAVE_KEY = "SAVE";

    [System.Serializable]
    public class SavePos
    {
        public string scene;
        public float x;
        public float y;
    }

    // 저장
    public static void Save(Vector2 pos, string sceneName = null)
    {
        if (string.IsNullOrEmpty(sceneName))
            sceneName = SceneManager.GetActiveScene().name;

        SavePos data = new SavePos
        {
            scene = sceneName,
            x = pos.x,
            y = pos.y
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("[Save] " + json);
    }

    // 불러오기
    public static SavePos Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
            return null;

        string json = PlayerPrefs.GetString(SAVE_KEY);
        var data = JsonUtility.FromJson<SavePos>(json);
        Debug.Log("[Save] Loaded: " + json);
        return data;
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
}
*/