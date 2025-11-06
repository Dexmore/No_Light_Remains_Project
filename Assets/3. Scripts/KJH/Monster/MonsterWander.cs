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
        await UniTask.Yield(cts.Token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();
        // if (Random.value < 0.33f)
        // {
        //     Activate(token).Forget();
        // }
        // else
        // {
        //     Activate2(token).Forget();
        // }
        isAnimation = false;
    }
    public async UniTask Activate(CancellationToken token)
    {
        float startTime = Time.time;
        moveDirection = Vector2.zero;
        if (Random.value <= 0.5f)
        {
            moveDirection = Vector2.right;
        }
        else
        {
            moveDirection = Vector2.left;
        }
        // 캐릭터 좌우 방향 설정
        if (moveDirection.x > 0 && model.right.x < 0)
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (moveDirection.x < 0 && model.right.x > 0)
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
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
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cts.Token);
        }
        control.ChangeNextState();
    }
    public async UniTask Activate2(CancellationToken token)
    {
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cts.Token);
    }
    public override void Exit()
    {
        base.Exit();
    }






}
