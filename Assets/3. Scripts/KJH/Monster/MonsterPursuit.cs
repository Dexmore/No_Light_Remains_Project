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
        if (Random.value < 0.8f)
            Activate2(token).Forget();
        else
            Activate(token).Forget();
    }
    Transform target;
    bool isMoveAnimation;
    public void Retry()
    {
        Activate2(cts.Token).Forget();
    }
    Ray2D checkCliffRay;
    RaycastHit2D CheckCliffHit;
    int farCount = 0;
    // 단순 좌우 이동형 추적
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
            control.ChangeNextState();
            return;
        }
        target = control.memories.First().Key.transform;

        checkRay = new Ray2D();
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            anim.Play("Idle");
        float startTime = Time.time;
        Vector2 moveDirection = (target.position - transform.position);
        moveDirection.y = 0f;
        moveDirection.Normalize();

        float duration = Random.Range(0.2f, 2f);
        // 캐릭터 좌우 방향 설정
        if (moveDirection.x > 0 && model.right.x < 0)
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (moveDirection.x < 0 && model.right.x > 0)
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        int tempCount = 0;
        float returnTime = Time.time;
        while (Time.time - startTime < duration && !token.IsCancellationRequested)
        {

            if (Time.time - returnTime > 1.8f)
            {
                float homeRadius = control.findRadius * 2.4f;
                float homeDistance = Vector2.Distance(control.startPosition, transform.position);
                float ratio = homeDistance / homeRadius;
                ratio = Mathf.Clamp(ratio - 0.1f, 0f, 1f);
                float returnChance = Mathf.Pow(ratio, 3);
                //Debug.Log($"homeDistance:{homeDistance} , W1 returnChanve:{returnChance}");
                if (Random.value < control.homeValue)
                    if (Random.value < returnChance * 0.5f)
                    {
                        await UniTask.Delay(5, cancellationToken: token);
                        control.ChangeState(MonsterControl.State.ReturnHome, true);
                        return;
                    }
                returnTime = Time.time;
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
            Vector2 rayOrigin = transform.position + 1.3f * control.width * model.right + 0.2f * control.height * Vector3.up;
            Vector2 rayDirection = Vector3.down;
            float rayLength = 0.9f * control.jumpLength + 0.1f * control.height;
            checkRay.origin = rayOrigin;
            checkRay.direction = rayDirection;
            //Debug.DrawRay(checkRay.origin, rayLength * checkRay.direction, Color.green, 1f);
            CheckRayHit = Physics2D.Raycast(checkRay.origin, checkRay.direction, rayLength, control.groundLayer);
            if (CheckRayHit.collider == null)
            {
                stopWall = true;
            }
            if (moveDirection.y > 0.1f && !control.isGround)
            {
                stopWall = true;
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
                    if (Random.value < 5f * Time.deltaTime)
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
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
        control.ChangeNextState();
    }
    public async UniTask Activate2(CancellationToken token)
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
            control.ChangeNextState();
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
        Vector2[] findPath = await astar.Find((Vector2)target.position + addPos, token);
        if (findPath.Length <= 1)
        {

            // 반복될수록 추적 포기할 확률 높게 만들기
            float returnChance = 1.0f / (1.0f + Mathf.Exp(-0.5f * (farCount - 7f)));

            if (Random.value < returnChance && Random.value < control.homeValue)
            {
                farCount = 0;
                await UniTask.Delay(5, cancellationToken: token);
                control.ChangeState(MonsterControl.State.ReturnHome, true);
                return;
            }
            else
            {
                farCount++;
                await UniTask.Yield(token);
                control.ChangeNextState();
            }
            return;
        }

        // 경로를 따라 이동
        for (int i = 1; i < findPath.Length; i++)
        {
            Vector2 targetPos = findPath[i];
            Vector2 startPos = (Vector2)transform.position + astar.offeset * Vector2.up;
            Vector2 moveDirection = targetPos - ((Vector2)transform.position + astar.offeset * Vector2.up);
            float length = moveDirection.magnitude;
            float xLength = Mathf.Abs(moveDirection.x);
            float yLength = Mathf.Abs(moveDirection.y);

            if (i % 3 == 0)
            {
                float homeRadius = control.findRadius * 2.4f;
                float homeDistance = Vector2.Distance(control.startPosition, transform.position);
                float ratio = homeDistance / homeRadius;
                ratio = Mathf.Clamp(ratio - 0.1f, 0f, 1f);
                float returnChance = Mathf.Pow(ratio, 3);
                //Debug.Log($"homeDistance:{homeDistance} , W1 returnChanve:{returnChance}");
                if (Random.value < control.homeValue)
                    if (Random.value < returnChance * 0.5f)
                    {
                        await UniTask.Delay(5, cancellationToken: token);
                        control.ChangeState(MonsterControl.State.ReturnHome, true);
                        return;
                    }
            }

            // 3. Astar상 땅 수직 아래로 파고들거나 하늘로 치솟는 경로가 나오는 경우
            if (xLength <= 0.5f * astar.unit && yLength > 2.5f * astar.unit || moveDirection.y > 0.1f && !control.isGround)
            {
                await UniTask.Delay(5, cancellationToken: token);
                control.ChangeNextState();
                return;
            }

            moveDirection.y *= 0.5f;
            moveDirection.Normalize();

            // 캐릭터 좌우 방향 설정
            if (moveDirection.x > 0 && model.right.x < 0)
                model.localRotation = Quaternion.Euler(0f, 0f, 0f);
            else if (moveDirection.x < 0 && model.right.x > 0)
                model.localRotation = Quaternion.Euler(0f, 180f, 0f);

            // 전방의 벽 체크용 레이캐스트
            Vector2 rayOrigin = transform.position + 0.1f * control.width * model.right + 0.1f * control.height * Vector3.up;
            Vector2 rayDirection = 0.15f * (Vector2)model.right + 0.85f * moveDirection.normalized;
            float rayLength = 0.75f * control.width + 0.1f * control.height + 0.65f * astar.unit + 0.2f;
            checkRay.origin = rayOrigin;
            checkRay.direction = rayDirection;
            CheckRayHit = Physics2D.Raycast(checkRay.origin, checkRay.direction, rayLength, control.groundLayer);
            float angle = 999;
            //Debug.DrawRay(checkRay.origin, rayLength * checkRay.direction, Color.white, 2f);
            if (CheckRayHit.collider != null)
            {
                Vector2 normal = CheckRayHit.normal;
                angle = Vector2.Angle(moveDirection, -normal);
            }

            // 여러 분기
            // 1. 전방에 급한 벽이 있는 경우
            if (angle < 45)
            {
                //Debug.DrawLine(startPos, targetPos, Color.red, 3f);
                moveDirection.y = 0f;
                moveDirection.Normalize();
                float ratio = yLength / control.jumpLength;
                ratio = 0.8f * Mathf.Clamp(ratio, 0.3f, 1f) + 0.2f;
                rb.AddForce((0.97f * Vector3.up + 0.03f * model.right).normalized * (control.jumpLength + 1.8f) * 260f * ratio);
                await UniTask.Delay(200, cancellationToken: token);
                float _time = Time.time;
                while (Time.time - _time < 0.25f && !token.IsCancellationRequested)
                {
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                    rb.AddForce(moveDirection * 2.5f * (control.data.MoveSpeed + 4.905f) / 1.25f);
                }
                await UniTask.Delay(500, cancellationToken: token);
                control.ChangeNextState();
                return;
            }

            // 2. 전방에 수평적 점프 포인트
            if (findPath.Length >= 3 && i < findPath.Length - 1)
            {
                Vector2 nextTarget = findPath[i + 1];
                float _length = Mathf.Abs(nextTarget.x - startPos.x);
                if (_length > 3f * astar.unit && _length < 1.2f * control.jumpLength && IsHorizontalJumpGround(startPos, nextTarget))
                {
                    //Debug.DrawLine(startPos, nextTarget, Color.green, 3f);
                    Vector2 jumpDirection = (nextTarget - startPos).normalized;
                    if (jumpDirection.y <= 0.3f * control.height) jumpDirection = model.right + Vector3.up;
                    jumpDirection = 0.4f * jumpDirection + 0.6f * Vector2.up;
                    rb.AddForce(jumpDirection.normalized * (control.jumpLength + 1.8f) * 220f);
                    await UniTask.Delay(200, cancellationToken: token);
                    float _time = Time.time;
                    while (Time.time - _time < 2f && !token.IsCancellationRequested)
                    {
                        if (Time.time - _time < 0.3f)
                        {
                            rb.AddForce(model.right * 2.5f * (control.data.MoveSpeed + 4.905f) / 1.25f);
                        }
                        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                        if (control.isGround) break;
                    }
                    await UniTask.Delay(500, cancellationToken: token);
                    control.ChangeNextState();
                    return;

                }
            }


            // !! 가까움 상태일시 탈출 !!
            if (control.HasCondition(MonsterControl.Condition.ClosePlayer))
            {
                if (Random.value < 0.4f)
                {
                    await UniTask.Delay(5, cancellationToken: token);
                    control.ChangeNextState();
                    return;
                }
            }

            // 4. 일반적인 고르게 이어진 길
            float startTime = Time.time;
            float expectTime = (length / control.MoveSpeed) * 1.5f;
            while (Time.time - startTime < expectTime && !token.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                moveDirection = targetPos - ((Vector2)transform.position + astar.offeset * Vector2.up);
                moveDirection.y *= 0.4f;
                moveDirection.y = Mathf.Clamp(moveDirection.y, 0f, control.height);
                if (moveDirection.sqrMagnitude < 0.08f * 0.08f) break;

                // !! stopDistance 보다 가까우면 탈출 !!
                float sqrMagnitudeFinalTarget = ((Vector2)target.position - ((Vector2)transform.position + astar.offeset * Vector2.up)).sqrMagnitude;
                if (sqrMagnitudeFinalTarget < stopDistance * stopDistance)
                {
                    await UniTask.Delay(5, cancellationToken: token);
                    control.ChangeNextState();
                    return;
                }

                // if (!control.isGround)
                // {
                //     await UniTask.Yield(token);
                //     control.ChangeNextState();
                //     return;
                // }

                // 낭떠러지 체크
                rayOrigin = transform.position + 1.3f * control.width * model.right + 0.2f * control.height * Vector3.up;
                rayDirection = Vector3.down;
                rayLength = 0.9f * control.jumpLength + 0.1f * control.height;
                //Debug.DrawRay(rayOrigin, rayLength * rayDirection, Color.white, 3f * Time.fixedDeltaTime);
                checkRay.origin = rayOrigin;
                checkRay.direction = rayDirection;
                CheckRayHit = Physics2D.Raycast(checkRay.origin, checkRay.direction, rayLength, control.groundLayer);
                if (CheckRayHit.collider == null)
                {
                    await UniTask.Yield(token);
                    if (Random.value < 0.999f)
                    {
                        control.ChangeNextState();
                    }
                    else if (!control.IsCoolTime(MonsterControl.State.Wander))
                    {
                        control.ChangeState(MonsterControl.State.Wander);
                    }
                    return;
                }
                //3. 하늘로 치솟는 버그 방지
                if (xLength <= 0.5f * astar.unit && yLength > 2.5f * astar.unit || moveDirection.y > 0.1f && !control.isGround)
                {
                    await UniTask.Delay(5, cancellationToken: token);
                    if (Random.value < 0.999f)
                    {
                        control.ChangeNextState();
                    }
                    else if (!control.IsCoolTime(MonsterControl.State.Wander))
                    {
                        control.ChangeState(MonsterControl.State.Wander);
                    }
                    return;
                }



                moveDirection.Normalize();
                float dot = Vector2.Dot(rb.linearVelocity, moveDirection);
                // 이동
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
            }
        }

        // 이동 끝
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
    Ray2D checkRay;
    RaycastHit2D CheckRayHit;
    bool IsHorizontalJumpGround(Vector2 startPos, Vector2 targetPos)
    {
        float[] checkT = { 0.35f, 0.45f, 0.55f, 0.75f, 0.85f };
        float rayHeightOffset = control.height * 1.5f;
        float rayLength = control.height * 4.5f + 2.5f;
        int count = 0;
        foreach (float t in checkT)
        {
            float checkX = Mathf.Lerp(startPos.x, targetPos.x, t);
            // Raycast 원점: 시작점 Y좌표 + 안전 높이
            Vector2 rayOrigin = new Vector2(checkX, startPos.y + rayHeightOffset);
            // 수직 아래로 레이캐스트 실행
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, control.groundLayer);
            //Debug.DrawRay(rayOrigin, rayLength * Vector2.down, Color.white, 2f);
            // 만약 레이가 땅에 닿지 않는다면 (hit.collider == null), 
            if (hit.collider == null) count++;
        }
        if (count <= 3) return false;

        // BoxCast 크기: 몬스터의 너비와 높이의 절반을 사용하여 충돌 영역 설정
        // BoxCast는 중심점(center) 기준이므로, size는 몬스터 크기의 90% 정도를 사용합니다.
        Vector2 boxSize = new Vector2(control.width * 0.9f, control.height * 0.5f);
        // 궤적을 검사할 수평 지점 (25%, 50%, 75% 지점)
        float[] checkXPositions = { 0.25f, 0.5f, 0.75f };
        // 몬스터의 최고 잠재적 점프 높이 (최악의 경우를 가정하여 목표 y에 몬스터 높이를 더함)
        float maxPossibleY = Mathf.Max(startPos.y, targetPos.y) + control.height * 1.5f;
        foreach (float t in checkXPositions)
        {
            // 검사 지점 X 좌표 (선형 보간)
            float checkX = startPos.x + (targetPos.x - startPos.x) * t;
            // 검사 지점 Y 좌표 (선형 보간)
            float checkY = Mathf.Lerp(startPos.y, targetPos.y, t);
            // BoxCast 원점 (레이저를 쏘기 시작하는 지점)
            // 몬스터의 바닥 Y 위치에 몬스터 높이의 절반을 더하여 몬스터의 중심 위치(Collider Center)를 원점으로 사용합니다.
            Vector2 rayOrigin = new Vector2(checkX, checkY + control.height * 0.5f);
            // BoxCast 길이: 현재 높이에서 최고 예상 높이까지
            rayLength = maxPossibleY - rayOrigin.y;
            // 장애물을 찾을 방향 (수직 위)
            Vector2 rayDirection = Vector2.up;
            // 실제 BoxCast 실행 (천장 장애물 찾기)
            RaycastHit2D hit = Physics2D.BoxCast
            (
                rayOrigin,
                boxSize,
                0f, // 각도
                rayDirection,
                rayLength,
                control.groundLayer
            );
            if (hit.collider != null)
            {
                return false; // 장애물 발견: 경로 막힘
            }
        }
        return true; // 모든 검사 통과: 경로 깨끗함
    }





}
