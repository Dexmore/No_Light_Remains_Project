using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using System.Linq;
public class DialogObject : Interactable, ISavable
{
    #region Interactable Complement
    public override Type type => Type.Normal;
    public override bool isReady { get; set; } = true;
    public override bool isAuto => false;
    #endregion
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete;
    bool ISavable.CanReplay => canReplay;
    int ISavable.ReplayWaitTimeSecond => replayWaitTimeSecond;
    public void SetCompletedImmediately()
    {
        isReady = false;
        isComplete = true;
        coll2D.enabled = false;
        onCompleteImmediately.Invoke();
    }
    #endregion
    [Header("나타날 DialogUI의 대사 번호")]
    public int dialogIndex;
    public string sfxName;
    [Space(30)]
    [Header("다이얼로그끝나고 아이템 습득이 일어나야하는 경우")]
    public ItemData[] itemDatas;
    public int[] itemCounts;
    public GearData[] gearDatas;
    public LanternFunctionData[] lanternDatas;
    public RecordData[] recordDatas;
    public int gold;
    Collider2D coll2D;
    [Space(30)]
    [Header("다이얼로그 켜짐과 함께 다른스크립트 메소드 실행필요하면")]
    public UnityEvent onDialogStart;
    [Header("다이얼로그끝나고 다른 스크립트의 메소드 실행필요하면")]
    public UnityEvent onDialogFinish;
    HUDBinder hUDBinder;
    void Awake()
    {
        isReady = true;
        TryGetComponent(out coll2D);
        coll2D.enabled = true;
        hUDBinder = FindAnyObjectByType<HUDBinder>();
    }
    public override void Run()
    {
        if (GameManager.I.isOpenDialog || GameManager.I.isOpenPop || GameManager.I.isOpenInventory) return;
        isReady = false;
        coll2D.enabled = false;
        GameManager.I.onDialog.Invoke(dialogIndex, transform);
        if (sfxName != null && sfxName != "")
        {
            AudioManager.I.PlaySFX(sfxName, transform.position, null, 0.2f);
        }
        StopCoroutine(nameof(WaitDialogFinish));
        StartCoroutine(nameof(WaitDialogFinish));
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
    public UnityEvent onCompleteImmediately;



}
