using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class LanternKeeperSequenceAttack1 : MonsterState
{
    public float damageMultiplier1 = 0.7f;
    public HitData.StaggerType staggerType1;
    public float damageMultiplier2 = 2.3f;
    public HitData.StaggerType staggerType2;
    public float damageMultiplier3 = 1.2f;
    public HitData.StaggerType staggerType3;
    int multiHitCount = 1;
    public Vector2 durationRange;
    GameObject chafe;
    public override MonsterControl.State mapping => MonsterControl.State.SequenceAttack1;
    public override async UniTask Enter(CancellationToken token)
    {
        if (transform.Find("Chafe") != null)
            chafe = transform.Find("Chafe").gameObject;
        else
            chafe = transform.GetChild(0).Find("Chafe").gameObject;
        chafe?.SetActive(false);
        control.attackRange.onTriggetStay2D += Handler_TriggerStay2D;
        await UniTask.Yield(cts.Token);
        Activate(token).Forget();
        attackedColliders.Clear();
    }
    public override void Exit()
    {
        base.Exit();
        control.attackRange.onTriggetStay2D -= Handler_TriggerStay2D;
        chafe?.SetActive(true);
    }
    public async UniTask Activate(CancellationToken token)
    {
        float startTime = Time.time;
        if (control.isDie) return;
        Transform target;
        Vector2 moveDirection;
        target = control.memories.First().Key.transform;
        moveDirection = target.position - transform.position;
        moveDirection.y = 0;
        moveDirection.Normalize();
        if (moveDirection.x > 0 && model.right.x < 0)
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (moveDirection.x < 0 && model.right.x > 0)
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);

        // 1번 공격
        attackIndex = 0;
        anim.Play("JumpAttack");
        await UniTask.Delay(1300, cancellationToken: token);
        rb.AddForce(Vector2.up * 26f + (Vector2)model.right * 6f, ForceMode2D.Impulse);
        await UniTask.Delay(250, cancellationToken: token);
        rb.gravityScale = 1.35f;
        await UniTask.Delay(720, cancellationToken: token);
        rb.gravityScale = 2f;
        attackedColliders.Clear();
        await UniTask.Delay(185, cancellationToken: token);
        moveDirection = target.position - transform.position;
        moveDirection.y = 0;
        moveDirection.Normalize();
        if (moveDirection.x > 0 && model.right.x < 0)
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (moveDirection.x < 0 && model.right.x > 0)
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        rb.AddForce(Vector2.down * 18f + (Vector2)(target.position - transform.position).normalized * 7.5f, ForceMode2D.Impulse);
        await UniTask.WaitUntil(() => control.isGround, cancellationToken: token);

        // 2번 공격
        attackIndex = 1;
        anim.Play("SlamAttack");
        await UniTask.Delay(600, cancellationToken: token);
        chafe?.SetActive(true);
        int dur = Random.Range((int)(durationRange.x * 1000f), (int)(durationRange.y * 1000f));
        await UniTask.Delay(dur, cancellationToken: token);
        control.ChangeNextState();
    }
    List<Collider2D> attackedColliders = new List<Collider2D>();
    int attackIndex = 0;
    void Handler_TriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (attackedColliders.Count >= multiHitCount) return;
        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            Vector2 hitPoint = 0.7f * coll.ClosestPoint(transform.position) + 0.3f * (Vector2)coll.transform.position + Vector2.up;
            HitData.StaggerType staggerType = HitData.StaggerType.Small;
            float damageMultiplier = damageMultiplier1;
            HitData hitData = new HitData
            (
                "SequenceAttack",
                transform,
                coll.transform,
                control.data.Attack,
                hitPoint,
                staggerType
            );
            switch (attackIndex)
            {
                case 0:
                    staggerType = HitData.StaggerType.Small;
                    damageMultiplier = damageMultiplier1;
                    hitData.attackName = "JumpAttack";
                    break;

                case 1:
                    staggerType = HitData.StaggerType.Large;
                    damageMultiplier = damageMultiplier2;
                    hitData.isCannotParry = true;
                    hitData.attackName = "SlamAttack";
                    break;
                case 2:
                    staggerType = HitData.StaggerType.Middle;
                    damageMultiplier = damageMultiplier3;
                    hitData.attackName = "BeamAttack";
                    break;
            }
            hitData.staggerType = staggerType;
            hitData.damage = Random.Range(0.9f, 1.1f) * damageMultiplier * control.data.Attack;
            GameManager.I.onHit.Invoke(hitData);
        }
    }




}