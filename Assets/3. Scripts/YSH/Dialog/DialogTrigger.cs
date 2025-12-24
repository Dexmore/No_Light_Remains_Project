using System.Collections;
using UnityEngine;
using UnityEngine.Events;
public class DialogTrigger : MonoBehaviour, ISavable
{
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete;
    bool ISavable.CanReplay => false;
    int ISavable.ReplayWaitTimeSecond => 0;
    public void SetCompletedState()
    {
        isComplete = true;
        coll2D.enabled = false;
    }
    #endregion
    [Header("나타날 DialogUI의 대사 번호")]
    public int dialogIndex;
    [Space(30)]
    [Header("다이얼로그가 끝나고 아이템 습득이 일어나야하는 경우")]
    public ItemData itemData;
    public GearData gearData;
    public LanternFunctionData lanternData;
    public RecordData recordData;
    public int gold;
    [Space(30)]
    [Header("다이얼로그가 끝나고 다른 스크립트의 메소드 실행필요하면")]
    public UnityEvent onDialogFinished;
    Collider2D coll2D;
    void Awake()
    {
        TryGetComponent(out coll2D);
        coll2D.enabled = true;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        isComplete = true;
        GameManager.I.onDialog.Invoke(dialogIndex, transform);
        coll2D.enabled = false;
    }
    
    
}
