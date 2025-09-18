using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CreatureDefaultJumpAttack1 : CreatureAbility
{
    public override CreatureData.State mapping => CreatureData.State.JumpAttack1;
    public override async UniTask Init(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
    }






}
