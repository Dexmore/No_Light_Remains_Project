using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CreatureDefaultRePosition : CreatureAbility
{
    public override CreatureData.State mapping => CreatureData.State.RePosition;
    public override async UniTask Init(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
    }






}
