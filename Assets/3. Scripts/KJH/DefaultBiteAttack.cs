using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DefaultBiteAttack : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.BiteAttack;
    public override async UniTask Init(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        Activate(token).Forget();
    }
    public override async UniTask Activate(CancellationToken token)
    {
        if (control.HasCondition(MonsterControl.Condition.Peaceful))
        {
            await UniTask.Yield(cts.Token);
            control.ChangeState(MonsterControl.State.Idle);
            return;
        }
        await UniTask.Delay((int)(1000f), cancellationToken: token);
        control.ChangeNextState();
    }




}
