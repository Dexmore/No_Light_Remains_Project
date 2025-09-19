using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CreatureRushAttack1 : CreatureAbility
{
    public override CreatureControl.State mapping => CreatureControl.State.RushAttack1;
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
