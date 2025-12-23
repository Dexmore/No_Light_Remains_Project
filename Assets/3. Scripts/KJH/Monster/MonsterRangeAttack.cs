using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterRangeAttack : MonsterState
{
    public float range;
    public float damageMultiplier = 1.7f;
    public HitData.StaggerType staggerType;
    public Vector2 durationRange;
    float duration;
    int multiHitCount = 1;
    public override MonsterControl.State mapping => MonsterControl.State.RangeAttack;
    public override async UniTask Enter(CancellationToken token)
    {
        control.attackRange.onTriggetStay2D += Handler_TriggerStay2D;
        attackedColliders.Clear();
        await UniTask.Yield(token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();

    }
    public async UniTask Activate(CancellationToken token)
    {
        Transform target;
        target = control.memories.First().Key.transform;
        float dist = Vector3.Distance(target.position, transform.position);
        float distX = Mathf.Abs(target.position.x - transform.position.x);
        float distY = Mathf.Abs(target.position.y - transform.position.y);
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
        RaycastHit2D raycastHit = Physics2D.Linecast((Vector2)control.eye.position, target.position, control.groundLayer);
        if(raycastHit.collider != null)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        Vector2 direction = target.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        if (direction.x > 0 && model.right.x < 0)
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (direction.x < 0 && model.right.x > 0)
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        if (control.isDie) return;
        anim.Play("RangeAttack");
        await UniTask.Delay((int)(1000f * duration), cancellationToken: token);
        control.ChangeNextState();
    }
    public override void Exit()
    {
        base.Exit();
        control.attackRange.onTriggetStay2D -= Handler_TriggerStay2D;
    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    void Handler_TriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (attackedColliders.Count >= multiHitCount) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            Vector2 hitPoint = 0.7f * coll.ClosestPoint(transform.position) + 0.3f * (Vector2)coll.transform.position + Vector2.up;
            GameManager.I.onHit.Invoke
            (
                new HitData
                (
                    "RangeAttack",
                    transform,
                    coll.transform,
                    Random.Range(0.9f, 1.1f) * damageMultiplier * control.adjustedAttack,
                    hitPoint,
                    new string[1] { "Hit2" },
                    staggerType
                )
            );

        }
    }






}
