using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DefaultDie : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Die;
    public override async UniTask Init(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        Activate(token).Forget();
    }
    public override async UniTask Activate(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
    }






}
