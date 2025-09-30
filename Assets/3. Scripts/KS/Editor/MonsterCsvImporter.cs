// MonsterCsvImporter.cs
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class MonsterCsvImporter
{
    // CSV 파일 위치 (프로젝트 기준 경로)
    const string CSV_PATH = "Assets/Resources/Data/Monsters.csv";

    // SO 생성될 위치
    const string OUTPUT_FOLDER = "Assets/Data/MonsterData";

    [MenuItem("Tools/Import/Monsters CSV → SOs")]
    public static void Import()
    {
        if (!File.Exists(CSV_PATH))
        {
            EditorUtility.DisplayDialog("CSV 없음", $"CSV를 찾을 수 없습니다:\n{CSV_PATH}", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(OUTPUT_FOLDER))
        {
            Directory.CreateDirectory(OUTPUT_FOLDER);
            AssetDatabase.Refresh();
        }

        var text = File.ReadAllText(CSV_PATH, new UTF8Encoding(true));
        var lines = text.Replace("\r", "").Split('\n');
        if (lines.Length <= 1) { Debug.LogWarning("CSV 내용이 비어있습니다."); return; }

        var inv = CultureInfo.InvariantCulture;
        int created = 0, updated = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var cols = line.Split(',');
            if (cols.Length < 6) continue;

            string id   = cols[0];
            string name = cols[1];
            string type = cols[2];
            float move  = float.Parse(cols[3], inv);
            float atk   = float.Parse(cols[4], inv);
            float hp    = float.Parse(cols[5], inv);

            MonsterType mType = type switch {
                "Small"  => MonsterType.Small,
                "Middle" => MonsterType.Middle,
                "Large"  => MonsterType.Large,
                _        => MonsterType.Small
            };

            string assetPath = $"{OUTPUT_FOLDER}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(assetPath);
            bool isNew = false;
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<MonsterDataSO>();
                AssetDatabase.CreateAsset(so, assetPath);
                isNew = true;
            }

            so.ID = id;
            so.Name = name;
            so.Type = mType;
            so.MoveSpeed = move;
            so.Attack = atk;
            so.HP = hp;

            EditorUtility.SetDirty(so);
            if (isNew) created++; else updated++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Import 완료", $"생성 {created}, 갱신 {updated}", "OK");
        Debug.Log($"[MonsterCsvImporter] 생성 {created}, 갱신 {updated}");
    }
}
