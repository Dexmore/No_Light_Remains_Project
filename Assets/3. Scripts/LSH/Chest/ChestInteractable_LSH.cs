using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ChestInteractable_LSH : Interactable
{
    public override Type type => Type.DropItem;
    public override bool isReady { get; set; } = true;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Drop Table")]
    [SerializeField] private DropTable_DB_LSH dropTable;

    [Header("Reward Map (Prefab -> DB info)")]
    [SerializeField] private DropRewardMap_DB_LSH rewardMap;

    [Header("Animator Trigger Name")]
    [SerializeField] private string openTrigger = "Open";

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private bool opened = false;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (opened || !isReady) return;
        if (!other.CompareTag("Player")) return;

        OpenChest();
    }

    private void OpenChest()
    {
        opened = true;
        isReady = false;

        if (animator)
        {
            animator.ResetTrigger(openTrigger);
            animator.SetTrigger(openTrigger);
        }

        GiveRewards();
    }

    private void GiveRewards()
    {
        if (dropTable == null)
        {
            Debug.LogError("[Chest] dropTable not assigned!");
            return;
        }
        if (rewardMap == null)
        {
            Debug.LogError("[Chest] rewardMap not assigned!");
            return;
        }
        if (DBManager.I == null)
        {
            Debug.LogError("[Chest] DBManager.I not found!");
            return;
        }

        var results = dropTable.Roll();

        foreach (var (entry, count) in results)
        {
            if (entry == null || entry.dropItemPrefab == null) continue;

            if (!rewardMap.TryGet(entry.dropItemPrefab, out var map))
            {
                Debug.LogError($"[Chest] RewardMap에 프리팹 매핑이 없음: {entry.dropItemPrefab.name}");
                continue;
            }

            switch (map.type)
            {
                case DropRewardMap_DB_LSH.RewardType.Item:
                    DBManager.I.AddItem(map.dbName, count);
                    if (debugLog) Debug.Log($"[Chest] Item added: {map.dbName} x{count}");
                    break;

                case DropRewardMap_DB_LSH.RewardType.Gear:
                    // 기어는 보통 개수 개념이 없으니 count 무시 (원하면 반복 AddGear)
                    DBManager.I.AddGear(map.dbName);
                    if (debugLog) Debug.Log($"[Chest] Gear added: {map.dbName}");
                    break;

                case DropRewardMap_DB_LSH.RewardType.Lantern:
                    DBManager.I.AddLantern(map.dbName);
                    if (debugLog) Debug.Log($"[Chest] Lantern added: {map.dbName}");
                    break;

                case DropRewardMap_DB_LSH.RewardType.Record:
                    DBManager.I.AddRecord(map.dbName);
                    if (debugLog) Debug.Log($"[Chest] Record added: {map.dbName}");
                    break;

                case DropRewardMap_DB_LSH.RewardType.Gold:
                    DBManager.I.currData.gold += map.goldPer1 * count;
                    if (debugLog) Debug.Log($"[Chest] Gold added: {map.goldPer1 * count}");
                    break;
            }
        }
    }
}
