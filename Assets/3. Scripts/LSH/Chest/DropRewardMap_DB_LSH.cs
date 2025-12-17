using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LSH/Drop Reward Map (DB)", fileName = "DropRewardMap_DB_LSH")]
public class DropRewardMap_DB_LSH : ScriptableObject
{
    public enum RewardType
    {
        Item,
        Gear,
        Lantern,
        Record,
        Gold
    }

    [Serializable]
    public class MapEntry
    {
        public GameObject prefab;     //DropTable에 넣는 프리팹
        public RewardType type;

        [Tooltip("DBManager에서 쓰는 Name (골드면 비워도 됨)")]
        public string dbName;

        [Tooltip("Gold일 때 1개당 골드량")]
        public int goldPer1 = 0;
    }

    public List<MapEntry> maps = new List<MapEntry>();

    public bool TryGet(GameObject prefab, out MapEntry entry)
    {
        for (int i = 0; i < maps.Count; i++)
        {
            var e = maps[i];
            if (e != null && e.prefab == prefab)
            {
                entry = e;
                return true;
            }
        }
        entry = null;
        return false;
    }
}
