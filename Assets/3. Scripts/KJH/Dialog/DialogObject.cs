using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
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
    public void SetCompletedState()
    {
        isReady = false;
        isComplete = true;
        coll2D.enabled = false;
    }
    #endregion
    [Header("나타날 DialogUI의 대사 번호")]
    public int dialogIndex;
    public string sfxName;
    [Space(30)]
    [Header("다이얼로그끝나고 아이템 습득이 일어나야하는 경우")]
    public ItemData itemData;
    public int itemCount = 0;
    public GearData gearData;
    public LanternFunctionData lanternData;
    public RecordData recordData;
    public int gold;
    Collider2D coll2D;
    [Space(30)]
    [Header("다이얼로그끝나고 다른 스크립트의 메소드 실행필요하면")]
    public UnityEvent onDialogFinished;
    void Awake()
    {
        isReady = true;
        TryGetComponent(out coll2D);
        coll2D.enabled = true;
    }
    public override void Run()
    {
        if(GameManager.I.isOpenDialog || GameManager.I.isOpenPop || GameManager.I.isOpenInventory) return;
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
        yield return new WaitUntil(() => !GameManager.I.isOpenDialog && !GameManager.I.isOpenPop && !GameManager.I.isOpenInventory);
        yield return YieldInstructionCache.WaitForSeconds(0.5f);
        if (itemData != null || gearData != null || lanternData != null || recordData != null || gold != 0)
        {
            AudioManager.I.PlaySFX("GetItem");
        }
        if (itemData != null)
        {
            DBManager.I.AddItem(itemData.name, itemCount);
        }
        if (gearData != null)
        {
            DBManager.I.AddGear(gearData.name);
        }
        if (lanternData != null)
        {
            DBManager.I.AddGear(lanternData.name);
        }
        if (recordData != null)
        {
            DBManager.I.AddGear(recordData.name);
        }
        if (gold != 0)
        {
            DBManager.I.currData.gold += gold;
        }

    }
    [Header("한번만 할수있는지or씬이동시 반복가능한지 여부")]
    [SerializeField] bool canReplay;
    [ShowIf("canReplay")]
    [SerializeField] int replayWaitTimeSecond;




}
