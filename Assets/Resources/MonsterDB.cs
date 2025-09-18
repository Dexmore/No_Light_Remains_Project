using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterRow {
    public string ID;
    public string Name;
    public string Type;     // "Small", "Middle", "Large"
    public float MoveSpeed;
    public float Attack;
    public float HP;
}

public class MonsterDB : MonoBehaviour
{
    [Header("CSV 파일 (Resources)")]
    public TextAsset csvFile;          // Resources/Data/Monsters.csv 연결
    public List<MonsterRow> monsters = new();

    void Awake() {
        LoadCSV();
    }

    void LoadCSV()
    {
        monsters.Clear();
        string[] lines = csvFile.text.Replace("\r", "").Split('\n');

        for (int i = 1; i < lines.Length; i++) // 0번째는 헤더
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(',');
            if (cols.Length < 6) continue;

            MonsterRow m = new MonsterRow {
                ID        = cols[0],
                Name      = cols[1],
                Type      = cols[2],
                MoveSpeed = float.Parse(cols[3], System.Globalization.CultureInfo.InvariantCulture),
                Attack    = float.Parse(cols[4], System.Globalization.CultureInfo.InvariantCulture),
                HP        = float.Parse(cols[5], System.Globalization.CultureInfo.InvariantCulture)
            };
            monsters.Add(m);
        }

        Debug.Log($"CSV 로드 완료: {monsters.Count}개 몬스터");
    }
}
