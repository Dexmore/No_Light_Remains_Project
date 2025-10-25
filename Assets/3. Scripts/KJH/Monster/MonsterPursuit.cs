using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
public class MonsterPursuit : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Pursuit;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        Activate(token).Forget();
    }
    Transform target;
    bool isAnimation;
    public void Retry()
    {
        Activate(cts.Token).Forget();
    }
    public async UniTask Activate(CancellationToken token)
    {
        if (control.HasCondition(MonsterControl.Condition.ClosePlayer))
        {
            await UniTask.Yield(cts.Token);
            control.ChangeState(MonsterControl.State.Idle);
            return;
        }
        if (sensor.memories.Count == 0)
        {
            await UniTask.Yield(cts.Token);
            control.ChangeState(MonsterControl.State.Idle);
            return;
        }
        target = sensor.memories.First().Key.transform;
        Vector2[] result = await astar.Find((Vector2)target.position);
        if (result.Length <= 1)
        {
            await UniTask.Yield(cts.Token);
            control.ChangeState(MonsterControl.State.Idle);
            return;
        }
        for (int i = 1; i < result.Length; i++)
        {
            if (i > 1 && Random.value < 0.25f)
            {
                Retry();
                return;
            }
            Vector2 target = result[i];
            //Debug.Log(target);
            //Debug.Log((Vector2)transform.position + astar.offeset * Vector2.up);
            Vector2 displacement = target - ((Vector2)transform.position + astar.offeset * Vector2.up);
            float distance = displacement.magnitude;
            Vector2 moveHorizontal = displacement;
            moveHorizontal.y = 0f;
            moveHorizontal.Normalize();
            float expectTime = 1.8f * (displacement.magnitude / control.data.MoveSpeed);
            float startTime = Time.time;
            while (distance > 0.12f && Time.time - startTime < expectTime)
            {
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                moveHorizontal = target - ((Vector2)transform.position + astar.offeset * Vector2.up);
                distance = moveHorizontal.magnitude;
                moveHorizontal.y = 0f;
                moveHorizontal.Normalize();
                float dot = Vector2.Dot(rb.linearVelocity, moveHorizontal);
                // 벽 향해서 전진하는 버그 막기
                bool stopWall = false;
                if (control.collisions.Count > 0)
                    foreach (var element in control.collisions)
                        if (Mathf.Abs(element.Value.y - control.transform.position.y) >= 0.09f * control.height)
                        {
                            if (element.Value.x - control.transform.position.x > 0.25f * control.width && moveHorizontal.x > 0)
                            {
                                stopWall = true;
                                break;
                            }
                            else if (element.Value.x - control.transform.position.x < -0.25f * control.width && moveHorizontal.x < 0)
                            {
                                stopWall = true;
                                break;
                            }
                        }
                if (!stopWall)
                    if (dot < control.data.MoveSpeed)
                    {
                        float multiplier = (control.data.MoveSpeed - dot) + 1f;
                        rb.AddForce(multiplier * moveHorizontal * (control.data.MoveSpeed + 4.905f) / 1.25f);
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
                        // 캐릭터 좌우 방향 설정
                        if (moveHorizontal.x > 0 && model.right.x < 0)
                            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
                        else if (moveHorizontal.x < 0 && model.right.x > 0)
                            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    }
                if (control.HasCondition(MonsterControl.Condition.ClosePlayer))
                {
                    await UniTask.Yield(cts.Token);
                    control.ChangeNextState();
                    return;
                }
            }
        }
        await UniTask.Delay(50, cancellationToken: token);
        if (Random.value < 0.75f)
        {
            Retry();
            return;
        }
        //Debug.Log("길 찾기");
        anim.Play("Idle");
        await UniTask.Delay(Random.Range(500,3000), cancellationToken: token);
        await UniTask.Yield(cts.Token);
        control.ChangeNextState();
    }






}
