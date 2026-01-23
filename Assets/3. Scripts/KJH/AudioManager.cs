using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
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

    public List<MyStruct1> autoBGM;
    [Space(50)]
    [SerializeField] List<AudioClip> bgmList = new List<AudioClip>();
    [SerializeField] List<AudioClip> sfxList = new List<AudioClip>();
    [SerializeField] SFX sfxPrefab;

    AudioSource ausBGM0;
    AudioSource ausBGM1;

    AudioSource currentAus;
    Coroutine autoBGMCoroutine;
    Coroutine fadeCoroutine; // 페이드 코루틴 참조 저장용
    
    private bool isChangingTrack = false; // 중복 실행 방지 플래그
    private AudioClip lastPlayedClip;    // 직전 재생 곡 기억

    [ReadOnlyInspector][SerializeField] float volumeBGM = 1f;
    [ReadOnlyInspector][SerializeField] float volumeSFX = 1f;

    protected override void Awake()
    {
        base.Awake();
        transform.GetChild(0).TryGetComponent(out ausBGM0);
        transform.GetChild(1).TryGetComponent(out ausBGM1);
        currentAus = ausBGM0;

        // 자동 전환 시스템을 위해 루프는 끄는 것이 관리하기 편합니다.
        ausBGM0.loop = false;
        ausBGM1.loop = false;
    }

    void OnEnable()
    {
        if (GameManager.I != null)
            GameManager.I.onSceneChange += SceneChangeHandler;
    }

    void OnDisable()
    {
        if (GameManager.I != null)
            GameManager.I.onSceneChange -= SceneChangeHandler;
    }

    void SceneChangeHandler()
    {
        StartAutoBGM();
    }

    public void StartAutoBGM()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int find = autoBGM.FindIndex(x => x.scneneName == sceneName);

        if (find == -1 || autoBGM[find].list.Count == 0)
        {
            StopAutoBGM();
            FadeOutBGM(2.0f);
            return;
        }

        var sceneBGMList = autoBGM[find].list;

        bool isAlreadyPlaying = false;
        if (currentAus && currentAus.clip != null)
        {
            isAlreadyPlaying = sceneBGMList.Any(x => x.audioClip == currentAus.clip);
        }

        if (isAlreadyPlaying && currentAus.isPlaying)
        {
            if (autoBGMCoroutine == null)
                autoBGMCoroutine = StartCoroutine(Co_AutoBGMNextTrack(sceneBGMList));
        }
        else
        {
            // 새 씬 진입 시 첫 곡 재생
            isChangingTrack = false; 
            AudioClip nextClip = GetRandomClipByWeight(sceneBGMList);
            PlayBGMWithFade(nextClip, 2.0f);

            if (autoBGMCoroutine != null) StopCoroutine(autoBGMCoroutine);
            autoBGMCoroutine = StartCoroutine(Co_AutoBGMNextTrack(sceneBGMList));
        }
    }

    // 가중치 기반 랜덤 선택 (똑같은 곡 반복 방지 포함)
    AudioClip GetRandomClipByWeight(List<MyStruct2> list)
    {
        if (list == null || list.Count == 0) return null;
        if (list.Count == 1) return list[0].audioClip;

        // 직전 곡을 제외한 리스트에서 선택 (확률 체감 개선)
        var filteredList = list.Where(x => x.audioClip != lastPlayedClip).ToList();
        var targetList = filteredList.Count > 0 ? filteredList : list;

        int totalWeight = targetList.Sum(x => x.weight);
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var item in targetList)
        {
            currentWeight += item.weight;
            if (randomValue < currentWeight)
            {
                lastPlayedClip = item.audioClip;
                return item.audioClip;
            }
        }
        return targetList[0].audioClip;
    }

    // 자동 재생 루프 관리 코루틴 (렉 및 중복 실행 방지)
    IEnumerator Co_AutoBGMNextTrack(List<MyStruct2> list)
    {
        while (true)
        {
            // 현재 곡이 거의 끝나갈 때 (약 1.5초 전) 다음 곡 준비
            if (!isChangingTrack && currentAus != null && currentAus.clip != null)
            {
                if (currentAus.time >= currentAus.clip.length - 1.6f)
                {
                    isChangingTrack = true;
                    AudioClip nextClip = GetRandomClipByWeight(list);
                    PlayBGMWithFade(nextClip, 1.5f);

                    // 페이드 시간 동안 대기하여 중복 실행 원천 차단
                    yield return new WaitForSeconds(2.0f);
                    isChangingTrack = false;
                }
            }
            yield return new WaitForSeconds(0.5f); // 체크 주기 최적화
        }
    }

    void StopAutoBGM()
    {
        if (autoBGMCoroutine != null)
        {
            StopCoroutine(autoBGMCoroutine);
            autoBGMCoroutine = null;
        }
        isChangingTrack = false;
    }

    public void PlayBGMWithFade(AudioClip clip, float duration = 1f)
    {
        if (clip == null) return;
        
        // 이전 페이드가 진행 중이라면 중단
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        AudioSource nextAus = (currentAus == ausBGM0) ? ausBGM1 : ausBGM0;
        nextAus.clip = clip;
        nextAus.volume = 0f;
        nextAus.Play();

        fadeCoroutine = StartCoroutine(PlayBGM_co(currentAus, nextAus, duration));
        currentAus = nextAus;
    }

    public void StopBGM()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        ausBGM0.Stop();
        ausBGM1.Stop();
        isChangingTrack = false;
    }

    // 기존의 이름 기반 재생 함수도 참조 방식으로 수정
    public void PlayBGMWithFade(string bgmName, float duration = 1f)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (string.IsNullOrEmpty(bgmName))
        {
            fadeCoroutine = StartCoroutine(PlayBGM_co(currentAus, null, duration));
            currentAus = null;
            return;
        }

        int find = bgmList.FindIndex(x => x.name == bgmName);
        if (find == -1)
        {
            Debug.LogWarning($"BGM {bgmName}을 찾을 수 없습니다.");
            return;
        }

        AudioSource nextAus = (currentAus == ausBGM0) ? ausBGM1 : ausBGM0;
        nextAus.clip = bgmList[find];
        nextAus.volume = 0f;
        nextAus.Play();

        fadeCoroutine = StartCoroutine(PlayBGM_co(currentAus, nextAus, duration));
        currentAus = nextAus;
    }

    IEnumerator PlayBGM_co(AudioSource fadeOutAus, AudioSource fadeInAus, float duration)
    {
        float startTime = Time.time;
        float startVol = (fadeOutAus != null) ? fadeOutAus.volume : 0f;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;

            if (fadeInAus != null)
                fadeInAus.volume = Mathf.Lerp(0f, volumeBGM, t);

            if (fadeOutAus != null)
                fadeOutAus.volume = Mathf.Lerp(startVol, 0f, t);

            yield return null;
        }

        if (fadeInAus != null) fadeInAus.volume = volumeBGM;
        if (fadeOutAus != null)
        {
            fadeOutAus.volume = 0f;
            fadeOutAus.Stop();
        }
        fadeCoroutine = null;
    }

    public void FadeOutBGM(float duration)
    {
        if (currentAus != null)
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
        int find = sfxList.FindIndex(x => x.name == Name);
        if (find == -1)
        {
            Debug.Log($"{Name} 효과음이 등록되지 않았습니다.");
            return null;
        }
        
        if (parent == null) parent = transform;
        PoolBehaviour pb = PoolManager.I?.Spawn(sfxPrefab, pos, Quaternion.identity, parent);
        SFX _pb = pb as SFX;
        float _vol = volumeSFX * vol;
        _pb.Play(sfxList[find], _vol, sfxList[find].length, spatialBlend);
        return _pb;
    }

    public SFX PlaySFX(string Name, Transform parent = null)
    {
        return PlaySFX(Name, Vector3.zero, parent);
    }

    public void PlayDirectSFX(AudioClip clip, Vector3 pos, float volume, float pitch, float startTime, float endTime)
    {
        if (clip == null) return;
        PoolBehaviour pb = PoolManager.I?.Spawn(sfxPrefab, pos, Quaternion.identity, transform);
        if (pb == null) return;

        AudioSource source = pb.GetComponent<AudioSource>();
        if (source == null) return;

        source.clip = clip;
        source.spatialBlend = 1f;
        source.volume = volume * volumeSFX;
        source.pitch = pitch;
        source.time = startTime;
        source.Play();

        if (endTime > 0 && endTime > startTime)
        {
            double duration = endTime - startTime;
            source.SetScheduledEndTime(AudioSettings.dspTime + duration);
        }
    }
}