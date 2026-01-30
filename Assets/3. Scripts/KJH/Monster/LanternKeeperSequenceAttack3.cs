using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class LanternKeeperSequenceAttack3 : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.SequenceAttack3;
    public float range;
    public Vector2 durationRange;
    float duration;
    bool _once = false;
    [SerializeField] LightPillar lightPillar;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();
    }
    public override void Exit()
    {
        base.Exit();
    }
    // 낭떠러지 체크용
    Vector2 rayOrigin;
    Vector2 rayDirection;
    float rayLength;
    Ray2D checkRay;
    RaycastHit2D CheckRayHit;

    public async UniTask Activate(CancellationToken token)
    {

        if (!_once)
        {
            _once = true;
            float coolTime = 0;
            for (int i = 0; i < control.patterns.Length; i++)
            {
                for (int j = 0; j < control.patterns[i].frequencies.Length; j++)
                {
                    if (mapping == control.patterns[i].frequencies[j].state)
                    {
                        coolTime = control.patterns[i].frequencies[j].coolTime;
                        break;
                    }
                }
            }
            control.SetCoolTime(MonsterControl.State.RareAttack, Random.Range(0.1f * coolTime, 0.5f * coolTime));
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }



        await UniTask.Yield(token);
        if (control.isDie) return;
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            anim.Play("Idle");

        Transform target;
        target = control.memories.First().Key.transform;
        float dist = Vector3.Distance(target.position, transform.position);
        float distX = Mathf.Abs(target.position.x - transform.position.x);
        float distY = Mathf.Abs(target.position.y - transform.position.y);
        float tempDist = 0.5f * (Mathf.Clamp(control.findRadius, 0f, 10f) + (3f * control.width));
        bool condition = false;
        if (dist < 0.6f * tempDist)
        {
            condition = true;
        }
        if (dist > 1.8f * range + 4f)
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
        if (distX > 1.8f * range + 4f)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }

        float startTime = Time.time;
        Vector2 moveDirection;
        bool once = false;

        // 적당히 넓게 트인 먼곳으로 이동
        if (condition)
        {
            moveDirection = transform.position - target.position;
            moveDirection.y = 0;
            moveDirection.Normalize();
            float repositionDuration = Random.Range(0.1f, 0.5f);
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
                moveSpeedMulti2 = 1.2f;
                moveDirection = -moveDirection;
                //Debug.DrawRay(transform.position, 6f * moveDirection, Color.red, 3f);
            }
            if (!isBackBlocked)
            {
                rb.AddForce(moveDirection * 2.2f, ForceMode2D.Impulse);
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

        anim.Play("ShootingAttack");

        // 페이즈별 공격 실행
        if (control.HasCondition(MonsterControl.Condition.Phase3))
        {
            await ExecutePhase3Pattern(target, token);
        }
        else if (control.HasCondition(MonsterControl.Condition.Phase2))
        {
            await ExecutePhase2Pattern(target, token);
        }
        else
        {
            // 기본 페이즈 (Phase 0 / 1)
            ExecutePhase1Pattern(target);
        }

        // 패턴 종료 후 딜레이 및 상태 전환
        await UniTask.Delay((int)(1000f * duration), cancellationToken: token);
        control.ChangeNextState();
    }

    #region Phase Patterns Logic

    // [기본 페이즈] 플레이어 주변 혹은 내 앞 단발 소환
    private void ExecutePhase1Pattern(Transform target)
    {
        Vector3 pos;
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;

        if (Random.value < 0.35f) // 플레이어 뒤쪽
        {
            float rndDist = Random.Range(0.55f, 1.1f);
            pos = target.position + rndDist * -dir;
        }
        else // 내 앞쪽
        {
            float rndDist = control.width + Random.Range(0f, 2f);
            pos = transform.position + rndDist * dir;
        }
        SpawnLightPillar(pos);
    }

    // [페이즈 2] 콤보형 공격 (순차적/예측)
    private async UniTask ExecutePhase2Pattern(Transform target, CancellationToken token)
    {
        float rand = Random.value;

        if (rand < 0.6f) // 패턴 A: 시간차 저격 (플레이어 현재 위치 -> 이동 경로 예측)
        {
            SpawnLightPillar(target.position);
            await UniTask.Delay(500, cancellationToken: token);

            Vector2 velocity = 3f * target.GetChild(0).right;
            Vector3 predictPos = target.position + (Vector3)velocity.normalized * 1.5f;
            SpawnLightPillar(predictPos);
        }
        else // 패턴 C: 보호 후 랜덤 저격
        {
            ExecuteSidePattern();
            await UniTask.Delay(400, cancellationToken: token);
            SpawnLightPillar(GetRandomPosInRange());
        }
    }

    // [페이즈 3] 복합/혼돈형 공격 (가두기/난사/랜덤)
    private async UniTask ExecutePhase3Pattern(Transform target, CancellationToken token)
    {
        int patternType = Random.Range(0, 4);

        switch (patternType)
        {
            case 0: // 광범위3 --> 저격 2연속 + 완전 랜덤 1개
                await ExecuteWideAreaPattern(token, 3);
                await UniTask.Delay(350, cancellationToken: token);
                for (int i = 0; i < 2; i++) { SpawnLightPillar(target.position); await UniTask.Delay(300, cancellationToken: token); }
                SpawnLightPillar(GetRandomPosInRange());
                break;
            case 1: // 광범위 폭격4 --> 플레이어 가두기
                await ExecuteWideAreaPattern(token, 4);
                await UniTask.Delay(200, cancellationToken: token);
                SpawnLightPillar(target.position + Vector3.left * 1.5f);
                SpawnLightPillar(target.position + Vector3.right * 1.5f);
                break;
            case 2: // 내 주변 양옆 + 완전 랜덤 2개 혼사 --> 광범위3
                ExecuteSidePattern();
                await UniTask.Delay(200, cancellationToken: token);
                for (int i = 0; i < 2; i++) { SpawnLightPillar(GetRandomPosInRange()); await UniTask.Delay(200, cancellationToken: token); }
                await UniTask.Delay(350, cancellationToken: token);
                await ExecuteWideAreaPattern(token, 3);
                break;
            case 3: // 광범위 폭격4 --> 혼돈
                await ExecuteWideAreaPattern(token, 4);
                await UniTask.Delay(250, cancellationToken: token);
                Vector3 pos;
                Vector3 dir = (target.position - transform.position).normalized;
                dir.y = 0;
                if (Random.value < 0.35f) // 플레이어 뒤쪽
                {
                    float rndDist = Random.Range(0.55f, 1.1f);
                    pos = target.position + rndDist * -dir;
                }
                else // 내 앞쪽
                {
                    float rndDist = control.width + Random.Range(0f, 2f);
                    pos = transform.position + rndDist * dir;
                }
                SpawnLightPillar(pos);
                for (int i = 0; i < 3; i++) { SpawnLightPillar(GetRandomPosInRange()); await UniTask.Delay(250, cancellationToken: token); }
                break;
        }
    }

    #endregion

    #region Helper Methods

    // 지면을 체크하여 빛기둥 소환
    private void SpawnLightPillar(Vector3 spawnPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(spawnPos.x, transform.position.y + 2f), Vector2.down, 5f, control.groundLayer);
        Vector3 finalPos = hit.collider != null ? (Vector3)hit.point : new Vector3(spawnPos.x, transform.position.y, 0);

        LightPillar clone = Instantiate(lightPillar);
        clone.transform.position = finalPos;
    }

    // 광범위 폭격 로직
    private async UniTask ExecuteWideAreaPattern(CancellationToken token, int count)
    {
        float startX = transform.position.x - range;
        float step = (range * 2f) / (count - 1);
        for (int i = 0; i < count; i++)
        {
            float x = startX + (i * step) + Random.Range(-0.5f, 0.5f);
            SpawnLightPillar(new Vector3(x, transform.position.y, 0));
            await UniTask.Delay(150, cancellationToken: token);
        }
    }

    // 내 주변 양옆 보호
    private void ExecuteSidePattern()
    {
        float sideOffset = 2.5f;
        SpawnLightPillar(transform.position + Vector3.left * sideOffset);
        SpawnLightPillar(transform.position + Vector3.right * sideOffset);
    }

    // 완전 랜덤 좌표 추출
    private Vector3 GetRandomPosInRange()
    {
        float randomX = transform.position.x + Random.Range(-range * 1.5f, range * 1.5f);
        return new Vector3(randomX, transform.position.y, 0);
    }

    #endregion



}
