using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterRareAttack : MonsterState
{
    public float damageMultiplier = 1.7f;
    public HitData.StaggerType staggerType;
    public Vector2 durationRange;
    float duration;
    int multiHitCount = 1;
    public override MonsterControl.State mapping => MonsterControl.State.RareAttack;
    bool onceFlag = false;
    public bool canParry;
    public override async UniTask Enter(CancellationToken token)
    {
        if (!onceFlag)
        {
            float coolTime = 0;
            for (int i = 0; i < control.patterns.Length; i++)
            {
                for (int j = 0; j < control.patterns[i].frequencies.Length; j++)
                {
                    if (mapping == control.patterns[i].frequencies[j].state)
                    {
                        coolTime = control.patterns[i].frequencies[j].coolTime;
                        break;
                    }
                }
            }
            control.SetCoolTime(MonsterControl.State.RareAttack, Random.Range(0.2f * coolTime, coolTime));
            onceFlag = true;
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        control.attackRange.onTriggetStay2D += TriggerStay2DHandler;
        attackedColliders.Clear();
        await UniTask.Yield(token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();

    }
    public async UniTask Activate(CancellationToken token)
    {
        
        Transform target;
        target = control.memories.First().Key.transform;
        Vector2 direction = target.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        if (direction.x > 0 && model.right.x < 0)
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (direction.x < 0 && model.right.x > 0)
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        if (control.isDie) return;

        anim.Play("RareAttack");

        await UniTask.Delay((int)(1000f * duration), cancellationToken: token);

        control.ChangeNextState();
    }
    public override void Exit()
    {
        base.Exit();
        control.attackRange.onTriggetStay2D -= TriggerStay2DHandler;
    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    void TriggerStay2DHandler(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (attackedColliders.Count >= multiHitCount) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            Vector2 hitPoint = 0.7f * coll.ClosestPoint(transform.position) + 0.3f * (Vector2)coll.transform.position + Vector2.up;
            HitData hitData = new HitData
            (
                "RareAttack",
                transform,
                coll.transform,
                Random.Range(0.9f, 1.1f) * damageMultiplier * control.adjustedAttack,
                hitPoint,
                new string[1] { "Hit2" },
                staggerType
            );
            hitData.isCannotParry = !canParry;
            GameManager.I.onHit.Invoke
            (
                hitData
            );

        }
    }






}
