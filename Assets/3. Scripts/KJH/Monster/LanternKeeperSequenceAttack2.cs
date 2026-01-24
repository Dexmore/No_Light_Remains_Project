using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class LanternKeeperSequenceAttack2 : MonsterState
{
    int multiHitCount = 1;
    public override MonsterControl.State mapping => MonsterControl.State.SequenceAttack2;
    float range = 1.4f;
    public Vector2 durationRange;
    float duration;
    float damageMultiplier = 1f;
    // 낭떠러지 체크용
    Vector2 rayOrigin;
    Vector2 rayDirection;
    float rayLength;
    Ray2D checkRay;
    RaycastHit2D CheckRayHit;
    public override async UniTask Enter(CancellationToken token)
    {
        control.attackRange.onTriggetStay2D += TriggerStay2DHandler;
        await UniTask.Yield(token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();
        attackedColliders.Clear();
        attackIndex = 0;
    }
    public override void Exit()
    {
        base.Exit();
        control.attackRange.onTriggetStay2D -= TriggerStay2DHandler;
    }
    public async UniTask Activate(CancellationToken token)
    {
        if (control.memories.Count == 0)
        {
            await UniTask.Yield(token);
            control.ChangeState(MonsterControl.State.Idle);
            return;
        }
        Transform target;
        target = control.memories.First().Key.transform;
        float dist = Mathf.Abs(target.position.x - transform.position.x);
        if (dist > 1.1f * range + 2f)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
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
        Vector2 moveDirection;
        float startTime;
        startTime = Time.time;
        moveDirection = target.position - transform.position;
        moveDirection.y = 0;
        moveDirection.Normalize();
        bool condition = dist < 0.9f * range - 0.1f;
        bool once = false;
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
                    if (control.isDie) return;
                    anim.Play("Move");
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

                // 낭떠러지 체크
                rayOrigin = transform.position + 1.3f * control.width * model.right + 0.2f * control.height * Vector3.up;
                rayDirection = Vector3.down;
                rayLength = 0.9f * control.jumpLength + 0.1f * control.height;
                checkRay.origin = rayOrigin;
                checkRay.direction = rayDirection;
                CheckRayHit = Physics2D.Raycast(checkRay.origin, checkRay.direction, rayLength, control.groundLayer);
                if (CheckRayHit.collider == null)
                {
                    stopWall = true;
                }

                // AddForce방식으로 캐릭터 이동
                if (!stopWall)
                    if (dot < control.data.MoveSpeed)
                    {
                        float multiplier = (control.data.MoveSpeed - dot) + 1f;
                        rb.AddForce(multiplier * moveDirection * 0.75f * (control.data.MoveSpeed + 4.905f) / 1.25f);
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


        //1타
        damageMultiplier = 0.75f;
        attackIndex = 0;
        anim.Play("SequenceAttack2");
        await UniTask.Yield(token);
        attackedColliders.Clear();
        await UniTask.Delay((int)(1000f * 1.2f), cancellationToken: token);

        //2타
        damageMultiplier = 0.8f;
        attackIndex = 1;
        anim.Play("NormalAttack", 0, 0.28f);
        await UniTask.Yield(token);
        attackedColliders.Clear();
        await UniTask.Delay((int)(1000f * 1f), cancellationToken: token);

        //3타
        damageMultiplier = 0.8f;
        attackIndex = 2;
        anim.Play("SequenceAttack2", 0, 0.25f);
        await UniTask.Yield(token);
        attackedColliders.Clear();
        await UniTask.Delay((int)(1000f * 1f), cancellationToken: token);


        if (control.HasCondition(MonsterControl.Condition.Phase3))
        {
            //4타
            SpriteRenderer sr = transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (sr)
            {
                GameManager.I.PlayAfterImageEffect(sr, 2.3f);
            }
            await UniTask.Delay((int)(1000f * 0.3f), cancellationToken: token);
            damageMultiplier = 1.4f;
            attackIndex = 3;
            anim.Play("JumpAttack", 0, 0.8f);
            await UniTask.Yield(token);
            attackedColliders.Clear();
            rb.gravityScale = 1.5f;
            rb.AddForce(Vector2.up * 18f + (Vector2)model.right * 2f, ForceMode2D.Impulse);
            await UniTask.Delay((int)(1000f * 0.3f), cancellationToken: token);
            rb.gravityScale = 2f;
            verticalLines = ParticleManager.I.PlayUIParticle("UIVerticalLines", new Vector2(960, 540), Quaternion.identity);
            await UniTask.Delay((int)(1000f * 0.5f), cancellationToken: token);
            rb.AddForce(Vector2.down * 20f + (Vector2)(target.position - transform.position).normalized * 7.5f, ForceMode2D.Impulse);
            anim.Play("SlamAttack");
            await UniTask.Delay((int)(1000f * 0.6f), cancellationToken: token);
            verticalLines?.Despawn();
            verticalLines = null;
            await UniTask.Delay((int)(1000f * 1.1f), cancellationToken: token);
        }

        await UniTask.Delay((int)(1000f * duration), cancellationToken: token);
        control.ChangeNextState();

    }
    UIParticle verticalLines;
    int attackIndex;
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
                $"SequenceAttack2-{attackIndex}",
                transform,
                coll.transform,
                Random.Range(0.9f, 1.1f) * damageMultiplier * control.adjustedAttack,
                hitPoint,
                new string[1] { "Hit2" },
                HitData.StaggerType.None
            );
            if (attackIndex == 3)
            {
                hitData.isCannotParry = true;
                hitData.staggerType = HitData.StaggerType.Large;
            }
            else
            {
                hitData.isCannotParry = false;
                hitData.staggerType = HitData.StaggerType.Middle;
            }
            GameManager.I.onHit.Invoke
            (
                hitData
            );
        }
    }


}
