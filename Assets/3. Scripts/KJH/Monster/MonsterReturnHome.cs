using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterReturnHome : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.ReturnHome;
    Ray2D checkCliffRay;
    RaycastHit2D CheckCliffHit;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(token);
        Activate(token).Forget();
    }
    public override void Exit()
    {
        base.Exit();
    }
    public async UniTask Activate(CancellationToken token)
    {

    }



}
