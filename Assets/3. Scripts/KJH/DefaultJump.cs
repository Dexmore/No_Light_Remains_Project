using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class DefaultJump : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Jump;
    public override async UniTask Init(CancellationToken token)
    {
        //Debug.Log($"{transform.name} : {control.state}");
        await UniTask.Yield(cts.Token);
        Activate(token).Forget();
    }
    public override async UniTask Activate(CancellationToken token)
    {
        anim.CrossFade("Jump", 0.18f);
        rb.AddForce(Vector2.up * control.jumpForce * 50f);
        float startTime = Time.time;
        await UniTask.Delay(1000, cancellationToken: token);
        await UniTask.WaitUntil(() => control.isGround, cancellationToken: token);
        control.ChangeNextState();
    }






}
