using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterJump : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Jump;
    public override async UniTask Enter(CancellationToken token)
    {
        //Debug.Log($"{transform.name} : {control.state}");
        await UniTask.Yield(token);
        Activate(token).Forget();
    }
    public async UniTask Activate(CancellationToken token)
    {
        if (control.isDie) return;
        if(!control.isGround)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        anim.Play("Idle");
        rb.AddForce(Vector2.up * (control.jumpLength + 1.85f) * 175f);
        float startTime = Time.time;
        await UniTask.Delay(1000, cancellationToken: token);
        await UniTask.WaitUntil(() => control.isGround, cancellationToken: token);
        control.ChangeNextState();
    }






}
