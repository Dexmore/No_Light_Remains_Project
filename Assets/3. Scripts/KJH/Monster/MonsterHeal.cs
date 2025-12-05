using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterHeal : MonsterState
{
    int multiHitCount = 1;
    public override MonsterControl.State mapping => MonsterControl.State.Heal;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(token);
        Activate(token).Forget();
    }
    public override void Exit()
    {
        base.Exit();
    }
    public async UniTask Activate(CancellationToken token)
    {

    }

    
}
