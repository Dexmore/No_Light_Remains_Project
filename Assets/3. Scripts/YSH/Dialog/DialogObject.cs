using System.Collections;
using UnityEngine;
using UnityEngine.Events;
public class DialogObject : Interactable, ISavable
{
    #region Interactable Complement
    public override Type type => Type.Normal;
    public override bool isReady { get; set; } = true;
    public override bool isAuto => false;
    public override void Run()
    {
        isReady = false;
        coll2D.enabled = false;
        GameManager.I.onDialog.Invoke(dialogIndex, transform);
        StartCoroutine(nameof(Run_co));
    }
    IEnumerator Run_co()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.5f);
        while (true)
        {
            yield return new WaitUntil(() => !GameManager.I.isOpenDialog && !GameManager.I.isOpenPop);
            yield return YieldInstructionCache.WaitForSeconds(0.2f);
            if (!GameManager.I.isOpenDialog && !GameManager.I.isOpenPop) break;
        }
        onDialogFinished.Invoke();

    }
    #endregion
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete;
    bool ISavable.CanReplay => false;
    int ISavable.ReplayWaitTimeSecond => 0;
    public void SetCompletedState()
    {
        isReady = false;
        isComplete = true;
        coll2D.enabled = false;
    }
    #endregion
    [Header("나타날 DialogUI의 대사 번호")]
    public int dialogIndex;
    [Space(30)]
    [Header("다이얼로그끝나고 아이템 습득이 일어나야하는 경우")]
    public ItemData itemData;
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




}
