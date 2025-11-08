using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterHit : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Hit;
    [HideInInspector] public int type;
    [HideInInspector] public MonsterControl.State prevState;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        Activate(token).Forget();
    }
    public async UniTask Activate(CancellationToken token)
    {
        Debug.Log($"{type}, {prevState}, {anim.GetCurrentAnimatorStateInfo(0).shortNameHash} , {anim.GetCurrentAnimatorStateInfo(0).normalizedTime}/{anim.GetCurrentAnimatorStateInfo(0).length} ");
        await UniTask.Yield(cts.Token);
        anim.Play("Idle");
        await UniTask.Delay((int)(600f), cancellationToken: token);
        control.ChangeNextState();
    }






}
