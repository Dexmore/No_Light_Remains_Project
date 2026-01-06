using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class LanternKeeperSequenceAttack3 : MonsterState
{
    int multiHitCount = 1;
    public override MonsterControl.State mapping => MonsterControl.State.SequenceAttack3;
    public override async UniTask Enter(CancellationToken token)
    {
        control.attackRange.onTriggetStay2D += TriggerStay2DHandler;
        await UniTask.Yield(token);
        Activate(token).Forget();
        attackedColliders.Clear();
    }
    public override void Exit()
    {
        base.Exit();
        control.attackRange.onTriggetStay2D -= TriggerStay2DHandler;
    }
    public async UniTask Activate(CancellationToken token)
    {

    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    void TriggerStay2DHandler(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (attackedColliders.Count >= multiHitCount) return;
        if (!attackedColliders.Contains(coll))
        {
            
        }
    }


}
