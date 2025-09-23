using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class DefaultRunAway : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.RunAway;
    public override async UniTask Init(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        Activate(token).Forget();
    }
    public override async UniTask Activate(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        await UniTask.Delay((int)(1000f), cancellationToken: token);
        control.ChangeNextState();
    }






}
