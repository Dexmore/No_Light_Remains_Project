using UnityEngine;
using NaughtyAttributes;
using System.Threading.Tasks;
[RequireComponent(typeof(Collider2D))]
public class ChestInteractable_LSH : Interactable, ISavable
{
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete;
    bool ISavable.CanReplay => canReplay;
    int ISavable.ReplayWaitTimeSecond => replayWaitTimeSecond;
    public void SetCompletedImmediately()
    {
        isComplete = true;
        col.enabled = false;
        animator.Play("Empty");
    }
    #endregion
    public override Type type => Type.DropItem;
    public override bool isAuto => false;
    public override bool isReady { get; set; } = true;
    [Header("한번만 할수있는지or씬이동시 반복가능한지 여부")]
    [SerializeField] bool canReplay;
    [ShowIf("canReplay")]
    [SerializeField] int replayWaitTimeSecond;
    [Header("Drop Table")]
    [SerializeField] DropTable[] dropTables;
    //[SerializeField] private DropTable_DB_LSH dropTable;
    [System.Serializable]
    public struct DropTable
    {
        public DropItem dropItem;
        public int gold;
        public RecordData record;
        public Vector2Int countRange;
        [Range(0f, 1f)] public float probability;
    }
    //[Space(20)]
    //[Header("Settings")]
    private string openTriggerName = "Open";
    private bool opened = false;
    Collider2D col;
    //[Header("Animator")]
    private Animator animator;
    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!animator) Debug.LogError("[Chest] Animator not found!");
        col = GetComponent<Collider2D>();
        isReady = true;
    }

    public override void Run()
    {
        if (!isReady || opened) return;
        OpenChest();
        GameManager.I.ach_chestCount++;
        if(GameManager.I.ach_chestCount >= 5)
        {
            DBManager.I.SteamAchievement("ACH_CHEST_OPEN_5");
        }
    }
    private async void OpenChest()
    {
        opened = true;
        isReady = false;
        // 애니메이션 트리거
        if (animator)
        {
            animator.ResetTrigger(openTriggerName);
            animator.SetTrigger(openTriggerName);
        }
        await Task.Delay(200);
        AudioManager.I.PlaySFX("Chest");
        await Task.Delay(200);
        ParticleManager.I.PlayParticle("Hit1", transform.position + 0.4f * Vector3.up, Quaternion.identity);
        await Task.Delay(200);

        HUDBinder hUDBinder = FindFirstObjectByType<HUDBinder>();
        // 아이템 드롭
        foreach (var element in dropTables)
        {
            if (Random.value > element.probability) continue;
            if (element.dropItem == null && element.record != null)
            {
                if (DBManager.I.HasRecord(element.record.name))
                {
                    //Debug.Log($"{dropInfo.recordData.name}는 이미 가지고 있습니다. 습득불가");
                    continue;
                }
                DBManager.I.AddRecord(element.record.name);
                hUDBinder.PlayNoticeText(3);
            }
            else
            {
                DropItem dropInfo = element.dropItem;
                if (dropInfo.gearData != null)
                {
                    bool outValue;
                    if (DBManager.I.HasGear(dropInfo.gearData.name, out outValue))
                    {
                        //Debug.Log($"{dropInfo.gearData.name}는 이미 가지고 있습니다. 드롭불가");
                        continue;
                    }
                }
                else if (dropInfo.lanternData != null)
                {
                    bool outValue;
                    if (DBManager.I.HasLantern(dropInfo.lanternData.name, out outValue))
                    {
                        //Debug.Log($"{dropInfo.lanternData.name}는 이미 가지고 있습니다. 드롭불가");
                        continue;
                    }
                }
                else if (dropInfo.recordData != null)
                {
                    bool outValue;
                    if (DBManager.I.HasRecord(dropInfo.recordData.name))
                    {
                        //Debug.Log($"{dropInfo.recordData.name}는 이미 가지고 있습니다. 드롭불가");
                        continue;
                    }
                }
                AudioManager.I.PlaySFX("Tick1");
                int count = Random.Range(element.countRange.x, element.countRange.y + 1);
                for (int k = 0; k < count; k++)
                {
                    DropItem dropItem = Instantiate(element.dropItem);
                    dropItem.gold = element.gold;
                    dropItem.transform.position = transform.position;
                    Rigidbody2D rigidbody2D = dropItem.GetComponentInChildren<Rigidbody2D>();
                    if (rigidbody2D != null)
                    {
                        Vector2 dir = Quaternion.Euler(0f, 0f, Random.Range(5f, 15f)) * Vector2.up;
                        if (Random.value <= 0.5f) dir.x = -dir.x;
                        rigidbody2D.AddForce(Random.Range(4f, 8.5f) * dir, ForceMode2D.Impulse);
                    }
                }
            }

            await Task.Delay(10);
        }
        isComplete = true;
    }

}
