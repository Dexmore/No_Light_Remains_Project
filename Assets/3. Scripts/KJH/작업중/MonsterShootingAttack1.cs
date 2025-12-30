using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterShootingAttack1 : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.ShootingAttack1;
    BulletControl bulletControl;
    public float range;
    public float animationWaitSecond;
    public override async UniTask Enter(CancellationToken token)
    {
        if (bulletControl == null) bulletControl = FindAnyObjectByType<BulletControl>();
        await UniTask.Yield(token);
        Activate(token).Forget();
    }
    public override void Exit()
    {
        base.Exit();
        particle?.Despawn();
        particle = null;
    }
    // 낭떠러지 체크용
    Vector2 rayOrigin;
    Vector2 rayDirection;
    float rayLength;
    Ray2D checkRay;
    RaycastHit2D CheckRayHit;
    Particle particle;
    public List<BulletControl.BulletPatern> bulletPaterns = new List<BulletControl.BulletPatern>();
    public async UniTask Activate(CancellationToken token)
    {
        await UniTask.Yield(token);
        if (control.isDie) return;
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            anim.Play("Idle");

        //

        Transform target;
        target = control.memories.First().Key.transform;
        float dist = Vector3.Distance(target.position, transform.position);
        float distX = Mathf.Abs(target.position.x - transform.position.x);
        float distY = Mathf.Abs(target.position.y - transform.position.y);
        //float tempDist = Mathf.Clamp(0.333f * control.findRadius, 0f, 3f * control.width);
        float tempDist = 0.5f * (Mathf.Clamp(control.findRadius, 0f, 10f) + (3f * control.width));
        bool condition = false;
        if (dist < 0.6f * tempDist)
        {
            condition = true;
        }
        if (dist > 1.1f * range + 2f)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        if (distY > 0.26 * range)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        if (distX > 1.1f * range + 2f)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }

        float startTime = Time.time;
        Vector2 moveDirection;
        bool once = false;

        // 너무 가까우면 살짝 뒤로 이동
        if (condition)
        {
            while (Time.time - startTime < 1.8f)
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
                        rb.AddForce(multiplier * moveDirection * 4f * (control.data.MoveSpeed + 6.905f) / 1.25f);
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

        dist = Vector3.Distance(target.position, transform.position);
        if (dist <= 0.2f * tempDist)
        {
            // 너무 가까워서 취소
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

        Vector2 direction = target.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        if (direction.x > 0 && model.right.x < 0)
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (direction.x < 0 && model.right.x > 0)
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        if (control.isDie) return;

        //
        if (control.isDie) return;
        anim.Play("ShootingAttack");
        await UniTask.Delay((int)(1000f * (0.3f * animationWaitSecond)), cancellationToken: token);
        particle = ParticleManager.I.PlayParticle("DarkCharge", transform.position + 0.5f * control.height * Vector3.up, Quaternion.identity);
        if (control.data.Type != MonsterType.Large && control.Type != MonsterType.Boss)
            particle.transform.localScale = 0.3f * Vector3.one;
        else
            particle.transform.localScale = Vector3.one;
        
        await UniTask.Delay((int)(1000f * (0.7f * animationWaitSecond)), cancellationToken: token);

        //


        await bulletControl.PlayBullet(bulletPaterns, transform, target, token, control.data.Attack);
        await UniTask.Delay((int)(1000f * 4f), cancellationToken: token);
        particle?.Despawn();
        particle = null;
        control.ChangeNextState();
    }



}
