using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
public class NumParticle : PoolBehaviour
{
    ParticleSystem ps;

    void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        // _maxParticles = ps.main.maxParticles;
        // // Persistent allocator로 NativeArray를 미리 할당
        // _particles = new NativeArray<ParticleSystem.Particle>(_maxParticles, Allocator.Persistent);
        // _random = new Random((uint)System.DateTime.Now.Millisecond + 1);
    }


    #region UniTask Setting
    CancellationTokenSource cts;

    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
    }

    void OnDisable()
    {
        if (ps != null)
        {
            ps.Clear(true);
            ps.Stop(); // Clear 후에 Stop 호출
        }
        UniTaskCancel();
    }

    void OnDestroy()
    {
        // 1. UniTask 취소 루틴만 실행
        UniTaskCancel();

        // 2. Job 완료 및 Native Container 해제는 여기서만 처리합니다. (단일 진입점)
        DisposeJobAndMemory();

    }

    void UniTaskCancel()
    {
        // Job 완료 로직 제거: 오직 취소 토큰 관련 로직만 남깁니다.
        cts?.Cancel();
        try
        {
            cts?.Dispose();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
        cts = null;
        // _jobHandle.Complete() 로직 제거됨
    }

    // 3. Job 완료 및 Native Container 해제를 위한 안전 함수
    void DisposeJobAndMemory()
    {
        // Job이 완료되지 않았다면 강제 완료 처리 (컨테이너에 대한 쓰기 보장)
        // IsCreated 체크는 기본값(default)인 경우에도 안전합니다.
        // if (_jobHandle.IsCompleted == false)
        // {
        //     _jobHandle.Complete();
        // }

        // // Native Container 정리 (Dispose는 한 번만 호출해야 함)
        // if (_particles.IsCreated)
        // {
        //     _particles.Dispose();
        //     _particles = default;
        // }
    }

    #endregion


    // // === Job / Native Container 관련 필드 추가 ===
    // private JobHandle _jobHandle;
    // private NativeArray<ParticleSystem.Particle> _particles;
    // private Random _random; // Job에서 사용하기 위한 Random 인스턴스
    // private int _maxParticles;

    // // UniTask 스케줄러가 켜고, LateUpdate가 확인하는 플래그
    // private bool _needsJobExecution = false;

    // // Grid Snap Movement 설정 (Inspector에서 조절 가능)
    // public float particleSpeed = 0.0042f;
    // public float axisChangeInterval = 0.08f; // 0.08초마다 Job 실행

    

    

    

    public void Play()
    {
        ps.Play();
        Play_ut(cts.Token).Forget();
    }

    async UniTask Play_ut(CancellationToken token)
    {
        await UniTask.Delay(1, ignoreTimeScale: true, cancellationToken: token);
        // UniTask 스케줄러 시작
        await UniTask.Delay((int)(1000f * (0.36f)), ignoreTimeScale: true, cancellationToken: token);
        //StartFixedIntervalUpdate(cts.Token).Forget();
        await UniTask.Delay((int)(1000f * (ps.main.duration + 0.1f - 0.36f)), ignoreTimeScale: true, cancellationToken: token);
        base.Despawn();
    }

    // // === 1. UniTask: Job 실행 플래그 설정 (CPU 부하 최소화) ===
    // async UniTask StartFixedIntervalUpdate(CancellationToken token)
    // {
    //     int intervalMs = (int)(1000f * axisChangeInterval);

    //     while (!token.IsCancellationRequested)
    //     {
    //         // 0.08초마다 (CPU를 사용하지 않고) 대기
    //         await UniTask.Delay(intervalMs, ignoreTimeScale: false, cancellationToken: token);

    //         // 대기 후, 다음 LateUpdate에서 Job을 실행하도록 플래그만 설정
    //         _needsJobExecution = true;
    //     }
    // }

    // === 2. LateUpdate: 플래그 확인 후 Job 실행 및 결과 적용 (안정성 확보) ===
    // void LateUpdate()
    // {
    //     // 0. 플래그가 설정되지 않았다면 (아직 0.08초가 안되었다면) 아무것도 하지 않고 종료
    //     if (!_needsJobExecution) return;

    //     // 1. 이전 Job 완료 확인
    //     if (_jobHandle.IsCompleted == false)
    //     {
    //         _jobHandle.Complete();
    //     }
    //     int particleCount = ps.GetParticles(_particles);
    //     if (particleCount == 0)
    //     {
    //         _needsJobExecution = false;
    //         return;
    //     }

    //     // 2. Job 인스턴스 생성 및 데이터 할당
    //     SciFiEmitterJbo job = new SciFiEmitterJbo
    //     {
    //         particles = _particles,
    //         speed = particleSpeed,
    //         random = _random
    //     };

    //     // 3. Job 스케줄링 및 완료 (메인 스레드에서 즉시 완료 처리하여 SetParticles 준비)
    //     _jobHandle = job.Schedule(particleCount, 64);
    //     _jobHandle.Complete();

    //     // 4. 파티클 시스템에 결과 적용
    //     ps.SetParticles(_particles, particleCount);

    //     // Job 내부에서 변경된 Random 상태를 다시 저장
    //     _random = job.random;

    //     // 5. Job 실행을 완료했으므로 플래그를 해제 (다음 0.08초를 기다림)
    //     _needsJobExecution = false;
    // }
}
// [BurstCompile]
// public struct SciFiEmitterJbo : IJobParallelFor
// {
//     public NativeArray<ParticleSystem.Particle> particles;

//     // 파티클의 이동 속력 (Job 실행 간 일정하게 유지)
//     [ReadOnly] public float speed;

//     // Job 내부에서 랜덤 선택에 사용
//     public Unity.Mathematics.Random random;

//     public void Execute(int index)
//     {
//         ParticleSystem.Particle p = particles[index];

//         // 1. 현재 속도 벡터를 float3로 변환
//         float3 currentVelocity = p.velocity;
//         float magnitude = math.length(currentVelocity);

//         float3 currentDirection;
//         float angle;

//         // 2. 방향 벡터 결정: 속도가 0이 아닐 경우와 0일 경우 분기
//         if (magnitude > 0.0001f)
//         {
//             // 현재 속도 방향 정규화
//             currentDirection = math.normalize(currentVelocity);
//         }
//         else
//         {
//             // 속도가 0일 경우 (멈춰있을 경우): 랜덤 방향으로 시작
//             // 2D 횡스크롤을 가정하여 Z=0인 랜덤 벡터 생성
//             float2 randomDir2D = random.NextFloat2Direction();
//             currentDirection = new float3(randomDir2D.x, randomDir2D.y, 0f);
//         }

//         // 3. 꺾임 각도 랜덤 선택 (0도, +30도, -30도)
//         int choice = random.NextInt(0, 10);
//         float multiplier = 1f;

//         switch (choice)
//         {
//             case 0: // 우회전 (+30도)
//                 angle = 30f;
//                 multiplier = 2f;
//                 break;
//             case 1: // 좌회전 (-30도)
//                 angle = -30f;
//                 multiplier = 2f;
//                 break;
//             default:
//                 angle = 0f;
//                 break;
//         }

//         // 4. Z축을 기준으로 회전하는 쿼터니언 생성
//         // math.radians()를 사용하여 Degree를 Radian으로 변환
//         quaternion rotation = quaternion.RotateZ(math.radians(angle));

//         // 5. 회전을 현재 방향에 적용
//         // math.mul(Rotation, Vector)를 사용하여 새로운 방향 벡터 계산
//         float3 newDirection = math.mul(rotation, currentDirection);

//         // 6. 속력을 곱하여 새로운 속도 벡터 완성 (일정한 speed 유지)
//         float3 newVelocity = newDirection * multiplier * speed;

//         // 7. 결과 저장
//         p.velocity = newVelocity;
//         particles[index] = p;
//     }
// }