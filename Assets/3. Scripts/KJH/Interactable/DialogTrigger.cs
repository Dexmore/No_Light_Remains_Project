using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
public class DialogTrigger : MonoBehaviour, ISavable
{
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete = false;
    bool ISavable.CanReplay => canReplay;
    int ISavable.ReplayWaitTimeSecond => replayWaitTimeSecond;
    public void SetCompletedImmediately()
    {
        isComplete = true;
        coll2D.enabled = false;
    }
    #endregion
    [Header("나타날 DialogUI의 대사 번호")]
    public int dialogIndex;
    public string sfxName;
    [Space(30)]
    [Header("다이얼로그가 끝나고 아이템 습득이 일어나야하는 경우")]
    public ItemData[] itemDatas;
    public int[] itemCounts;
    public GearData[] gearDatas;
    public LanternFunctionData[] lanternDatas;
    public RecordData[] recordDatas;
    public int gold;
    [Space(30)]
    [Header("다이얼로그 켜짐과 함께 다른스크립트 메소드 실행필요하면")]
    public UnityEvent onDialogStart;
    [Header("다이얼로그가 끝난뒤 다른스크립트 메소드 실행필요하면")]
    public UnityEvent onDialogFinish;
    Collider2D coll2D;
    int playerLayer;
    HUDBinder hUDBinder;
    void Awake()
    {
        TryGetComponent(out coll2D);
        coll2D.enabled = true;
        playerLayer = LayerMask.NameToLayer("Player");
        hUDBinder = FindAnyObjectByType<HUDBinder>();
    }
    PlayerControl playerControl;
    float stayTimer;
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != playerLayer) return;
        stayTimer = 0;
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer != playerLayer) return;
        if (GameManager.I.isOpenDialog || GameManager.I.isOpenPop || GameManager.I.isOpenInventory)
        {
            stayTimer = 0;
            return;
        }
        if (playerControl == null) playerControl = collision.gameObject.GetComponentInParent<PlayerControl>();
        if (playerControl == null)
        {
            stayTimer = 0;
            return;
        }
        if (!playerControl.Grounded)
        {
            stayTimer = 0;
            return;
        }
        if (isComplete) return;
        stayTimer += Time.deltaTime;
        if (stayTimer >= 0.115f)
        {
            isComplete = true;
            if (canReplay)
            {
                DBManager.I.SetLastTimeReplayObject(this);
            }
            coll2D.enabled = false;
            GameManager.I.onDialog.Invoke(dialogIndex, transform);
            if (!string.IsNullOrEmpty(sfxName))
            {
                AudioManager.I.PlaySFX(sfxName, transform.position, null, 0.2f);
            }
            StopCoroutine(nameof(WaitDialogFinish));
            StartCoroutine(nameof(WaitDialogFinish));
        }
    }

    IEnumerator WaitDialogFinish()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.37f);
        onDialogStart.Invoke();
        yield return new WaitUntil(() => !GameManager.I.isOpenDialog && !GameManager.I.isOpenPop && !GameManager.I.isOpenInventory);
        yield return YieldInstructionCache.WaitForSeconds(0.5f);
        if (itemDatas != null || gearDatas != null || lanternDatas != null || recordDatas != null || gold != 0)
        {
            if (itemDatas.Length > 0 || gearDatas.Length > 0 || lanternDatas.Length > 0 || recordDatas.Length > 0)
                AudioManager.I.PlaySFX("GetItem");
        }
        if (itemDatas != null && itemDatas.Length > 0)
        {
            for (int k = 0; k < itemDatas.Length; k++)
            {
                DBManager.I.AddItem(itemDatas[k].name, itemCounts[k]);
                hUDBinder.PlayNoticeText(0);
            }
        }
        if (gearDatas != null && gearDatas.Length > 0)
        {
            foreach (var element in gearDatas)
            {
                DBManager.I.AddGear(element.name);
                hUDBinder.PlayNoticeText(1);
            }
        }
        if (lanternDatas != null && lanternDatas.Length > 0)
        {
            foreach (var element in lanternDatas)
            {
                DBManager.I.AddLantern(element.name);
                hUDBinder.PlayNoticeText(2);
            }
        }
        if (recordDatas != null && recordDatas.Length > 0)
        {
            foreach (var element in recordDatas)
            {
                DBManager.I.AddRecord(element.name);
                hUDBinder.PlayNoticeText(3);
            }
        }
        if (gold != 0)
        {
            DBManager.I.currData.gold += gold;
        }
        onDialogFinish.Invoke();
    }
    [Header("한번만 할수있는지or씬이동시 반복가능한지 여부")]
    [SerializeField] bool canReplay;
    [ShowIf("canReplay")]
    [SerializeField] int replayWaitTimeSecond;




}
