using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ChestInteractable_LSH : Interactable
{
    public override Type type => Type.DropItem;
    public override bool isAuto => false;
    public override bool isReady { get; set; } = true;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Drop Table")]
    [SerializeField] private DropTable_DB_LSH dropTable;

    [Header("Settings")]
    [SerializeField] private string openTriggerName = "Open";

    private bool opened = false;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!animator) Debug.LogError("[Chest] Animator not found!");

        // 상자는 트리거여야 함
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;

        if (!dropTable) Debug.LogWarning("[Chest] DropTable not assigned!");

        isReady = true;
    }

    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (!isReady || opened) return;
    //     if (!other.CompareTag("Player")) return;

    //     OpenChest();
    // }
    public override void Run()
    {
        if (!isReady || opened) return;
        OpenChest();
    }

    private void OpenChest()
    {
        opened = true;
        isReady = false;

        // 애니메이션 트리거
        if (animator)
        {
            animator.ResetTrigger(openTriggerName);
            animator.SetTrigger(openTriggerName);
        }

        GiveReward();
    }

    private void GiveReward()
    {
        if (!dropTable)
        {
            Debug.LogError("[Chest] dropTable not assigned!");
            return;
        }

        dropTable.GiveToDB();
    }
}
