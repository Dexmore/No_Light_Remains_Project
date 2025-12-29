using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterShootingAttack1 : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.ShootingAttack1;
    BulletControl bulletControl;
    public float range;
    public float animationWaitSecond;
    public override async UniTask Enter(CancellationToken token)
    {
        if (bulletControl == null) bulletControl = FindAnyObjectByType<BulletControl>();
        await UniTask.Yield(token);
        Activate(token).Forget();
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

        //

        Transform target;
        target = control.memories.First().Key.transform;
        float dist = Vector3.Distance(target.position, transform.position);
        float distX = Mathf.Abs(target.position.x - transform.position.x);
        float distY = Mathf.Abs(target.position.y - transform.position.y);
        //float tempDist = Mathf.Clamp(0.333f * control.findRadius, 0f, 3f * control.width);
        float tempDist = 0.5f * (Mathf.Clamp(control.findRadius, 0f, 10f) + (3f * control.width));
        Debug.Log(0.3f * tempDist);
        if (dist < 0.3f * tempDist)
        {
            // 너무 가까워서 취소
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        if (dist > 1.1f * range + 2f)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        if (distY > 0.26 * range)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        if (distX > 1.1f * range + 2f)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }

        //

        RaycastHit2D[] raycastHits = Physics2D.LinecastAll((Vector2)control.eye.position, (Vector2)target.position + Vector2.up, control.groundLayer);
        bool isBlocked = false;
        for (int i = 0; i < raycastHits.Length; i++)
        {
            if (raycastHits[i].collider.isTrigger) continue;
            isBlocked = true;
            break;
        }
        if (isBlocked)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }

        //

        Vector2 direction = target.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        if (direction.x > 0 && model.right.x < 0)
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (direction.x < 0 && model.right.x > 0)
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        if (control.isDie) return;

        //

        anim.Play("ShootingAttack");
        await UniTask.Delay((int)(1000f * (0.3f * animationWaitSecond)), cancellationToken: token);
        particle = ParticleManager.I.PlayParticle("DarkCharge", transform.position + 0.5f * control.height * Vector3.up, Quaternion.identity);
        particle.transform.localScale = 0.3f * Vector3.one;
        await UniTask.Delay((int)(1000f * (0.7f * animationWaitSecond)), cancellationToken: token);

        //


        await bulletControl.PlayBullet(bulletPaterns, transform, target, token, control.data.Attack);
        await UniTask.Delay((int)(1000f * 4f), cancellationToken: token);
        particle?.Despawn();
        particle = null;
        control.ChangeNextState();
    }



}
