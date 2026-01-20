using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
public class DoorType1 : MonoBehaviour, ISavable
{
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    [HideInInspector] public bool isComplete;
    bool ISavable.CanReplay => true;
    int ISavable.ReplayWaitTimeSecond => _ReplayWaitTimeSecond;
    int _ReplayWaitTimeSecond = 0;
    public Vector2Int ReplayWaitTimeSecondRange;
    public async void SetCompletedImmediately()
    {
        isComplete = true;
        col.enabled = false;
        animator.Play("Open2");
        await Task.Delay(500);
        doorType2.SetCompletedImmediately();
        Debug.Log(transform.name);
    }
    Collider2D col;
    #endregion
    public bool isOneWay;
    Animator animator;
    public DoorType2 doorType2;
    void Awake()
    {
        TryGetComponent(out col);
        TryGetComponent(out animator);
        playerLayer = LayerMask.NameToLayer("Player");
        isOpen = false;
        _ReplayWaitTimeSecond = Random.Range(ReplayWaitTimeSecondRange.x, ReplayWaitTimeSecondRange.y);
    }
    IEnumerator Start()
    {
        if (!isOneWay) yield break;
        isPlayerRight = false;
        yield return YieldInstructionCache.WaitForSeconds(0.2f);
        PlayerControl playerControl = FindAnyObjectByType<PlayerControl>();
        //Debug.Log($"playerPos : {playerControl.transform.position.x} vs myPos : {transform.position.x}");
        if (playerControl == null)
        {
            yield break;
        }
        // 플레이어가 오른쪽에서 씬 이동해온 경우
        if (playerControl.transform.position.x > transform.position.x + 3)
        {
            // 오른쪽에서 씬 이동해왔다는것은 어떤 경우에라도 양쪽 문 다 열고 웨이브 발동 안되게 해야함.
            Debug.Log($"{transform.name} : Player Is Right");
            col.enabled = false;
            animator.Play("Open2");
            doorType2.SetCompletedImmediately();
            isPlayerRight = true;
            yield break;
        }
        // 플레이어가 왼쪽에서 씬 이동해온 경우
        else if (transform.position.x < playerControl.transform.position.x - 3)
        {
            //Debug.Log("Player Is Left");
        }

    }
    [HideInInspector] public bool isPlayerRight;
    bool isOpen;
    int playerLayer;
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        animator.Play("Open");
        StartCoroutine(nameof(Open_co));
    }
    IEnumerator Open_co()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.7f);
        AudioManager.I.PlaySFX("DoorOpen", transform.position, spatialBlend: 0.5f);
    }
    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        AudioManager.I.PlaySFX("DoorClose", transform.position, spatialBlend: 0.5f);
        animator.Play("Close");
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != playerLayer) return;
        Open();
    }


}
