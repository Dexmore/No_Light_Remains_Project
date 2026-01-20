using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using State = MonsterControl.State;
public abstract class MonsterState : MonoBehaviour
{
    [HideInInspector] public abstract State mapping { get; }
    [HideInInspector] public float coolTime = 0f;
    protected MonsterControl control;
    protected Rigidbody2D rb;
    protected Transform model;
    protected Astar2DXYPathFinder astar;
    protected Animator anim;
    public abstract UniTask Enter(CancellationToken token);
    #region UniTask Setting
    [HideInInspector] public CancellationTokenSource cts;
    protected virtual void OnEnable()
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
    }
    protected virtual void OnDisable()
    {
        UniTaskCancel();
    }
    void UniTaskCancel()
    {
        cts?.Cancel();
        try
        {
            cts?.Dispose();
        }
        catch (System.Exception e)
        {

            Debug.Log(e.Message);
        }
        cts = null;
    }
    #endregion
    protected virtual void Awake()
    {
        TryGetComponent(out control);
        rb = GetComponentInParent<Rigidbody2D>();
        TryGetComponent(out astar);
        anim = GetComponentInChildren<Animator>();
        model = transform.GetChild(0);
    }
    public virtual void Exit()
    {
        if (coolTime > 1f)
            if (!control.IsCoolTime(mapping))
                control.SetCoolTime(mapping, coolTime);
        this.enabled = false;
    }
}