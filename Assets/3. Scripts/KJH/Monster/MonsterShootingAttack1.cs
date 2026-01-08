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
    bool _once;
    public override async UniTask Enter(CancellationToken token)
    {

        if (bulletControl == null) bulletControl = FindAnyObjectByType<BulletControl>();
        await UniTask.Yield(token);
        Activate(token).Forget();
        randomedBulletPaterns.Clear();
        for (int i = 0; i < bulletPaterns.Count; i++)
        {
            randomedBulletPaterns.Add(bulletPaterns[i]);
            var pattern = randomedBulletPaterns[i];
            pattern.startTime = Random.Range(0.9f, 1.1f) * pattern.startTime;
            pattern.force = Random.Range(0.9f, 1.1f) * pattern.force;
            randomedBulletPaterns[i] = pattern;
        }
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
    List<BulletControl.BulletPatern> randomedBulletPaterns = new List<BulletControl.BulletPatern>();
    public async UniTask Activate(CancellationToken token)
    {
        if (!_once)
        {
            _once = true;
            control.SetCoolTime(MonsterControl.State.ShootingAttack1, 10f);
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
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
            moveDirection = transform.position - target.position;
            moveDirection.y = 0;
            moveDirection.Normalize();
            float repositionDuration = 1.8f;

            // 만약 뒤가 (절벽으로 가로막히거나 낭떠러지라면) && (전방에 충분한 공간이 있다면)
            bool isBackBlocked = false;
            bool isFrontClear = false;


            // 뒤 절벽 체크
            rayOrigin = transform.position + control.height * Vector3.up;
            rayDirection = moveDirection;
            rayLength = 4f * control.width;
            checkRay.origin = rayOrigin;
            checkRay.direction = rayDirection;
            CheckRayHit = Physics2D.Raycast(checkRay.origin, checkRay.direction, rayLength, control.groundLayer);
            if (CheckRayHit.collider != null)
            {
                isBackBlocked = true;
            }
            // 앞 절벽 체크
            rayDirection = -moveDirection;
            rayLength = 10f * control.width;
            checkRay.origin = rayOrigin;
            checkRay.direction = rayDirection;
            CheckRayHit = Physics2D.Raycast(checkRay.origin, checkRay.direction, rayLength, control.groundLayer);
            if (CheckRayHit.collider == null)
            {
                isFrontClear = true;
            }
            // 뒤 낭떠러지 체크
            if (!isBackBlocked)
                for (int i = 0; i < 4; i++)
                {
                    rayOrigin = transform.position + (control.height * Vector3.up) + ((1 + i) * control.width * (Vector3)moveDirection);
                    rayDirection = Vector3.down;
                    rayLength = 1.1f * control.height;
                    checkRay.origin = rayOrigin;
                    checkRay.direction = rayDirection;
                    CheckRayHit = Physics2D.Raycast(checkRay.origin, checkRay.direction, rayLength, control.groundLayer);
                    if (CheckRayHit.collider == null)
                    {
                        isBackBlocked = true;
                        Debug.DrawRay(rayOrigin, rayLength * rayDirection, Color.white, 1f);
                        break;
                    }
                }
            // 앞 낭떠러지 체크
            if (isFrontClear) // 위에서 레이캐스트로 앞쪽 벽이 없다는 것이 확인된 경우에만 진행
            {
                // 몬스터 앞쪽으로 일정 거리(예: 너비의 2~3배) 지점을 체크
                for (int i = 0; i < 3; i++)
                {
                    // 몬스터의 눈 높이 정도에서 앞쪽(target 방향)으로 i만큼 떨어진 위치에서 아래로 레이 발사
                    rayOrigin = transform.position + (control.height * Vector3.up) + ((4.5f + i) * control.width * (Vector3)(-moveDirection));
                    rayDirection = Vector3.down;
                    rayLength = 1.1f * control.height;
                    checkRay.origin = rayOrigin;
                    checkRay.direction = rayDirection;
                    CheckRayHit = Physics2D.Raycast(checkRay.origin, checkRay.direction, rayLength, control.groundLayer);
                    if (CheckRayHit.collider == null)
                    {
                        isFrontClear = false;
                        break;
                    }
                }
            }

            //Debug.Log($"{isBackBlocked} , {isFrontClear}");

            float moveSpeedMulti2 = 1f;
            // 위 여러 검사결과로 판단하여. 뒤 대신 전방으로 길게 직진해야 한다면.
            if (isBackBlocked && isFrontClear)
            {
                repositionDuration = 2.8f;
                moveSpeedMulti2 = 1.4f;
                moveDirection = -moveDirection;
                //Debug.DrawRay(transform.position, 6f * moveDirection, Color.red, 3f);
            }
            if (!isBackBlocked)
            {
                rb.AddForce(moveDirection * 6.8f, ForceMode2D.Impulse);
            }
            while (Time.time - startTime < repositionDuration)
            {
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
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
                if (stopWall) break;

                // AddForce방식으로 캐릭터 이동
                if (!stopWall)
                    if (dot < control.data.MoveSpeed)
                    {
                        float multiplier = 1.2f * (control.data.MoveSpeed - dot) + 1.1f;
                        rb.AddForce(multiplier * moveDirection * 7.4f * moveSpeedMulti2 * (control.data.MoveSpeed + 7.905f) / 1.1f);
                    }
                if (!condition) break;
            }
        }
        moveDirection = target.position - transform.position;
        moveDirection.y = 0;
        moveDirection.Normalize();
        rb.AddForce(moveDirection * 4.6f);
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
            if (Random.value < 0.3f)
            {
                // 너무 가까워서 취소
                await UniTask.Yield(token);
                control.ChangeNextState();
                return;
            }
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
        Vector2 direction = target.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        if (direction.x > 0 && model.right.x < 0)
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (direction.x < 0 && model.right.x > 0)
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);




        // -----------위치 재조정 끝------------




        if (control.isDie) return;
        anim.Play("ShootingAttack");
        ShootingAnimationLoop(token).Forget();
        await UniTask.Delay((int)(1000f * (0.3f * animationWaitSecond)), cancellationToken: token);
        particle = ParticleManager.I.PlayParticle("DarkCharge", transform.position + 0.5f * control.height * Vector3.up, Quaternion.identity);
        if (control.data.Type != MonsterType.Large && control.Type != MonsterType.Boss)
            particle.transform.localScale = 0.3f * Vector3.one;
        else
            particle.transform.localScale = Vector3.one;

        await UniTask.Delay((int)(1000f * (0.7f * animationWaitSecond)), cancellationToken: token);

        //


        await bulletControl.PlayBullet(randomedBulletPaterns, transform, target, token, control.data.Attack);
        await UniTask.Delay((int)(1000f * 3.7f), cancellationToken: token);
        particle?.Despawn();
        particle = null;
        control.ChangeNextState();
    }

    async UniTask ShootingAnimationLoop(CancellationToken token)
    {
        if (randomedBulletPaterns.Count < 2) return;
        for (int i = 1; i < randomedBulletPaterns.Count; i++)
        {
            await UniTask.Delay((int)(1000f * (randomedBulletPaterns[i].startTime - randomedBulletPaterns[i - 1].startTime)), cancellationToken: token);
            anim.Play("ShootingAttack");
            particle?.Despawn();
            particle = null;
            particle = ParticleManager.I.PlayParticle("DarkCharge", transform.position + 0.5f * control.height * Vector3.up, Quaternion.identity);
        }
    }



}
