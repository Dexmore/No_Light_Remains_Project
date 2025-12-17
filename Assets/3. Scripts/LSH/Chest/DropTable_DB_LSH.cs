using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LSH/Drop Table (DB)", fileName = "DropTable_DB_LSH")]
public class DropTable_DB_LSH : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [Header("Drop Prefab")]
        public GameObject dropItemPrefab;

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
}
