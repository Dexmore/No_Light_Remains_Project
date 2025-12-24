using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq; // 가중치 계산을 위해 추가

public class AudioManager : SingletonBehaviour<AudioManager>
{
    protected override bool IsDontDestroy() => true;

    [System.Serializable]
    public struct MyStruct1
    {
        public string scneneName;
        public List<MyStruct2> list;
    }

    [System.Serializable]
    public struct MyStruct2
    {
        public AudioClip audioClip;
        public int weight;
    }

    [SerializeField] List<MyStruct1> autoBGM;
    [Space(50)]
    [SerializeField] List<AudioClip> bgmList = new List<AudioClip>();
    [SerializeField] List<AudioClip> sfxList = new List<AudioClip>();
    [SerializeField] SFX sfxPrefab;

    AudioSource ausBGM0;
    AudioSource ausBGM1;

    // 현재 활성화된 오디오 소스를 추적 (크로스페이드를 위함)
    AudioSource currentAus;
    Coroutine autoBGMCoroutine;

    [ReadOnlyInspector][SerializeField] float volumeBGM = 1f;
    [ReadOnlyInspector][SerializeField] float volumeSFX = 1f;

    protected override void Awake()
    {
        base.Awake();
        transform.GetChild(0).TryGetComponent(out ausBGM0);
        transform.GetChild(1).TryGetComponent(out ausBGM1);
        currentAus = ausBGM0;
    }

    void OnEnable()
    {
        if (GameManager.I != null)
            GameManager.I.onSceneChange += HandlerSceneChange;
    }

    void OnDisable()
    {
        if (GameManager.I != null)
            GameManager.I.onSceneChange -= HandlerSceneChange;
    }

    void HandlerSceneChange()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int find = autoBGM.FindIndex(x => x.scneneName == sceneName);

        // 1 & 2. 씬 등록이 없거나 리스트가 비어있는 경우
        if (find == -1 || autoBGM[find].list.Count == 0)
        {
            StopAutoBGM();
            FadeOutBGM(2.0f); // 2초간 페이드 아웃
            return;
        }

        var sceneBGMList = autoBGM[find].list;

        // 현재 재생 중인 클립 확인
        AudioClip currentClip = currentAus.clip;
        bool isAlreadyPlaying = sceneBGMList.Any(x => x.audioClip == currentClip);

        if (isAlreadyPlaying && currentAus.isPlaying)
        {
            // 3. 이미 현재 씬의 음악 중 하나가 재생 중인 경우
            // 아무것도 안 함 (현재 곡이 끝나면 다음 곡을 고르도록 루프 코루틴만 체크)
            if (autoBGMCoroutine == null)
                autoBGMCoroutine = StartCoroutine(Co_AutoBGMNextTrack(sceneBGMList));
        }
        else
        {
            // 4. 새로운 씬 음악으로 교체해야 하는 경우
            AudioClip nextClip = GetRandomClipByWeight(sceneBGMList);
            PlayBGMWithFade(nextClip, 2.0f);

            if (autoBGMCoroutine != null) StopCoroutine(autoBGMCoroutine);
            autoBGMCoroutine = StartCoroutine(Co_AutoBGMNextTrack(sceneBGMList));
        }
    }

    // 가중치 기반 랜덤 선택 로직
    AudioClip GetRandomClipByWeight(List<MyStruct2> list)
    {
        int totalWeight = list.Sum(x => x.weight);
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var item in list)
        {
            currentWeight += item.weight;
            if (randomValue < currentWeight)
                return item.audioClip;
        }
        return list[0].audioClip;
    }

    // 자동 재생 루프 관리 코루틴
    IEnumerator Co_AutoBGMNextTrack(List<MyStruct2> list)
    {
        while (true)
        {
            // 현재 곡이 거의 끝나갈 때 (약 0.5초 전) 다음 곡 준비
            if (currentAus.clip != null && currentAus.time >= currentAus.clip.length - 0.5f)
            {
                AudioClip nextClip = GetRandomClipByWeight(list);
                PlayBGMWithFade(nextClip, 1.5f);
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    void StopAutoBGM()
    {
        if (autoBGMCoroutine != null)
        {
            StopCoroutine(autoBGMCoroutine);
            autoBGMCoroutine = null;
        }
    }

    public void PlayBGMWithFade(AudioClip clip, float duration)
    {
        StopCoroutine("PlayBGM_co");

        // 소스 스위칭 (0 -> 1, 1 -> 0)
        AudioSource nextAus = (currentAus == ausBGM0) ? ausBGM1 : ausBGM0;

        nextAus.clip = clip;
        nextAus.Play();

        StartCoroutine(PlayBGM_co(currentAus, nextAus, duration));
        currentAus = nextAus;
    }

    IEnumerator PlayBGM_co(AudioSource fadeOutAus, AudioSource fadeInAus, float duration)
    {
        float startTime = Time.time;
        float startVol = fadeOutAus.volume;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            fadeInAus.volume = Mathf.Lerp(0f, volumeBGM, t);
            fadeOutAus.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }

        fadeInAus.volume = volumeBGM;
        fadeOutAus.volume = 0f;
        fadeOutAus.Stop();
    }

    public void FadeOutBGM(float duration)
    {
        StartCoroutine(Co_FadeOut(currentAus, duration));
    }

    IEnumerator Co_FadeOut(AudioSource aus, float duration)
    {
        float startVol = aus.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            aus.volume = Mathf.Lerp(startVol, 0, t / duration);
            yield return null;
        }
        aus.volume = 0;
        aus.Stop();
    }
    public SFX PlaySFX(string Name, Vector3 pos, Transform parent = null, float spatialBlend = 0f, float vol = 1f)
    {
        int find = -1;
        for (int i = 0; i < sfxList.Count; i++)
        {
            if (sfxList[i].name == Name)
            {
                //Debug.Log(sfxList[i].name);
                find = i;
                break;
            }
        }
        if (find == -1) return null;
        if (parent == null) parent = transform;
        PoolBehaviour pb = PoolManager.I?.Spawn(sfxPrefab, pos, Quaternion.identity, parent);
        SFX _pb = pb as SFX;
        float _vol = volumeSFX * vol;
        _pb.Play(sfxList[find], _vol, sfxList[find].length, spatialBlend);
        return _pb;
    }
    public SFX PlaySFX(string Name, Transform parent = null)
    {
        return PlaySFX(Name, Vector3.zero, null);
    }
    void Start()
    {
        ausBGM0.loop = true;
        ausBGM1.loop = true;
    }



}
