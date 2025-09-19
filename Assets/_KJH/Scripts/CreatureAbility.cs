using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using State = CreatureControl.State;
public abstract class CreatureAbility : MonoBehaviour
{
    #region UniTask Setting
    [HideInInspector] public CancellationTokenSource cts;
    protected virtual void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
        OnEnableAfter();
    }
    protected virtual void OnDisable()
    {
        UniTaskCancel();
    }
    protected virtual void OnDestroy() { UniTaskCancel(); }
    void UniTaskCancel()
    {
        try
        {
            cts?.Cancel();
            cts?.Dispose();
        }
        catch (System.Exception e)
        {

            Debug.Log(e.Message);
        }
        cts = null;
    }
    #endregion
    protected CreatureControl control;
    //CreatureData data;
    protected virtual void Awake()
    {
        TryGetComponent(out control);
        rb = GetComponentInParent<Rigidbody2D>();
        TryGetComponent(out astar);
        TryGetComponent(out sensor);
        anim = GetComponentInChildren<Animator>();
    }
    void OnEnableAfter()
    {
        //data = control.data;
    }
    [HideInInspector] public abstract State mapping { get; }
    [HideInInspector] public float coolTime = 0f;
    protected Rigidbody2D rb;
    protected Astar2DXYPathFinder astar;
    protected CreatureSensor sensor;
    protected Animator anim;
    public abstract UniTask Init(CancellationToken token);
    public abstract UniTask Activate(CancellationToken token);
    public virtual void UnInit()
    {
        if (coolTime > 1f)
            if (!control.IsCoolTime(mapping))
                control.SetCoolTime(mapping, coolTime);
        //anim.CrossFade("Idle", 0.2f);
        this.enabled = false;
    }
}