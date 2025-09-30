using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
public class DefaultBiteAttack : MonsterState
{
    public Vector2 durationRange;
    float duration;
    int multiHitCount = 1;
    public override MonsterControl.State mapping => MonsterControl.State.BiteAttack;
    public override async UniTask Enter(CancellationToken token)
    {
        control.attackRange.onTriggetStay2D += Handler_TriggerStay2D;
        attackedColliders.Clear();
        await UniTask.Yield(cts.Token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();
        anim.Play("BAttack");
    }
    public override async UniTask Activate(CancellationToken token)
    {
        // 이동
        if (sensor.memories.Count == 0)
        {
            await UniTask.Yield(cts.Token);
            control.ChangeState(MonsterControl.State.Idle);
            return;
        }
        Transform target = sensor.memories.First().Key.transform;
        Vector2 moveDirection = target.position - transform.position;
        if (Random.value < 0.5f && moveDirection.magnitude > sensor.closeRadius * 0.8f)
        {
            float time = Time.time;
            float slow = Random.Range(0.3f, 1.2f);
            while (Time.time - time < 0.2f)
            {
                moveDirection = target.position - transform.position;
                moveDirection.y = 0;
                moveDirection.Normalize();
                float dot = Vector2.Dot(rb.linearVelocity, moveDirection);
                // 캐릭터 좌우 방향 설정
                if (moveDirection.x > 0 && model.right.x < 0)
                {
                    model.localRotation = Quaternion.Euler(0f, 0f, 0f);
                }
                else if (moveDirection.x < 0 && model.right.x > 0)
                {
                    model.localRotation = Quaternion.Euler(0f, 180f, 0f);
                }
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
                        rb.AddForce(multiplier * slow * moveDirection * (control.data.MoveSpeed + 4.905f) / 1.25f);
                    }
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cts.Token);
            }
        }
        else
        {
            await UniTask.Delay((int)(1000f * 0.2f), cancellationToken: token);
        }
        await UniTask.Delay((int)(1000f * (duration - 0.2f)), cancellationToken: token);
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
            EventManager.I.onAttack(new EventManager.AttackData(transform, coll.transform, Random.Range(0.9f, 1.1f) * control.data.Attack));
        }
    }




}
