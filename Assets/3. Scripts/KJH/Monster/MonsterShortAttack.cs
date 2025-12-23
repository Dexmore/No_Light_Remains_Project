using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
public class MonsterShortAttack : MonsterState
{
    public float damageMultiplier = 1f;
    public HitData.StaggerType staggerType;
    public Vector2 durationRange;
    public float range = 1.4f;
    float duration;
    int multiHitCount = 1;
    GameObject chafe;
    public override MonsterControl.State mapping => MonsterControl.State.ShortAttack;
    // 낭떠러지 체크용
    Vector2 rayOrigin;
    Vector2 rayDirection;
    float rayLength;
    Ray2D checkRay;
    RaycastHit2D CheckRayHit;
    public override async UniTask Enter(CancellationToken token)
    {
        control.attackRange.onTriggetStay2D += Handler_TriggerStay2D;
        attackedColliders.Clear();
        if (transform.Find("Chafe") != null)
            chafe = transform.Find("Chafe").gameObject;
        else
            chafe = transform.GetChild(0).Find("Chafe").gameObject;
        chafe?.SetActive(false);
        await UniTask.Yield(token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();
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
        Vector2 moveDirection;
        float startTime;
        bool once;
        target = control.memories.First().Key.transform;
        moveDirection = target.position - transform.position;
        moveDirection.y = 0;
        moveDirection.Normalize();
        startTime = Time.time;
        float dist = Mathf.Abs(target.position.x - transform.position.x);
        RaycastHit2D raycastHit = Physics2D.Linecast((Vector2)control.eye.position, target.position, control.groundLayer);
        if(raycastHit.collider != null)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        if (range < dist)
        {
            once = false;
            while (Time.time - startTime < 2f)
            {
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
                moveDirection = target.position - transform.position;
                moveDirection.y = 0;
                moveDirection.Normalize();
                dist = Mathf.Abs(target.position.x - transform.position.x);
                // 캐릭터 방향 설정
                if (!once)
                {
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
                        float speedFactor1 = 0.9f;
                        if (control.prevState == MonsterControl.State.MovingAttack)
                            speedFactor1 = 1.3f;
                        rb.AddForce(multiplier * moveDirection * speedFactor1 * (control.data.MoveSpeed + 4.905f) / 1.25f);
                    }
                if (dist <= range) break;
            }
            if (range < dist)
            {
                await UniTask.Yield(token);
                control.ChangeNextState();
                return;
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
        anim.Play("ShortAttack");
        await UniTask.Delay((int)(1000f * (duration)), cancellationToken: token);
        control.ChangeNextState();
    }
    public override void Exit()
    {
        base.Exit();
        control.attackRange.onTriggetStay2D -= Handler_TriggerStay2D;
        chafe?.SetActive(true);
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
                    "ShortAttack",
                    transform,
                    coll.transform,
                    Random.Range(0.9f, 1.1f) * damageMultiplier * control.adjustedAttack,
                    hitPoint,
                    new string[1]{"Hit2"},
                    staggerType
                )
            );
        }
    }




}
