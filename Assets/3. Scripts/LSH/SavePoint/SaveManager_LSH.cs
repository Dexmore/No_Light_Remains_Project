using UnityEngine;

[System.Serializable]
public class SavePos
{
    public string scene;
    public float x;
    public float y;

    public Vector2 Position => new Vector2(x, y);
}

public class SaveManager_LSH : MonoBehaviour
{
    private const string SAVE_KEY = "SAVE";

    public static void Save(string sceneName, Vector2 pos)
    {
        SavePos data = new SavePos
        {
            scene = sceneName,
            x = pos.x,
            y = pos.y
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("[Save] Saved: " + json);
    }

    public static SavePos Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
            return default(SavePos);

        string json = PlayerPrefs.GetString(SAVE_KEY);
        SavePos data = JsonUtility.FromJson<SavePos>(json);

        Debug.Log("[Save] Loaded: " + json);
        return data;
    }
}
