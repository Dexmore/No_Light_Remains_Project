using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
public class MonsterReposition : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Reposition;
    MonsterPursuit monsterPursuit;
    protected override void Awake()
    {
        base.Awake();
        TryGetComponent(out monsterPursuit);
    }
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        Activate(token).Forget();
    }
    List<Vector2[]> paths = new List<Vector2[]>();
    RaycastHit2D[] grounds = new RaycastHit2D[10];
    public async UniTask Activate(CancellationToken token)
    {
        if (control.memories.Count == 0)
        {
            await UniTask.Yield(cts.Token);
            control.ChangeState(MonsterControl.State.Idle);
            return;
        }
        Transform target;
        float startTime;
        startTime = Time.time;
        target = control.memories.First().Key.transform;

        // 1. 타겟을 넘어서 지나갈지 or 타겟과 거리를 벌릴지 판단.
        float a = monsterPursuit.stopDistance;
        float result = 1f;
        // a가 1f보다 작은 몬스터는. 몸톰 박치기 위주 몬스터이므로. 지나가는 확률을 매우 키워야하고.. (아래에서 result가 0에 가까워야 다가가는 방향임)
        // a가 5f보다 큰 몬스터는. 원거리 공격형 위주 몬스터이므로. 거리를 벌리는 확률을 키워야함. (아래에서 result가 1에 가까워야 멀어지는 방향임)
        result = 1f - Mathf.Exp(-0.8f * a); //위의 로직은 이 곡선이 적당함
        bool isTowardPlayer = true;
        Vector2 directionX = target.position - transform.position; // 다가가는 방향
        directionX.y = 0f;
        directionX.Normalize();
        float rnd = Random.value;
        if (rnd <= result)
        {
            isTowardPlayer = false;
            directionX = transform.position - target.position; // 멀어지는 방향
            directionX.y = 0f;
            directionX.Normalize();
        }
        // 2. 방향이 정해졌으니, 그 방향으로 어느정도 시간동안 이동할지 판단
        float b = (target.position - transform.position).magnitude;
        float _duration = Random.Range(0.3f, 0.8f);
        // 몸통 박치기형 몬스터의 경우 더욱 충분한 시간동안 이동해서 플레이어를 지나쳐 넘어가야함
        if (a < 1f)
        {
            _duration += (b / control.data.MoveSpeed) + Random.Range(0.5f, 1.2f);
        }
        // 멀어지려하는 몬스터의 경우. stopDistance보다 멀어지는건 방지해야함.
        else if (rnd <= result)
        {
            if (b > 0.9f * a - 1f)
            {
                if (Random.value <= 0.75f)
                {
                    await UniTask.Yield(cts.Token);
                    control.ChangeNextState();
                    return;
                }
                directionX = transform.position - target.position; // 다가가는 방향
                directionX.y = 0f;
                directionX.Normalize();
            }
        }
        // 3. Astar를 통해 8개의 목적지 후보중 총경로 길이가 가장 짧은 것 고르기
        paths.Clear();
        for (int i = 0; i < 8; i++) paths.Add(null);
        Vector2 findPos = Vector2.zero;
        Vector2[] findPath = null;
        float minTotalLength = float.MaxValue;
        for (int i = 0; i < paths.Count; i++)
        {
            // Astar로 지정할 임시목적지 선정
            Vector2 pos = ((Vector2)target.position) + control.data.MoveSpeed * _duration * directionX * Random.Range(1.5f, 2.5f);
            int count = Physics2D.RaycastNonAlloc(pos + 5f * Vector2.up, Vector2.down, grounds, 10f, control.groundLayer);
            //Debug.DrawRay(pos + 5f * Vector2.up, 5f * Vector2.down, Color.blue, 2f);
            if (count == 0) continue;
            int rnd2 = Random.Range(0, count);
            for (int k = 0; k < 20; k++)
            {
                if (rnd2 < grounds.Length)
                {
                    break;
                }
                else
                {
                    rnd2 = Random.Range(0, count);
                }
            }
            if (rnd2 >= grounds.Length) continue;
            Vector2 pos2 = grounds[rnd2].point + 0.02f * Vector2.up;
            paths[i] = await astar.Find(pos2);
            if (paths[i].Length <= 1) continue;
            float leng = 0f;
            for (int k = 1; k < paths[i].Length; k++)
            {
                leng += (paths[i][k] - paths[i][k - 1]).magnitude;
            }
            if (leng == 0) continue;
            if (leng < minTotalLength)
            {
                minTotalLength = leng;
                findPath = paths[i];
                findPos = pos2;
            }
        }
        if (findPath == null)
        {
            await UniTask.Yield(cts.Token);
            control.ChangeNextState();
            return;
        }

        // 경로를 따라 이동
        bool isAnimation = false;
        startTime = Time.time;
        for (int i = 1; i < findPath.Length; i++)
        {
            Vector2 segmentPos = findPath[i];
            Vector2 displacement = segmentPos - ((Vector2)transform.position + astar.offeset * Vector2.up);
            float distance = displacement.magnitude;
            Vector2 moveHorizontal = displacement;
            moveHorizontal.y = 0f;
            moveHorizontal.Normalize();
            float expectTime = 1.8f * (displacement.magnitude / control.data.MoveSpeed);
            float startTime2 = Time.time;
            if (moveHorizontal.x > 0 && model.right.x < 0)
                model.localRotation = Quaternion.Euler(0f, 0f, 0f);
            else if (moveHorizontal.x < 0 && model.right.x > 0)
                model.localRotation = Quaternion.Euler(0f, 180f, 0f);
            while (distance > 0.05f && Time.time - startTime2 < expectTime && Time.time - startTime < _duration)
            {
                moveHorizontal = segmentPos - ((Vector2)transform.position + astar.offeset * Vector2.up);
                distance = moveHorizontal.magnitude;
                if (Mathf.Abs(moveHorizontal.x) <= 0.002f)
                {
                    await UniTask.Yield(cts.Token);
                    control.ChangeState(MonsterControl.State.Idle);
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
                        if(!isTowardPlayer || (isTowardPlayer && !control.isStagger))
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
            }
        }
        while (Time.time - startTime < _duration)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            Vector2 moveDirection = model.right;
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
                    if(!isTowardPlayer || (isTowardPlayer && !control.isStagger))
                        rb.AddForce(multiplier * moveDirection * 2.9f * (control.data.MoveSpeed + 4.905f) / 1.25f);
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
        }
        await UniTask.Yield(cts.Token);
        await UniTask.Delay((int)(100f), cancellationToken: token);
        control.ChangeNextState();
    }

}
