using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CreatureDefaultBiteAttack1 : CreatureAbility
{
    public override CreatureData.State mapping => CreatureData.State.BiteAttack1;
    public override async UniTask Init(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
    }






}
