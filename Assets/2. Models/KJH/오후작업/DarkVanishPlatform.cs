using UnityEngine;
//using Unity.Mathematics;
// using Unity.Burst;
// using Unity.Jobs;
// using Unity.Collections;
using UnityEngine.Rendering.Universal;
using NaughtyAttributes;
// using System.Threading.Tasks;
using DG.Tweening;
using System.Threading.Tasks;
// using Random = UnityEngine.Random;
public class DarkVanishPlatform : Lanternable, ISavable
{
    #region Lanternable Complement
    public override bool isReady { get { return _isReady; } set { _isReady = value; } }
    public override bool isAuto => false;
    public override ParticleSystem particle => fogParticle;
    public override SpriteRenderer lightPoint => lp;
    #endregion
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete;
    bool ISavable.CanReplay => false;
    int ISavable.ReplayWaitTimeSecond => 0;
    public void SetCompletedState()
    {
        _isReady = false;
        isComplete = true;
        Step99();
    }
    #endregion
    bool _isReady;
    ParticleSystem fogParticle;
    SpriteRenderer lp;
    [SerializeField] GameObject platform;
    Collider2D platformColl;
    void Awake()
    {
        _isReady = true;
        isComplete = false;
        platform?.SetActive(true);
        platform.TryGetComponent(out platformColl);
        fogParticle = transform.Find("DarkFogParticle").GetComponent<ParticleSystem>();
        fogParticle?.gameObject.SetActive(true);
        lp = transform.Find("LightPoint").GetComponent<SpriteRenderer>();
        lp?.gameObject.SetActive(false);
        lpLight = lp.transform.GetComponentInChildren<Light2D>(true);
        lpParticle = lp.transform.GetComponentInChildren<ParticleSystem>(true);
        lpParticle.gameObject.SetActive(false);
        transform.Find("DarkVanishParticle").TryGetComponent(out dvParticle);
        dvParticle.gameObject.SetActive(false);
    }
    public override void Run()
    {
        _isReady = false;
        isComplete = true;
        AudioManager.I.PlaySFX("UIClick2");

    }
    Tween tweenLpLight;
    Light2D lpLight;
    public override void PromptFill()
    {
        lp?.gameObject.SetActive(true);
        DOTween.Kill(lp);
        tweenLpLight?.Kill();
        lp.color = new Color(lp.color.r, lp.color.g, lp.color.b, 0f);
        lpLight.intensity = 0f;
        lp.DOFade(1f, 0.5f).SetEase(Ease.InSine);
        tweenLpLight = DOTween.To(() => lpLight.intensity, x => lpLight.intensity = x, 0.5f, 0.5f).SetEase(Ease.InSine).Play();
    }
    public override void PromptCancel()
    {
        DOTween.Kill(lp);
        tweenLpLight?.Kill();
        lp.DOFade(0f, 2.2f).SetEase(Ease.InSine);
        tweenLpLight = DOTween.To(() => lpLight.intensity, x => lpLight.intensity = x, 0f, 2.2f).SetEase(Ease.InSine)
        .OnComplete(() => lp.gameObject.SetActive(false)).Play();
    }
    void Start()
    {

    }

    ParticleSystem lpParticle;
    ParticleSystem dvParticle;
    // 단계적 작업
    // step1. Light가 집중 조사받는 지점에 lpParticle 파티클(Loop) 재생 
    // -> 약간 시간흐른뒤 검은색 분해되며 피어오르는 dvParticle 재생(Loop 아니므로 이건 나중에 따로 꺼줄필요 X)
    // -> 약간 시간 흐른뒤 lpParticle 파티클 루프 중단
    // step2. 스프라이트가 검은색으로 수많은 가루로 분해되며 재/연기/가루 느낌으로 산산히 바스러지며 흩날리는 연출 
    // step3. 검은색 때가 다 빠진 오브젝트들이 와르르 무너짐. 알고보니 책상 고철등 잡동사니들이 쌓여있던거였고 그동안 시커먼 어둠이 접착제느낌으로 서로붙여서 장애물을 형성했던것.
    // step4. 파티클시스템으로 이루어진 검은 포그 파티클들 점점 걷히는 처리. 안개가 걷히는 듯한 좋은 연출 필요
    // step5. 다각형의 프리폼라이트가 있고 강조선 모양으로 몇부분 돌출되는듯한 (일종의 라이징썬의 그 무늬). 설명하기 어려워서 그런데 포그 중심에서부터 빛기둥이 3~4 군데방향으로 방사형으로 나오는 연출임
    // 프리폼 라이트를 서서히 0으로 만들고 끝.
    [Button]
    public async void Step1()
    {
        lp.gameObject.SetActive(true);
        lpParticle.gameObject.SetActive(true);
        lpParticle.Play();
        await Task.Delay(1500);
        dvParticle.gameObject.SetActive(true);
        dvParticle.Play();
        await Task.Delay(1500);
        var mainModule = lpParticle.main;
        mainModule.loop = false;

    }

    // 스프라이트 디졸브
    [Button]
    public async void Step2()
    {

    }


    // fogParticle 걷혀가기
    // fogParticle 는현재 파티클시스템으로 셰이프로는 렉탱글 직사각형 영역으로
    // 속도 0짜리 안개모양의 검은색 파티클들이 매우 빽빽하게 직사각형을 둘러싸고 있어서 검은 안개처럼 보이는중
    // 이 개별 파티클들에 렉탱글에서 벗어나는 방사형 속도를 아주 약간 0.1 속도(?) 주면서 서서히 파티클 개별 랜덤적으로 투명도를 0으로 바꾸는게 좋나?
    // 하여튼 파티클로 이루어진 검은 안개 밝혀서 걷혀가는 연출?로 더 좋은게 있으면 함 알려주고
    [Button]
    public async void Step4()
    {
        // 1. 새로운 안개 생성 중단
        var emission = fogParticle.emission;
        emission.enabled = false;

        // 2. 이미 생성된 파티클들을 제어하기 위한 배열 준비
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[fogParticle.main.maxParticles];

        float elapsed = 0f;
        float fadeDuration = 10f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeDuration;

            int numParticlesAlive = fogParticle.GetParticles(particles);

            for (int i = 0; i < numParticlesAlive; i++)
            {
                // 중심부(플랫폼 위치)에서 파티클까지의 방향 계산
                Vector3 direction = (particles[i].position - Vector3.zero).normalized;

                // 방사형으로 밀려나는 속도 부여 (점점 빨라지게)
                particles[i].velocity += direction * 0.2f * Time.deltaTime;

                // 투명도를 랜덤하게 0으로 (Step-down 느낌)
                Color col = particles[i].startColor;
                col.a = Mathf.Lerp(col.a, 0, progress + Random.Range(0, 0.1f));
                particles[i].startColor = col;
            }

            // 변경된 파티클 데이터 적용
            fogParticle.SetParticles(particles, numParticlesAlive);

            await Task.Yield(); // 다음 프레임까지 대기
        }
    }



    [Button]
    void Step99()
    {
        Transform[] children = transform.GetComponentsInChildren<Transform>();
        foreach (var child in children)
            child.gameObject.SetActive(false);
    }





}
