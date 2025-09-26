using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DefaultIdle : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Idle;
    public Vector2 durationRange;
    float duration;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();
    }
    public override async UniTask Activate(CancellationToken token)
    {
        anim.Play("Idle");
        await UniTask.Delay((int)(duration * 1000f), cancellationToken: token);
        control.ChangeNextState();
    }






}
