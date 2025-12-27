using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterExampleState: MonsterState
{
    int multiHitCount = 1;
    public override MonsterControl.State mapping => MonsterControl.State.ShootingAttack1;
    BulletControl bulletControl;
    public override async UniTask Enter(CancellationToken token)
    {
        if (bulletControl == null) bulletControl = FindAnyObjectByType<BulletControl>();
        control.attackRange.onTriggetStay2D += Handler_TriggerStay2D;
        await UniTask.Yield(token);
        Activate(token).Forget();
        attackedColliders.Clear();
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            anim.Play("Idle");
    }
    public override void Exit()
    {
        base.Exit();
        control.attackRange.onTriggetStay2D -= Handler_TriggerStay2D;
        particle?.Despawn();
        particle = null;
    }
    Particle particle;
    public async UniTask Activate(CancellationToken token)
    {
        particle = ParticleManager.I.PlayParticle("DarkCharge", transform.position + 0.5f * control.height * Vector3.up, Quaternion.identity);
        await UniTask.Delay((int)(1000f * 3.5f), cancellationToken: token);
        particle?.Despawn();
        particle = null;
        control.ChangeNextState();
    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    void Handler_TriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (attackedColliders.Count >= multiHitCount) return;
        if (!attackedColliders.Contains(coll))
        {

        }
    }


}
