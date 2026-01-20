using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System.Threading.Tasks;
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
    public void SetCompletedImmediately()
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
    Light2D darkVanishLight2D;
    void Awake()
    {
        _isReady = true;
        isComplete = false;
        platform?.SetActive(true);
        fogParticle = transform.Find("DarkFogParticle").GetComponent<ParticleSystem>();
        fogParticle?.gameObject.SetActive(true);
        lp = transform.Find("LightPoint").GetComponent<SpriteRenderer>();
        lp?.gameObject.SetActive(false);
        lpLight = lp.transform.GetComponentInChildren<Light2D>(true);
        transform.Find("LightPointParticle").TryGetComponent(out lpParticle);
        lpParticle.gameObject.SetActive(false);
        transform.Find("DarkVanishParticle").TryGetComponent(out dvParticle);
        dvParticle.gameObject.SetActive(false);
        if (platform != null)
        {
            var renderer = platform.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.material.SetFloat("_DissolveAmount", 1f);
            }
        }
        darkVanishLight2D = transform.Find("DarkVanishLight2D").GetComponent<Light2D>();
    }
    public override void Run()
    {
        _isReady = false;
        isComplete = true;
        AudioManager.I.PlaySFX("UIClick2");
        AudioManager.I.PlaySFX("DarkVanish", fogParticle.transform.position, null, spatialBlend: 0.4f);
        Step1();
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
        lp.DOFade(1f, 0.5f).SetEase(Ease.InSine).SetLink(gameObject);
        tweenLpLight = DOTween.To(() => lpLight.intensity, x => lpLight.intensity = x, 0.5f, 0.5f).SetLink(gameObject).SetEase(Ease.InSine).Play();
        lpParticle.gameObject.SetActive(true);
        lpParticle.Stop();
        lpParticle.Play();
        SubParticleOnFill();
    }
    async void SubParticleOnFill()
    {
        float subParticleInterval = Random.Range(0.6f, 1.8f);
        float _time = Time.time;
        while (!isCancel)
        {
            if (Time.time - _time > subParticleInterval)
            {
                _time = Time.time;
                subParticleInterval = Random.Range(0.6f, 1.8f);
                ParticleManager.I.PlayParticle("DarkDust", lp.transform.position, Quaternion.identity, null);
            }
            await Task.Yield();
        }
    }
    bool isCancel;
    public override void PromptCancel()
    {
        isCancel = true;
        DOTween.Kill(lp);
        tweenLpLight?.Kill();
        lp.DOFade(0f, 1.1f).SetEase(Ease.InSine).SetLink(gameObject);
        tweenLpLight = DOTween.To(() => lpLight.intensity, x => lpLight.intensity = x, 0f, 1.1f).SetEase(Ease.InSine)
        .SetLink(gameObject).OnComplete(() => lp.gameObject.SetActive(false)).Play();
        lpParticle.Stop();
    }
    ParticleSystem lpParticle;
    ParticleSystem dvParticle;
    public async void Step1()
    {
        isCancel = true;
        lp.gameObject.SetActive(true);
        lpParticle.Stop();
        lpParticle.Play();
        dvParticle.gameObject.SetActive(true);
        darkVanishLight2D.gameObject.SetActive(true);
        // darkVanishLight2D
        dvParticle.Play();
        if (platform != null)
        {
            var renderer = platform.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.material.SetFloat("_DissolveAmount", 1f);
                renderer.material.DOFloat(0f, "_DissolveAmount", 3.2f).SetEase(Ease.InSine).SetLink(gameObject);
            }
        }
        await Task.Delay(200);
        var mainModule = lpParticle.main;
        mainModule.loop = false;
        await Task.Delay(3000);
        platform.SetActive(false);
        _isReady = false;
        isComplete = true;
        Step2();
    }
    public async void Step2()
    {
        var emission = fogParticle.emission;
        emission.enabled = false;
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[fogParticle.main.maxParticles];
        float elapsed = 0f;
        float fadeDuration = 6.0f;
        SpriteRenderer spr = platform.GetComponent<SpriteRenderer>();
        float xLength = spr.bounds.size.x;
        float yLength = spr.bounds.size.y;
        float subParticleInterval = Random.Range(0.1f, 0.6f);
        float _time;
        _time = Time.time;
        while (elapsed < fadeDuration)
        {
            if (this == null) return;
            elapsed += Time.deltaTime;
            // deltaTime에 따른 알파 감소량 (1초당 1/fadeDuration 만큼 감소)
            float alphaDecrease = Time.deltaTime / fadeDuration;
            int numParticlesAlive = fogParticle.GetParticles(particles);
            for (int i = 0; i < numParticlesAlive; i++)
            {
                Vector3 direction = (particles[i].position - Vector3.zero).normalized;
                particles[i].velocity += direction * 0.5f * Time.deltaTime;
                // 2. 알파값 감소
                Color col = particles[i].GetCurrentColor(fogParticle);
                float randomFactor = Random.Range(0.5f, 1.5f);
                col.a = Mathf.Max(0, col.a - (alphaDecrease * randomFactor));
                particles[i].startColor = col;
            }
            fogParticle.SetParticles(particles, numParticlesAlive);
            await Task.Yield();
            if (Time.time - _time > subParticleInterval && elapsed < fadeDuration * 0.5f)
            {
                _time = Time.time;
                subParticleInterval = Random.Range(0.1f, 0.6f);
                float rnd = Random.value;
                Vector2 pivot = rnd * fogParticle.transform.position + (1 - rnd) * transform.position + Random.Range(-0.5f, 0.5f) * Vector3.up;
                Vector2 jitter = new Vector2(Random.Range(-0.4f, 0.4f) * xLength, Random.Range(-0.4f, 0.4f) * yLength);
                ParticleManager.I.PlayParticle("DarkDust", pivot + jitter, Quaternion.identity, null);
            }
        }
        darkVanishLight2D.gameObject.SetActive(false);
    }
    void Step99()
    {
        Transform[] children = transform.GetComponentsInChildren<Transform>();
        foreach (var child in children)
            child.gameObject.SetActive(false);
        darkVanishLight2D.gameObject.SetActive(false);
    }





}
