using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MonsterDie : MonsterState
{
    public float duration = 0.4f;
    public override MonsterControl.State mapping => MonsterControl.State.Die;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        Activate(token).Forget();
    }
    public async UniTask Activate(CancellationToken token)
    {
        control.isDie = true;
        await UniTask.Yield(cts.Token);
        anim.Play("Die");
        await UniTask.Delay((int)(1000f * (duration)), cancellationToken: token);
        Destroy(gameObject);
    }






}
