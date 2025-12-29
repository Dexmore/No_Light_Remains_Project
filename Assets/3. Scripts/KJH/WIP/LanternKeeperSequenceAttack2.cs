using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class LanternKeeperSequenceAttack2 : MonsterState
{
    int multiHitCount = 1;
    public override MonsterControl.State mapping => MonsterControl.State.SequenceAttack2;
    public override async UniTask Enter(CancellationToken token)
    {
        control.attackRange.onTriggetStay2D += Handler_TriggerStay2D;
        await UniTask.Yield(token);
        Activate(token).Forget();
        attackedColliders.Clear();
    }
    public override void Exit()
    {
        base.Exit();
        control.attackRange.onTriggetStay2D -= Handler_TriggerStay2D;
    }
    public async UniTask Activate(CancellationToken token)
    {

    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    void Handler_TriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (attackedColliders.Count >= multiHitCount) return;
        if (!attackedColliders.Contains(coll))
        {
            
        }
    }


}
