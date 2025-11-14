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
        await UniTask.Yield(cts.Token);
        isAnimation = false;
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

        if (stopDistance > 0 && control.HasCondition(MonsterControl.Condition.ClosePlayer))
        {
            await UniTask.Yield(cts.Token);
            control.ChangeNextState();
            return;
        }
        if (control.memories.Count == 0)
        {
            await UniTask.Yield(cts.Token);
            control.ChangeNextState();
            return;
        }
        target = control.memories.First().Key.transform;
        Vector2 addPos = Vector2.zero;
        if (stopDistance < 1f)
        {
            float distance = Mathf.Abs(target.position.x - transform.position.x);
            if(distance < 0.3f)
            {
                await UniTask.Delay(Random.Range(0, 200), cancellationToken: token);
                if(Random.value < 0.81f)
                    addPos = Random.Range(-3f, 3f) * Vector2.right;
                else
                {
                    if(Random.value < 0.5f)
                        addPos = Random.Range(6f , 12f) * Vector2.right;
                    else
                        addPos = Random.Range(6f , 12f) * Vector2.left;
                }
            }
            else
            {
                addPos = Random.Range(0f, 4f)  * (target.position - transform.position).normalized;
            }
        }
        Vector2[] result = await astar.Find((Vector2)target.position + addPos);
        if (result.Length <= 1)
        {
            await UniTask.Yield(cts.Token);
            control.ChangeNextState();
            return;
        }
        for (int i = 1; i < result.Length; i++)
        {
            if (i > 1 && Random.value < 0.25f)
            {
                Retry();
                return;
            }
            Vector2 segmentPos = result[i];
            Vector2 displacement = segmentPos - ((Vector2)transform.position + astar.offeset * Vector2.up);
            float distance = displacement.magnitude;
            Vector2 moveHorizontal = displacement;
            moveHorizontal.y = 0f;
            moveHorizontal.Normalize();
            float expectTime = 1.8f * (displacement.magnitude / control.data.MoveSpeed);
            float startTime = Time.time;
            if (moveHorizontal.x > 0 && model.right.x < 0)
                model.localRotation = Quaternion.Euler(0f, 0f, 0f);
            else if (moveHorizontal.x < 0 && model.right.x > 0)
                model.localRotation = Quaternion.Euler(0f, 180f, 0f);
            while (distance > 0.05f && Time.time - startTime < expectTime)
            {
                moveHorizontal = segmentPos - ((Vector2)transform.position + astar.offeset * Vector2.up);
                distance = moveHorizontal.magnitude;
                if (Mathf.Abs(moveHorizontal.x) <= 0.002f)
                {
                    await UniTask.Yield(cts.Token);
                    control.ChangeNextState();
                    return;
                }
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: token);
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
                        if (control.isGround)
                        {
                            if (!isAnimation)
                                if (control.isGround)
                                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Move"))
                                    {
                                        isAnimation = true;
                                        if (control.isDie) return;
                                        anim.Play("Move");
                                    }
                        }
                        else if (isAnimation)
                        {
                            isAnimation = false;
                            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                            {
                                if (control.isDie) return;
                                anim.Play("Idle");
                            }
                        }
                    }
                float sqrMagnitudeFinalTarget = ((Vector2)target.position - ((Vector2)transform.position + astar.offeset * Vector2.up)).sqrMagnitude;
                if (sqrMagnitudeFinalTarget < stopDistance * stopDistance)
                {
                    await UniTask.Delay(5, cancellationToken: token);
                    control.ChangeNextState();
                    return;
                }
                if (stopDistance > 0)
                    if (control.HasCondition(MonsterControl.Condition.ClosePlayer))
                    {
                        await UniTask.Delay(5, cancellationToken: token);
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
        anim.Play("Idle");
        await UniTask.Delay(Random.Range(500, 3000), cancellationToken: token);
        await UniTask.Yield(cts.Token);
        control.ChangeNextState();
    }





}
