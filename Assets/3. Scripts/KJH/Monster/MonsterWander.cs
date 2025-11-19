using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterWander : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Wander;
    public Vector2 durationRange;
    float duration;
    bool isAnimation;
    Vector2 moveDirection = Vector2.zero;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(token);
        duration = Random.Range(durationRange.x, durationRange.y);
        ctsWander?.Cancel();
        ctsWander = new CancellationTokenSource();
        var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(token, ctsWander.Token);
        anim.Play("Idle");
        if (Random.value <= 1f)
            Activate(ctsLink.Token).Forget();
        else
            Activate2(ctsLink.Token).Forget();
        isAnimation = false;
    }
    CancellationTokenSource ctsWander = new CancellationTokenSource();
    public async UniTask Activate(CancellationToken token)
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            anim.Play("Idle");
        float startTime = Time.time;
        moveDirection = Vector2.zero;
        if (Random.value <= 0.5f)
            moveDirection = Vector2.right;
        else
            moveDirection = Vector2.left;
        // 캐릭터 좌우 방향 설정
        if (moveDirection.x > 0 && model.right.x < 0)
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (moveDirection.x < 0 && model.right.x > 0)
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        int tempCount = 0;
        while (Time.time - startTime < duration)
        {
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
            if (stopWall)
            {
                if (control.isDie) return;
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    anim.Play("Idle");
                if (Random.value < 5f * Time.deltaTime)
                {
                    await UniTask.Delay(5, cancellationToken: token);
                    control.ChangeNextState();
                    return;
                }
            }
            // 버그 방지
            if (rb.linearVelocity.magnitude < 0.001f)
            {
                tempCount++;
                if (tempCount > 5)
                {
                    if (control.isDie) return;
                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                        anim.Play("Idle");
                    if (Random.value < 10f * Time.deltaTime)
                    {
                        await UniTask.Delay(5, cancellationToken: token);
                        control.ChangeNextState();
                        return;
                    }
                }
            }
            else tempCount = 0;
            // AddForce방식으로 캐릭터 이동
            if (!stopWall)
                if (dot < control.data.MoveSpeed)
                {
                    float multiplier = (control.data.MoveSpeed - dot) + 1f;
                    rb.AddForce(multiplier * moveDirection * (control.data.MoveSpeed + 4.905f) / 1.25f);
                    // 애니매이션처리
                    if (control.isGround)
                    {
                        if (!isAnimation)
                            if (control.isGround)
                            {
                                isAnimation = true;
                                anim.Play("Move");
                            }
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
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
        control.ChangeNextState();
    }
    List<Vector2[]> paths = new List<Vector2[]>();
    RaycastHit2D[] grounds = new RaycastHit2D[10];
    public async UniTask Activate2(CancellationToken token)
    {
        
        // if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        //     anim.Play("Idle");
        // await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

        // // 3. Astar를 통해 12개의 목적지 후보중 총 경로 길이가 가장 짧은 것 고르기
        // paths.Clear();
        // for (int i = 0; i < 12; i++) paths.Add(null);
        // Vector2 findPos = Vector2.zero;
        // Vector2[] findPath = null;
        // float minTotalLength = float.MaxValue;
        // for (int i = 0; i < paths.Count; i++)
        // {
        //     // Astar로 지정할 임시목적지 선정
        //     Vector2 pos = ((Vector2)transform.position) + control.data.MoveSpeed * Random.insideUnitCircle * Random.Range(5f, 10f);
        //     int count = Physics2D.RaycastNonAlloc(pos + 5f * Vector2.up, Vector2.down, grounds, 10f, control.groundLayer);
        //     //Debug.DrawRay(pos + 5f * Vector2.up, 5f * Vector2.down, Color.blue, 2f);
        //     if (count == 0) continue;
        //     int rnd2 = Random.Range(0, count);
        //     for (int k = 0; k < 20; k++)
        //     {
        //         if (rnd2 < grounds.Length)
        //         {
        //             break;
        //         }
        //         else
        //         {
        //             rnd2 = Random.Range(0, count);
        //         }
        //     }
        //     if (rnd2 >= grounds.Length) continue;
        //     Vector2 pos2 = grounds[rnd2].point + 0.02f * Vector2.up;
        //     paths[i] = await astar.Find(pos2);
        //     if (paths[i].Length <= 1) continue;
        //     float leng = 0f;
        //     for (int k = 1; k < paths[i].Length; k++)
        //     {
        //         leng += (paths[i][k] - paths[i][k - 1]).magnitude;
        //     }
        //     if (leng == 0) continue;
        //     if (leng < minTotalLength)
        //     {
        //         minTotalLength = leng;
        //         findPath = paths[i];
        //         findPos = pos2;
        //     }
        // }
        // if (findPath == null)
        // {
        //     await UniTask.Yield(token);
        //     control.ChangeNextState();
        //     return;
        // }

        // // 경로를 따라 이동
        // bool isAnimation = false;
        // float startTime = Time.time;
        // int tempCount = 0;

        // for (int i = 1; i < findPath.Length; i++)
        // {
        //     Vector2 segmentPos = findPath[i];
        //     Vector2 displacement = segmentPos - ((Vector2)transform.position + astar.offeset * Vector2.up);
        //     float distance = displacement.magnitude;
        //     Vector2 moveHorizontal = displacement;
        //     moveHorizontal.y = 0f;
        //     moveHorizontal.Normalize();
        //     float expectTime = 1.8f * (displacement.magnitude / control.data.MoveSpeed);
        //     float startTime2 = Time.time;
        //     if (moveHorizontal.x > 0 && model.right.x < 0)
        //         model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        //     else if (moveHorizontal.x < 0 && model.right.x > 0)
        //         model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        //     while (distance > 0.05f && Time.time - startTime2 < expectTime && Time.time - startTime < duration * 2f)
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
        //         if (stopWall)
        //         {
        //             if (control.isDie) return;
        //             if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        //                 anim.Play("Idle");
        //             if (Random.value < 5f * Time.deltaTime)
        //             {
        //                 await UniTask.Delay(5, cancellationToken: token);
        //                 control.ChangeNextState();
        //                 return;
        //             }
        //         }
        //         // 버그 방지
        //         Debug.Log(transform.name);
        //         if (rb.linearVelocity.magnitude < 0.001f)
        //         {
        //             tempCount++;
        //             if (tempCount > 5)
        //             {
        //                 if (control.isDie) return;
        //                 if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        //                     anim.Play("Idle");
        //                 if (Random.value < 10f * Time.deltaTime)
        //                 {
        //                     await UniTask.Delay(5, cancellationToken: token);
        //                     control.ChangeNextState();
        //                     return;
        //                 }
        //             }
        //         }
        //         if (!stopWall)
        //             if (dot < control.data.MoveSpeed)
        //             {
        //                 float multiplier = (control.data.MoveSpeed - dot) + 1f;
        //                 rb.AddForce(multiplier * moveDirection * (control.data.MoveSpeed + 4.905f) / 1.25f);
        //                 if (control.isGround)
        //                 {
        //                     if (!isAnimation)
        //                         if (control.isGround)
        //                             if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Move"))
        //                             {
        //                                 isAnimation = true;
        //                                 if (control.isDie) return;
        //                                 anim.Play("Move");
        //                             }
        //                 }
        //                 else if (isAnimation)
        //                 {
        //                     isAnimation = false;
        //                     if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        //                     {
        //                         if (control.isDie) return;
        //                         anim.Play("Idle");
        //                     }
        //                 }
        //             }
        //     }
        // }
        // await UniTask.Delay((int)(100f), cancellationToken: token);
        // control.ChangeNextState();
    }
    public override void Exit()
    {
        base.Exit();
    }






}
