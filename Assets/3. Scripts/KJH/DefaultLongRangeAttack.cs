using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class DefaultLongRangeAttack : MonsterState
{
    public Vector2 durationRange;
    float duration;
    int multiHitCount = 1;
    public override MonsterControl.State mapping => MonsterControl.State.LoneRangeAttack;
    public override async UniTask Enter(CancellationToken token)
    {
        control.attackRange.onTriggetStay2D += OnTriggerStay2D_Child;
        attackedColliders.Clear();
        await UniTask.Yield(cts.Token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();
        anim.Play("LRAttack");
    }
    public override async UniTask Activate(CancellationToken token)
    {
        await UniTask.Delay((int)(1000f * duration), cancellationToken: token);
        control.ChangeNextState();
    }
    public override void Exit()
    {
        base.Exit();
        control.attackRange.onTriggetStay2D -= OnTriggerStay2D_Child;
    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    void OnTriggerStay2D_Child(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (attackedColliders.Count >= multiHitCount) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            EventManager.I.onAttack(new EventManager.AttackData(transform, coll.transform, Random.Range(0.9f,1.1f) * control.data.Attack * 2.2f));
        }
    }
    





}
