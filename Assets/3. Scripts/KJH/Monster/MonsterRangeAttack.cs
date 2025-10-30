using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterRangeAttack : MonsterState
{
    public float damageMultiplier = 1.7f;
    public HitData.StaggerType staggerType;
    public Vector2 durationRange;
    float duration;
    int multiHitCount = 1;
    public override MonsterControl.State mapping => MonsterControl.State.RangeAttack;
    public override async UniTask Enter(CancellationToken token)
    {
        control.attackRange.onTriggetStay2D += Handler_TriggerStay2D;
        attackedColliders.Clear();
        await UniTask.Yield(cts.Token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();
        anim.Play("RAttack");
    }
    public async UniTask Activate(CancellationToken token)
    {
        await UniTask.Delay((int)(1000f * duration), cancellationToken: token);
        control.ChangeNextState();
    }
    public override void Exit()
    {
        base.Exit();
        control.attackRange.onTriggetStay2D -= Handler_TriggerStay2D;
    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    void Handler_TriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (attackedColliders.Count >= multiHitCount) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            GameManager.I.onHit.Invoke(new HitData(transform, coll.transform, Random.Range(0.9f,1.1f) * damageMultiplier * control.data.Attack, staggerType));
        }
    }
    





}
