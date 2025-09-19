using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CreatureHit : CreatureAbility
{
    public override CreatureControl.State mapping => CreatureControl.State.Hit;
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
