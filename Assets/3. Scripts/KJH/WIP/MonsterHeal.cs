using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterHeal : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Heal;
    public Vector2 durationRange;
    float duration;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(token);
        Activate(token).Forget();
        duration = Random.Range(durationRange.x, durationRange.y);
    }
    public override void Exit()
    {
        base.Exit();
        particle?.Despawn();
        particle = null;
    }
    Particle particle;
    public List<BulletControl.BulletPatern> bulletPaterns = new List<BulletControl.BulletPatern>();
    public async UniTask Activate(CancellationToken token)
    {
        await UniTask.Yield(token);
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            anim.Play("Idle");


        anim.Play("Heal");
        await UniTask.Delay((int)(1000f * (0.3f * duration)), cancellationToken: token);
        particle = ParticleManager.I.PlayParticle("DarkCharge", transform.position + 0.5f * control.height * Vector3.up, Quaternion.identity);
        particle.transform.localScale = 0.3f * Vector3.one;


        



        await UniTask.Delay((int)(1000f * (0.7f * duration)), cancellationToken: token);
        particle?.Despawn();
        particle = null;
        control.ChangeNextState();
    }



}
