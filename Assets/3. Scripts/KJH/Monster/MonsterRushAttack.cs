using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterRushAttack : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.RushAttack;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        Activate(token).Forget();
    }
    public async UniTask Activate(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        await UniTask.Delay((int)(1000f), cancellationToken: token);
        control.ChangeNextState();
    }






}
