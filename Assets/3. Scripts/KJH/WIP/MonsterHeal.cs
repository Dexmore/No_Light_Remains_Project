using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterHeal : MonsterState
{
    int multiHitCount = 1;
    public override MonsterControl.State mapping => MonsterControl.State.Heal;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(token);
        Activate(token).Forget();
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            anim.Play("Idle");
    }
    public override void Exit()
    {
        base.Exit();
        particle?.Despawn();
        particle = null;
    }
    Particle particle;
    public async UniTask Activate(CancellationToken token)
    {
        await UniTask.Yield(token);
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            anim.Play("Idle");
        particle = ParticleManager.I.PlayParticle("DarkCharge", transform.position + 0.5f * control.height * Vector3.up, Quaternion.identity);
        await UniTask.Delay((int)(1000f * 3.5f), cancellationToken: token);
        particle?.Despawn();
        particle = null;
        control.ChangeNextState();
    }
}
