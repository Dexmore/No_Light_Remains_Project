using UnityEngine;
//using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using NaughtyAttributes;
using Random = UnityEngine.Random;
public class LightAppearPlatform : Lanternable, ISavable
{
    #region Lanternable Complement
    public override bool isReady { get { return _isReady; } set { _isReady = value; } }
    public override bool isAuto => false;
    public override ParticleSystem particle => ps;
    public override SpriteRenderer lightPoint => lp;
    #endregion
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete;
    bool ISavable.CanReplay => false;
    int ISavable.ReplayWaitTimeSecond => 0;
    public void SetCompletedImmediately()
    {
        _isReady = false;
        isComplete = true;
        Step4();
    }
    #endregion
    bool _isReady;
    ParticleSystem ps;
    SpriteRenderer lp;
    [SerializeField] GameObject platform;
    Collider2D platformColl;
    void Awake()
    {
        _isReady = true;
        isComplete = false;
        platform?.SetActive(false);
        platform.TryGetComponent(out platformColl);
        ps = transform.GetComponentInChildren<ParticleSystem>(true);
        ps?.gameObject.SetActive(false);
        lp = transform.Find("LightPoint").GetComponent<SpriteRenderer>();
        lp?.gameObject.SetActive(false);
        light2 = transform.Find("Light(2)").GetComponent<Light2D>();
        light2?.gameObject.SetActive(false);
        childLight = lp.GetComponentInChildren<Light2D>();
    }
    public override void Run()
    {
        _isReady = false;
        isComplete = true;
        Step2();
        AudioManager.I.PlaySFX("UIClick2");
        AudioManager.I.PlaySFX("LightApper", platform.transform.position, spatialBlend: 0.35f);
    }
    public override void PromptFill()
    {
        lp?.gameObject.SetActive(true);
        DOTween.Kill(lp);
        tweenChildLight?.Kill();
        lp.color = new Color(lp.color.r, lp.color.g, lp.color.b, 0f);
        childLight.intensity = 0f;
        lp.DOFade(1f, 0.5f).SetEase(Ease.InSine);
        tweenChildLight = DOTween.To(() => childLight.intensity, x => childLight.intensity = x, 0.5f, 0.5f).SetEase(Ease.InSine).Play();
    }
    public override void PromptCancel()
    {
        DOTween.Kill(lp);
        tweenChildLight?.Kill();
        lp.DOFade(0f, 2.2f).SetEase(Ease.InSine);
        tweenChildLight = DOTween.To(() => childLight.intensity, x => childLight.intensity = x, 0f, 2.2f).SetEase(Ease.InSine)
        .OnComplete(() => lp.gameObject.SetActive(false)).Play();
    }

    // 단계적 작업.

    // PlayerInteraction.cs에서 전부 처리 할일) 
    // 1. PlayerLight/Lantern 스프라이트가. 플레이어-상호작용오브젝트 사이 적당한 위치에 서서히 붕 뜨면서.
    // 2. PlayerLight/Lantern 스프라이트의 투명도와 PlayerLight/Lantern/Intensity 가 0 에서 적절한값으로 서서히 올라가면서
    // 3. 라인 랜더러를 서서히 목표물에 닿게하고
    // 4. PlayerLight/Lantern/전용 Freeform Light를 특정 물체에 빛을 모으고 집중하는 형태로 만들기
    // ------------
    // PlayerInteraction.cs에서 실행) 단. 메소드는 여기에서 제공
    // 5. 상호작용오브젝트(Lanternable) 에서 LightPoint를 서서히 켜주면서
    
    Light2D childLight;
    Tween tweenChildLight;
    Light2D light2;
    Tween tweenLight2;
    private ParticleSystem.Particle[] particles;
    private Vector3[] targetPositions;
    private bool isParticleTracking = false;
    public async void Step2()
    {
        DOTween.Kill(lp);
        tweenChildLight?.Kill();
        lp.DOFade(1f, 0.5f).SetEase(Ease.InSine);
        tweenChildLight = DOTween.To(() => childLight.intensity, x => childLight.intensity = x, 0.5f, 0.5f).SetEase(Ease.InSine).Play();
        light2?.gameObject.SetActive(true);
        light2.intensity = 0f;
        tweenLight2?.Kill();
        tweenLight2 = DOTween.To(() => light2.intensity, x => light2.intensity = x, 1.8f, 4f);
        ps?.gameObject.SetActive(true);
        ps.Play();
        int maxCount = ps.main.maxParticles;
        particles = new ParticleSystem.Particle[maxCount];
        Vector3[] targetPositions = new Vector3[maxCount];
        Vector3[] initialVels = new Vector3[maxCount];
        Random.InitState(platform.name.GetHashCode() + System.DateTime.Now.Month);
        for (int i = 0; i < maxCount; i++)
        {
            int rndLine = Random.Range(0, lineRenderers.Length);
            float bias = (float)i / maxCount;
            float t = Mathf.Pow(Random.value, 1.5f + (1f - bias));
            Vector3 endOffset = (rndLine <= 2) ? new Vector3(lineXLength, 0, 0) : new Vector3(0, -lineYLength, 0);
            targetPositions[i] = lineRenderers[rndLine].transform.TransformPoint(Vector3.Lerp(Vector3.zero, endOffset, t));
            initialVels[i] = Quaternion.Euler(0, 0, Random.Range(0, 360)) * Vector3.up * Random.Range(0.8f, 2.0f);
        }
        float _startTime = Time.time;
        bool step3Flag = false;
        while (ps != null && (ps.isPlaying || ps.particleCount > 0))
        {
            if (!step3Flag && Time.time - _startTime > 1.8f)
            {
                step3Flag = true;
                Step3();
                lp?.gameObject.SetActive(false);
            }
            await Task.Yield();
            if (this.destroyCancellationToken.IsCancellationRequested) break;
            int aliveCount = ps.GetParticles(particles);
            if (aliveCount == 0) continue;
            NativeArray<Vector3> posArr = new NativeArray<Vector3>(aliveCount, Allocator.TempJob);
            NativeArray<Vector3> velArr = new NativeArray<Vector3>(aliveCount, Allocator.TempJob);
            NativeArray<Vector3> tarArr = new NativeArray<Vector3>(aliveCount, Allocator.TempJob);
            for (int i = 0; i < aliveCount; i++)
            {
                posArr[i] = particles[i].position;
                velArr[i] = (particles[i].velocity.sqrMagnitude < 0.1f) ? initialVels[i] : particles[i].velocity;
                tarArr[i] = targetPositions[i];
            }
            ParticleFireflyJob job = new ParticleFireflyJob
            {
                Positions = posArr,
                Velocities = velArr,
                Targets = tarArr,
                DeltaTime = Time.deltaTime,
                Speed = 2.6f,
                SteeringForce = 2f,
                NoiseStrength = 0.01f,
                TimeInput = Time.time
            };
            JobHandle handle = job.Schedule(aliveCount, 64);
            handle.Complete();
            for (int i = 0; i < aliveCount; i++)
            {
                particles[i].position = posArr[i];
                particles[i].velocity = velArr[i];
                float dist = Vector3.Distance(posArr[i], tarArr[i]);
                if (dist < 0.25f)
                {
                    particles[i].remainingLifetime = Mathf.Lerp(particles[i].remainingLifetime, 0f, Time.deltaTime * 6f);
                }
            }
            ps.SetParticles(particles, aliveCount);
            // 메모리 해제
            posArr.Dispose();
            velArr.Dispose();
            tarArr.Dispose();
        }
    }
    float lineXLength;
    float lineYLength;
    void Start()
    {
        lineRenderers = new LineRenderer[platform.transform.childCount];
        light2Ds = new Light2D[platform.transform.childCount];
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            platform.transform.GetChild(i).TryGetComponent(out lineRenderers[i]);
            platform.transform.GetChild(i).GetChild(0).TryGetComponent(out light2Ds[i]);
            // --- Light2D 노드 강제 초기화 ---
            if (light2Ds[i] != null)
            {
                float s = 0.05f; // 초기 두께
                Vector3[] initialPath = new Vector3[4]
                {
                    new Vector3(-s, s, 0),  // 0: 좌상
                    new Vector3(-s, -s, 0), // 1: 좌하
                    new Vector3(s, -s, 0),  // 2: 우하
                    new Vector3(s, s, 0)    // 3: 우상
                };
                light2Ds[i].SetShapePath(initialPath);
            }
        }
        lineXLength = Mathf.Abs(lineRenderers[3].transform.position.x - lineRenderers[6].transform.position.x);
        lineXLength -= 0.08f * lineXLength;
        lineYLength = Mathf.Abs(lineRenderers[0].transform.position.y - lineRenderers[2].transform.position.y);
        lineYLength -= 0.065f * lineYLength;
    }
    // 0,1,2가로라인(위에서아래순)-->3,4,5,6세로라인(왼에서오른순)
    LineRenderer[] lineRenderers;
    Light2D[] light2Ds;
    public async void Step3()
    {
        if (platform == null) return;
        platform.SetActive(true);
        platformColl.enabled = false;
        foreach (var lr in lineRenderers) lr.SetPosition(1, Vector2.zero);
        foreach (var li in light2Ds) li.gameObject.SetActive(false);
        int lineCount = lineRenderers.Length;
        float[] delays = new float[lineCount];
        float[] speeds = new float[lineCount];
        int baseSeed = platform.name.GetHashCode() + System.DateTime.Now.Month;
        for (int i = 0; i < lineCount; i++)
        {
            int individualSeed = baseSeed + i;
            float seedRatio = (Mathf.Abs(individualSeed * 15485863) % 1000000) / 1000000f;
            delays[i] = seedRatio * 0.3f;
            float speedVar = 0.9f + (seedRatio * 0.8f);
            speeds[i] = (1f / (1f - (delays[i] / 1.0f))) * speedVar;
            if (light2Ds[i] != null) light2Ds[i].gameObject.SetActive(true);
        }
        float duration = 3f;
        float elapsed = 0f;
        float s = 0.08f; // 라이트 두께 오프셋
        bool collflag = false;
        while (elapsed < duration)
        {
            await Task.Yield();
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / duration);
            if (ratio > 0.6f && !collflag)
            {
                collflag = true;
                platformColl.enabled = true;
            }
            for (int k = 0; k < lineCount; k++)
            {
                float timeRatio = delays[k] / duration;
                float individualRatio = Mathf.Clamp01((ratio - timeRatio) * speeds[k]);
                Vector3[] path = new Vector3[4];
                if (k <= 2) // 가로 라인
                {
                    float currentX = lineXLength * individualRatio;
                    lineRenderers[k].SetPosition(1, new Vector3(currentX, 0f, 0f));
                    path[0] = new Vector3(0, s, 0);
                    path[1] = new Vector3(0, -s, 0);
                    path[2] = new Vector3(currentX + s, -s, 0);
                    path[3] = new Vector3(currentX + s, s, 0);
                }
                else // 세로 라인
                {
                    float currentY = -lineYLength * individualRatio;
                    lineRenderers[k].SetPosition(1, new Vector3(0f, currentY, 0f));
                    path[0] = new Vector3(-s, 0, 0);
                    path[1] = new Vector3(-s, currentY - s, 0);
                    path[2] = new Vector3(s, currentY - s, 0);
                    path[3] = new Vector3(s, 0, 0);
                }
                if (light2Ds[k] != null) light2Ds[k].SetShapePath(path);
            }
        }
        Step4();
    }
    public void Step4()
    {
        platformColl.enabled = true;
        platform?.SetActive(true);
        ps?.gameObject.SetActive(false);
        lp?.gameObject.SetActive(false);
        light2?.gameObject.SetActive(true);
        tweenLight2?.Kill();
        light2.intensity = 1.8f;
        int lineCount = lineRenderers.Length;
        float s = 0.08f;
        for (int i = 0; i < lineCount; i++)
        {
            if (i <= 2) lineRenderers[i].SetPosition(1, new Vector3(lineXLength, 0f, 0f));
            else lineRenderers[i].SetPosition(1, new Vector3(0f, -lineYLength, 0f));
            // 최종 상태 라이트 확정
            Vector3[] finalPath = new Vector3[4];
            if (i <= 2)
            {
                finalPath[0] = new Vector3(0, s, 0); finalPath[1] = new Vector3(0, -s, 0);
                finalPath[2] = new Vector3(lineXLength + s, -s, 0); finalPath[3] = new Vector3(lineXLength + s, s, 0);
            }
            else
            {
                finalPath[0] = new Vector3(-s, 0, 0); finalPath[1] = new Vector3(-s, -lineYLength - s, 0);
                finalPath[2] = new Vector3(s, -lineYLength - s, 0); finalPath[3] = new Vector3(s, 0, 0);
            }
            if (light2Ds[i] != null) light2Ds[i].SetShapePath(finalPath);
        }
    }
}
[BurstCompile]
public struct ParticleFireflyJob : IJobParallelFor
{
    public NativeArray<Vector3> Positions;
    public NativeArray<Vector3> Velocities;
    [Unity.Collections.ReadOnly] public NativeArray<Vector3> Targets;
    public float DeltaTime;
    public float Speed;
    public float SteeringForce;
    public float NoiseStrength;
    public float TimeInput;
    public void Execute(int index)
    {
        Vector3 pos = Positions[index];
        Vector3 vel = Velocities[index];
        Vector3 target = Targets[index];
        Vector3 desired = (target - pos).normalized * Speed;
        Vector3 steer = desired - vel;
        vel += steer * SteeringForce * DeltaTime;
        float speedMultiplier = Mathf.Clamp01(vel.magnitude / 1.0f);
        float noiseX = Mathf.Sin(TimeInput * 2f + index) * NoiseStrength * speedMultiplier;
        float noiseY = Mathf.Cos(TimeInput * 1.5f + index) * NoiseStrength * speedMultiplier;
        vel += new Vector3(noiseX, noiseY, 0);
        if (vel.magnitude > Speed)
            vel = vel.normalized * Speed;
        pos += vel * DeltaTime;
        Positions[index] = pos;
        Velocities[index] = vel;
    }
}
