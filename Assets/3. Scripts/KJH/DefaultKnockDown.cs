using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class DefaultKnockDown : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.KnockDown;
    public override async UniTask Enter(CancellationToken token)
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