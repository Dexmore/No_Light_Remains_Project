using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CreatureDefaultPursuit : CreatureAbility
{
    public override CreatureData.State mapping => CreatureData.State.Pursuit;
    public override async UniTask Init(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
    }






}
