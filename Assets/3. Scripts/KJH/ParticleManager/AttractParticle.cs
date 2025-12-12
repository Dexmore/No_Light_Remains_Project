using System.Threading;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
public class AttractParticle : MonoBehaviour
{
    #region UniTask Setting
    private CancellationTokenSource cts;
    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
        _time = Time.time;
        _maxCount = _ps.main.maxParticles; 
        _particles = new ParticleSystem.Particle[_maxCount]; 
        _naPatPositions = new NativeArray<float3>(_maxCount, Allocator.Persistent);
        _naPatVelocities = new NativeArray<float3>(_maxCount, Allocator.Persistent);
    }
    void OnDisable()
    {
        if (_ps != null)
        {
            _ps.Clear(true);
            _ps.Stop(); // Clear 후에 Stop 호출
        }
        UniTaskCancel();
        DisposeJobAndMemory();
    }
    void OnDestroy()
    {
        UniTaskCancel();
    }
    void UniTaskCancel()
    {
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
    }
    #endregion
    
    public Vector3 targetVector;
    public Transform targetTransform;
    [Range(0.01f, 1f)]
    public float attractionStrength = 0.3f;
    private ParticleSystem _ps;
    private ParticleSystem.Particle[] _particles; // 파티클 데이터 배열 (CPU)
    private int _maxCount;
    // Job에 필요한 NativeArray
    private NativeArray<float3> _naPatPositions; // 파티클 위치
    private NativeArray<float3> _naPatVelocities; // 파티클 속도 (수정 대상)
    private JobHandle _jobHandle;
    private void Awake()
    {
        _ps = GetComponentInChildren<ParticleSystem>();
    }
    // Job 실행 전에 이전 Job이 완료되었는지 확인하고 NativeArray를 해제합니다.
    private void DisposeJobAndMemory()
    {
        if (_jobHandle.IsCompleted == false)
        {
            _jobHandle.Complete();
        }
        
        if (_naPatPositions.IsCreated)
        {
            _naPatPositions.Dispose();
        }
        if (_naPatVelocities.IsCreated)
        {
            _naPatVelocities.Dispose();
        }
    }
    float _interval = 0.08f;
    float _time;
    private void LateUpdate()
    {
        if(Time.time - _time < _interval) return;
        _time = Time.time;
        if (targetVector == Vector3.zero && targetTransform == null) return;
        int liveParticleCount = _ps.GetParticles(_particles);
        if (liveParticleCount == 0) return;
        // NativeArray에 데이터 복사 (CPU -> NativeArray)
        for (int i = 0; i < liveParticleCount; i++)
        {
            _naPatPositions[i] = (float3)_particles[i].position;
            _naPatVelocities[i] = (float3)_particles[i].velocity;
        }
        // Job 데이터 설정
        float3 target = targetTransform != null ? (float3)targetTransform.position : (float3)targetVector;
        var job = new AttractParticleJob
        {
            _naPatPositions = this._naPatPositions,
            _naPatVelocities = this._naPatVelocities,
            _target = target,
            _attractionStrength = this.attractionStrength,
            _maxSpeed = _ps.main.startSpeedMultiplier + 5f,
            _minSpeed = 5f,
            _maxAttractDistance = 10f
        };
        _jobHandle = job.Schedule(liveParticleCount, 32);
        // LateUpdate가 끝난 후 렌더링 전에 Job이 완료되도록 강제 대기합니다.
        _jobHandle.Complete(); 
        // NativeArray에 저장된 수정된 속도 값을 다시 CPU 배열에 복사합니다.
        for (int i = 0; i < liveParticleCount; i++)
        {
            _particles[i].velocity = (Vector3)_naPatVelocities[i];
        }
        _ps.SetParticles(_particles, liveParticleCount);
    }
}
[BurstCompile]
public struct AttractParticleJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> _naPatPositions; 
    public NativeArray<float3> _naPatVelocities; 
    
    [ReadOnly] public float3 _target;
    [ReadOnly] public float _attractionStrength; // 방향을 꺾는 힘 (기존 로직 유지)
    [ReadOnly] public float _maxSpeed;           
    [ReadOnly] public float _minSpeed;
    [ReadOnly] public float _maxAttractDistance; // 이 거리 밖에서는 maxSpeed를 유지합니다.
    public void Execute(int index)
    {
        float3 currentVelocity = _naPatVelocities[index];
        float3 currentPosition = _naPatPositions[index];
        float3 targetDirection = math.normalize(_target - currentPosition);
        float3 currentVelocityDirection = math.normalize(currentVelocity);
        float3 newDirection = math.normalize(
            (currentVelocityDirection * (1f - _attractionStrength)) + 
            (targetDirection * _attractionStrength)
        );
        float distance = math.distance(_target, currentPosition);
        // 거리 비율 계산: 0 (타겟에 가까움) ~ 1 (최대 거리 이상)
        // math.saturate는 0 미만은 0, 1 초과는 1로 클램프합니다.
        float distanceFactor = math.saturate(distance / _maxAttractDistance);
        float targetSpeed = math.lerp(_minSpeed, _maxSpeed, distanceFactor);
        _naPatVelocities[index] = newDirection * targetSpeed;
    }
}