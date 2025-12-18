using System;
using System.Collections.Generic;
using UnityEngine;

public enum RewardType
{
    Item,
    Gear,
    Lantern,
    Record,
    Gold
}

[CreateAssetMenu(menuName = "LSH/Drop Table (DB)", fileName = "DropTable_DB_LSH")]
public class DropTable_DB_LSH : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [Header("Drop Prefab")]
        public GameObject dropItemPrefab;

        [Header("Reward Type")]
        public RewardType type;

        [Header("DB Name (비우면 prefab.name 사용)")]
        public string dbName;

        [Header("Gold Per 1 (Gold 타입일 때)")]
        public int goldPer1 = 0;

        [Header("Count Range")]
        public Vector2Int countRange = new Vector2Int(1, 1);

        [Range(0f, 1f)]
        public float probability = 1f;

        [Tooltip("true면 한 번 뽑히면 같은 엔트리는 다시 안 뽑힘")]
        public bool unique = false;

        public int RollCount()
        {
            int min = Mathf.Min(countRange.x, countRange.y);
            int max = Mathf.Max(countRange.x, countRange.y);
            return UnityEngine.Random.Range(min, max + 1);
        }

        public string GetName()
        {
            if (!string.IsNullOrWhiteSpace(dbName)) return dbName;
            return dropItemPrefab ? dropItemPrefab.name : string.Empty;
        }
    }

    public List<Entry> entries = new List<Entry>();
    public int minPicks = 1;
    public int maxPicks = 1;

    public (Entry entry, int count)[] Roll()
    {
        int pickCount = UnityEngine.Random.Range(minPicks, maxPicks + 1);
        bool[] used = new bool[entries.Count];
        var result = new List<(Entry, int)>();

        for (int i = 0; i < pickCount; i++)
        {
            int idx = PickIndex(used);
            if (idx < 0) break;

            var e = entries[idx];
            if (e == null || e.dropItemPrefab == null) continue;

            if (e.unique) used[idx] = true;
            result.Add((e, e.RollCount()));
        }

        return result.ToArray();
    }

    private int PickIndex(bool[] used)
    {
        float total = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            if (used[i]) continue;
            var e = entries[i];
            if (e == null || e.dropItemPrefab == null) continue;
            total += Mathf.Max(0f, e.probability);
        }

        if (total <= 0f) return -1;

        float r = UnityEngine.Random.value * total;
        float acc = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            if (used[i]) continue;
            var e = entries[i];
            if (e == null || e.dropItemPrefab == null) continue;

            acc += Mathf.Max(0f, e.probability);
            if (r <= acc) return i;
        }

        return -1;
    }

    public void GiveToDB()
    {
        if (DBManager.I == null)
        {
            Debug.LogError("[DropTable] DBManager.I not found!");
            return;
        }

        var results = Roll();
        foreach (var (entry, count) in results)
        {
            if (entry == null) continue;
            string name = entry.GetName();
            if (string.IsNullOrEmpty(name) && entry.type != RewardType.Gold)
            {
                Debug.LogError("[DropTable] dbName/prefab name is empty!");
                continue;
            }

            switch (entry.type)
            {
                case RewardType.Item:
                    DBManager.I.AddItem(name, count);
                    Debug.Log($"[DropTable] Item + {name} x{count}");
                    break;

                case RewardType.Gear:
                    DBManager.I.AddGear(name);
                    Debug.Log($"[DropTable] Gear + {name}");
                    break;

                case RewardType.Lantern:
                    DBManager.I.AddLantern(name);
                    Debug.Log($"[DropTable] Lantern + {name}");
                    break;

                case RewardType.Record:
                    DBManager.I.AddRecord(name);
                    Debug.Log($"[DropTable] Record + {name}");
                    break;

                case RewardType.Gold:
                    int gold = entry.goldPer1 * count;
                    DBManager.I.currData.gold += gold;
                    Debug.Log($"[DropTable] Gold + {gold}");
                    break;
            }
        }
    }
}
