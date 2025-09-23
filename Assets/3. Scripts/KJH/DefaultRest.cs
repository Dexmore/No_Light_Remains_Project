using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class DefaultRest : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Rest;
    public Vector2 durationRange;
    float duration;
    public override async UniTask Init(CancellationToken token)
    {
        //Debug.Log($"{transform.name} : {control.state}");
        await UniTask.Yield(cts.Token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();
    }
    public override async UniTask Activate(CancellationToken token)
    {
        anim.CrossFade("Rest", 0.18f);
        await UniTask.Delay((int)(duration * 1000f), cancellationToken: token);
        control.ChangeNextState();
    }
    public override void UnInit()
    {
        base.UnInit();
    }





}

