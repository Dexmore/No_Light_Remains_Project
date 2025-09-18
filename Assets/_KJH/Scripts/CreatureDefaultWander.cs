using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CreatureDefaultWander : CreatureAbility
{
    public override CreatureData.State mapping => CreatureData.State.Wander;
    public override async UniTask Init(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
    }






}
