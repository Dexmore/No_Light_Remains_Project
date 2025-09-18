using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CreatureDefaultRunAway : CreatureAbility
{
    public override CreatureData.State mapping => CreatureData.State.RunAway;
    public override async UniTask Init(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
    }






}
