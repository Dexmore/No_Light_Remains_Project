using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterMovingAttack : MonsterState
{
    public float damageMultiplier = 1f;
    public HitData.StaggerType staggerType;
    public Vector2 durationRange;
    float duration;
    public float range = 1.4f;
    public Vector2 moveTimeRange;
    int multiHitCount = 1;
    MonsterShortAttack monsterShortAttack;
    public override MonsterControl.State mapping => MonsterControl.State.MovingAttack;
    public override async UniTask Enter(CancellationToken token)
    {
        TryGetComponent(out monsterShortAttack);
        control.attackRange.onTriggetStay2D += Handler_TriggerStay2D;
        attackedColliders.Clear();
        await UniTask.Yield(cts.Token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();
    }
    public async UniTask Activate(CancellationToken token)
    {
        if (control.memories.Count == 0)
        {
            await UniTask.Yield(cts.Token);
            control.ChangeState(MonsterControl.State.Idle);
            return;
        }
        Transform target;
        Vector2 moveDirection;
        float startTime;
        startTime = Time.time;
        target = control.memories.First().Key.transform;
        moveDirection = target.position - transform.position;
        moveDirection.y = 0;
        moveDirection.Normalize();
        float dist = Mathf.Abs(target.position.x - transform.position.x);
        bool condition = dist < 0.9f * range - 0.1f;
        bool once = false;
        bool isAnimation = false;
        // 너무 가까우면 살짝 뒤로 이동
        if (condition)
        {
            while (Time.time - startTime < 0.3f)
            {
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
                moveDirection = transform.position - target.position;
                moveDirection.y = 0;
                moveDirection.Normalize();
                dist = Mathf.Abs(target.position.x - transform.position.x);
                condition = dist < 0.9f * range - 0.1f;
                // 캐릭터 방향 설정
                if (!once)
                {
                    anim.Play("Move");
                    isAnimation = true;
                    once = true;
                    if (moveDirection.x > 0 && model.right.x < 0)
                        model.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    else if (moveDirection.x < 0 && model.right.x > 0)
                        model.localRotation = Quaternion.Euler(0f, 180f, 0f);
                }
                float dot = Vector2.Dot(rb.linearVelocity, moveDirection);
                // 벽 향해서 전진하는 버그 막기
                bool stopWall = false;
                if (control.collisions.Count > 0)
                {
                    foreach (var element in control.collisions)
                    {
                        if (Mathf.Abs(element.Value.y - transform.position.y) >= 0.09f * control.height)
                        {
                            if (element.Value.x - transform.position.x > 0.25f * control.width && moveDirection.x > 0)
                            {
                                stopWall = true;
                                break;
                            }
                            else if (element.Value.x - transform.position.x < -0.25f * control.width && moveDirection.x < 0)
                            {
                                stopWall = true;
                                break;
                            }
                        }
                    }
                }
                // AddForce방식으로 캐릭터 이동
                if (!stopWall)
                    if (dot < control.data.MoveSpeed)
                    {
                        float multiplier = (control.data.MoveSpeed - dot) + 1f;
                        rb.AddForce(multiplier * moveDirection * 0.75f * (control.data.MoveSpeed + 4.905f) / 1.25f);
                        if (control.isGround)
                        {
                            if (!isAnimation)
                                if (control.isGround)
                                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Move"))
                                    {
                                        isAnimation = true;
                                        anim.Play("Move");
                                    }
                        }
                        else if (isAnimation)
                        {
                            isAnimation = false;
                            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                            {
                                anim.Play("Idle");
                            }
                        }
                    }
                if (!condition) break;
            }
        }
        moveDirection = target.position - transform.position;
        moveDirection.y = 0;
        moveDirection.Normalize();
        if (moveDirection.x > 0 && model.right.x < 0)
        {
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (moveDirection.x < 0 && model.right.x > 0)
        {
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
        if (control.isDie) return;
        anim.Play("MovingAttack");
        startTime = Time.time;
        await UniTask.Delay((int)(1000f * moveTimeRange.x), cancellationToken: token);
        rb.AddForce(23f * model.right, ForceMode2D.Impulse);
        await UniTask.Delay(50, cancellationToken: token);
        while (Time.time - startTime < moveTimeRange.y)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            moveDirection = model.right;
            float dot = Vector2.Dot(rb.linearVelocity, moveDirection);
            // 벽 향해서 전진하는 버그 막기
            bool stopWall = false;
            if (control.collisions.Count > 0)
            {
                foreach (var element in control.collisions)
                {
                    if (Mathf.Abs(element.Value.y - transform.position.y) >= 0.09f * control.height)
                    {
                        if (element.Value.x - transform.position.x > 0.25f * control.width && moveDirection.x > 0)
                        {
                            stopWall = true;
                            break;
                        }
                        else if (element.Value.x - transform.position.x < -0.25f * control.width && moveDirection.x < 0)
                        {
                            stopWall = true;
                            break;
                        }
                    }
                }
            }
            // AddForce방식으로 캐릭터 이동
            if (!stopWall)
                if (dot < control.data.MoveSpeed)
                {
                    float multiplier = (control.data.MoveSpeed - dot) + 1f;
                    rb.AddForce(multiplier * moveDirection * 2.5f * (control.data.MoveSpeed + 4.905f) / 1.25f);
                }
        }
        anim.Play("Idle");
        //await UniTask.Delay((int)(1000f * (duration - (moveTimeRange.y - moveTimeRange.x))), cancellationToken: token);
        PlayerControl pControl = target.GetComponentInParent<PlayerControl>();
        startTime = Time.time;
        bool isPlayerHit = false;
        while (Time.time - startTime < duration - (moveTimeRange.y - moveTimeRange.x))
        {
            if (monsterShortAttack == null) break;
            await UniTask.Yield(token);
            if (!isPlayerHit && pControl.fsm.currentState == pControl.hit) 
            {
                isPlayerHit = true;
                startTime -= 1.7f;
            }
        }
        if (isPlayerHit)
            control.ChangeState(MonsterControl.State.ShortAttack, true);
        else
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
                    "MovingAttack",
                    transform,
                    coll.transform,
                    Random.Range(0.9f, 1.1f) * damageMultiplier * control.data.Attack,
                    hitPoint,
                    new string[1]{"Hit2"},
                    staggerType
                )
            );
        }
    }




}
