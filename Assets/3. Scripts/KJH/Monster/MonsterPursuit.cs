using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
public class MonsterPursuit : MonsterState
{
    public float stopDistance = 0f;
    public override MonsterControl.State mapping => MonsterControl.State.Pursuit;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(token);
        isMoveAnimation = false;
        Activate(token).Forget();
    }
    Transform target;
    bool isMoveAnimation;
    public void Retry()
    {
        Activate(cts.Token).Forget();
    }
    Ray2D checkCliffRay;
    RaycastHit2D CheckCliffHit;
    public async UniTask Activate(CancellationToken token)
    {

        if (stopDistance > 0 && control.HasCondition(MonsterControl.Condition.ClosePlayer))
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        if (control.memories.Count == 0)
        {
            await UniTask.Yield(token);
            if (Random.value < 0.5f)
            {
                control.ChangeNextState();
            }
            else if (!control.IsCoolTime(MonsterControl.State.Wander))
            {
                control.ChangeState(MonsterControl.State.Wander);
            }
            return;
        }
        target = control.memories.First().Key.transform;
        Vector2 addPos = Vector2.zero;
        if (stopDistance < 1f)
        {
            float distance = Mathf.Abs(target.position.x - transform.position.x);
            if (distance < 0.3f)
            {
                await UniTask.Delay(Random.Range(0, 1000), cancellationToken: token);
                if (Random.value < 0.81f)
                    addPos = Random.Range(-3f, 3f) * Vector2.right;
                else
                {
                    if (Random.value < 0.5f)
                        addPos = Random.Range(6f, 12f) * Vector2.right;
                    else
                        addPos = Random.Range(6f, 12f) * Vector2.left;
                }
            }
            else
            {
                addPos = Random.Range(0f, 4f) * (target.position - transform.position).normalized;
            }
        }
        Vector2[] result = await astar.Find((Vector2)target.position + addPos, token);
        if (result.Length <= 1)
        {
            await UniTask.Yield(token);
            if (Random.value < 0.5f)
            {
                control.ChangeNextState();
            }
            else if (!control.IsCoolTime(MonsterControl.State.Wander))
            {
                control.ChangeState(MonsterControl.State.Wander);
            }
            //Debug.Log("3");
            return;
        }


        // 경로를 따라 이동
        for (int i = 1; i < result.Length; i++)
        {
            if (i > 1 && Random.value < 0.25f)
            {
                Retry();
                return;
            }

            //DebugExtension.DebugWireSphere(result[i], Color.white, 0.1f, 1f, true);
            Vector2 targetPos = result[i];
            Vector2 moveDirection = targetPos - ((Vector2)transform.position + astar.offeset * Vector2.up);
            float length = moveDirection.magnitude;
            float xLength = Mathf.Abs(moveDirection.x);
            float yLength = Mathf.Abs(moveDirection.y);

            // 캐릭터 좌우 방향 설정
            if (moveDirection.x > 0 && model.right.x < 0)
                model.localRotation = Quaternion.Euler(0f, 0f, 0f);
            else if (moveDirection.x < 0 && model.right.x > 0)
                model.localRotation = Quaternion.Euler(0f, 180f, 0f);




            Vector2 rayOrigin = transform.position + 0.1f * control.width * model.right + 0.1f * control.height * Vector3.up;
            Vector2 rayDirection = 0.14f * (Vector2)model.right + 0.86f * moveDirection.normalized;
            float rayLength = 0.7f * control.width + 0.7f * control.height;
            checkCliffRay.origin = rayOrigin;
            checkCliffRay.direction = rayDirection;
            CheckCliffHit = Physics2D.Raycast(checkCliffRay.origin, checkCliffRay.direction, rayLength, control.groundLayer);
            float angle = 999;
            if (CheckCliffHit.collider != null)
            {
                Vector2 normal = CheckCliffHit.normal;
                angle = Vector2.Angle(moveDirection, -normal);
            }

            if (angle < 45)
            {
                //Debug.Log("점프 포인트1 (절벽)");
                await UniTask.Delay(5, cancellationToken: token);
                if (Random.value < 0.5f)
                {
                    control.ChangeNextState();
                }
                else if (!control.IsCoolTime(MonsterControl.State.Wander))
                {
                    control.ChangeState(MonsterControl.State.Wander);
                }
            }

            /////////// 징검다리 /////////

            if (i > 1 && xLength > astar.unit * 1.8f)
            {
                //Debug.Log("점프 포인트2 (징검다리)");

            }


            /////////// 낭떠러지 /////////
            rayOrigin = transform.position + control.width * model.right + 0.2f * control.height * Vector3.up;
            rayDirection = Vector3.down;
            rayLength = 2f * control.height;
            checkCliffRay.origin = rayOrigin;
            checkCliffRay.direction = rayDirection;
            //Debug.DrawRay(checkCliffRay.origin, rayLength * checkCliffRay.direction, Color.green, 1f);
            CheckCliffHit = Physics2D.Raycast(checkCliffRay.origin, checkCliffRay.direction, rayLength, control.groundLayer);
            if (CheckCliffHit.collider == null)
            {
                if (control.isDie) return;
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    anim.Play("Idle");
                if (Random.value < 5f * Time.deltaTime)
                {
                    await UniTask.Delay(5, cancellationToken: token);
                    //Debug.Log("점프 포인트1 (낭떠러지)");
                    if (Random.value < 0.5f)
                    {
                        control.ChangeNextState();
                    }
                    else if (!control.IsCoolTime(MonsterControl.State.Wander))
                    {
                        control.ChangeState(MonsterControl.State.Wander);
                    }
                    return;
                }
            }
            //////////////////////////////


            float startTime = Time.time;
            float expectTime = (length / control.MoveSpeed) * 1.5f;
            while (Time.time - startTime < expectTime)
            {
                moveDirection = targetPos - ((Vector2)transform.position + astar.offeset * Vector2.up);
                moveDirection.y *= 0.4f;
                moveDirection.y = Mathf.Clamp(moveDirection.y, 0f, control.height);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                if (moveDirection.sqrMagnitude < 0.08f * 0.08f) break;

                //////////////////////////////////////////
                //rb.linearVelocity = control.MoveSpeed * moveDirection.normalized;
                moveDirection.Normalize();
                float dot = Vector2.Dot(rb.linearVelocity, moveDirection);
                if (dot < control.data.MoveSpeed)
                {
                    float multiplier = (control.data.MoveSpeed - dot) + 1f;
                    rb.AddForce(multiplier * moveDirection * (control.data.MoveSpeed + 4.905f) / 1.25f);
                    // 애니매이션처리
                    if (control.isGround)
                    {
                        if (!isMoveAnimation)
                            if (control.isGround)
                            {
                                isMoveAnimation = true;
                                anim.Play("Move");
                            }
                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Move"))
                        {
                            isMoveAnimation = true;
                            anim.Play("Move");
                        }
                    }
                    else if (isMoveAnimation)
                    {
                        isMoveAnimation = false;
                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                        {
                            anim.Play("Idle");
                        }
                    }
                }
                //////////////////////////////////////////


            }
        }



        // for (int i = 1; i < result.Length; i++)
        // {
        //     if (i > 1 && Random.value < 0.25f)
        //     {
        //         Retry();
        //         return;
        //     }


        //     Vector2 segmentPos = result[i];
        //     Vector2 displacement = segmentPos - ((Vector2)transform.position + astar.offeset * Vector2.up);
        //     float distance = displacement.magnitude;
        //     Vector2 moveHorizontal = displacement;
        //     moveHorizontal.y = 0f;
        //     moveHorizontal.Normalize();
        //     float expectTime = 1.8f * (displacement.magnitude / control.data.MoveSpeed);
        //     float startTime = Time.time;
        //     if (moveHorizontal.x > 0 && model.right.x < 0)
        //         model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        //     else if (moveHorizontal.x < 0 && model.right.x > 0)
        //         model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        //     while (distance > 0.05f && Time.time - startTime < expectTime)
        //     {
        //         moveHorizontal = segmentPos - ((Vector2)transform.position + astar.offeset * Vector2.up);
        //         distance = moveHorizontal.magnitude;
        //         await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: token);
        //         moveHorizontal.y = 0f;
        //         moveHorizontal.Normalize();
        //         float dot = Vector2.Dot(rb.linearVelocity, moveHorizontal);
        //         // 벽 향해서 전진하는 버그 막기
        //         bool stopWall = false;
        //         if (control.collisions.Count > 0)
        //             foreach (var element in control.collisions)
        //                 if (Mathf.Abs(element.Value.y - control.transform.position.y) >= 0.09f * control.height)
        //                 {
        //                     if (element.Value.x - control.transform.position.x > 0.25f * control.width && moveHorizontal.x > 0)
        //                     {
        //                         stopWall = true;
        //                         break;
        //                     }
        //                     else if (element.Value.x - control.transform.position.x < -0.25f * control.width && moveHorizontal.x < 0)
        //                     {
        //                         stopWall = true;
        //                         break;
        //                     }
        //                 }
        //         if (!stopWall)
        //             if (dot < control.data.MoveSpeed)
        //             {
        //                 float multiplier = (control.data.MoveSpeed - dot) + 1f;
        //                 if(!control.isStagger)
        //                     rb.AddForce(multiplier * moveHorizontal * (control.data.MoveSpeed + 4.905f) / 1.25f);
        //                 if (control.isGround)
        //                 {
        //                     if (!isMoveAnimation)
        //                         if (control.isGround)
        //                             if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Move"))
        //                             {
        //                                 isMoveAnimation = true;
        //                                 if (control.isDie) return;
        //                                 anim.Play("Move");
        //                             }
        //                 }
        //                 else if (isMoveAnimation)
        //                 {
        //                     isMoveAnimation = false;
        //                     if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        //                     {
        //                         if (control.isDie) return;
        //                         anim.Play("Idle");
        //                     }
        //                 }
        //             }
        //         float sqrMagnitudeFinalTarget = ((Vector2)target.position - ((Vector2)transform.position + astar.offeset * Vector2.up)).sqrMagnitude;
        //         if (sqrMagnitudeFinalTarget < stopDistance * stopDistance)
        //         {
        //             await UniTask.Delay(5, cancellationToken: token);
        //             control.ChangeNextState();
        //             return;
        //         }
        //         if (stopDistance > 0)
        //             if (control.HasCondition(MonsterControl.Condition.ClosePlayer))
        //             {
        //                 await UniTask.Delay(5, cancellationToken: token);
        //                 control.ChangeNextState();
        //                 return;
        //             }
        //     }
        // }


        if (isMoveAnimation)
        {
            isMoveAnimation = false;
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                anim.Play("Idle");
            }
        }
        await UniTask.Delay(50, cancellationToken: token);
        if (Random.value < 0.75f)
        {
            Retry();
            return;
        }
        await UniTask.Delay(Random.Range(500, 3000), cancellationToken: token);
        await UniTask.Yield(token);
        control.ChangeNextState();
    }





}
