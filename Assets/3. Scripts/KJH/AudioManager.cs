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

    public void PlayBGMWithFade(AudioClip clip, float duration = 1f)
    {
        StopCoroutine("PlayBGM_co");
        // 소스 스위칭 (0 -> 1, 1 -> 0)
        AudioSource nextAus = (currentAus == ausBGM0) ? ausBGM1 : ausBGM0;
        nextAus.clip = clip;
        nextAus.Play();
        StartCoroutine(PlayBGM_co(currentAus, nextAus, duration));
        currentAus = nextAus;
    }
    public void PlayBGMWithFade(string bgmName, float duration = 1f)
    {
        StopCoroutine("PlayBGM_co");
        // 소스 스위칭 (0 -> 1, 1 -> 0)
        int find = bgmList.FindIndex(x => x.name == bgmName);
        if(find == -1) return;
        AudioSource nextAus = (currentAus == ausBGM0) ? ausBGM1 : ausBGM0;
        nextAus.clip = bgmList[find];
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

    // [AudioManager.cs] 기존 코드 하단에 추가

    // 몬스터 시스템 연동을 위한 확장 함수 (클립 직접 재생 + 고급 설정)
    public void PlayDirectSFX(AudioClip clip, Vector3 pos, float volume, float pitch, float startTime, float endTime)
    {
        if (clip == null) return;

        // 1. 기존 풀링 시스템을 이용해 SFX 객체 소환 (부모를 null이나 Manager로 지정해 몬스터가 죽어도 유지됨)
        // 기존 PlaySFX 로직을 활용하되, 리스트 검색 없이 바로 스폰합니다.
        PoolBehaviour pb = PoolManager.I?.Spawn(sfxPrefab, pos, Quaternion.identity, transform);
        
        if (pb == null) return;

        // 2. SFX 프리팹 내부의 AudioSource 접근
        AudioSource source = pb.GetComponent<AudioSource>();
        if (source == null) return;

        // 3. 고급 설정 적용
        source.clip = clip;
        source.spatialBlend = 1f; // 3D 사운드 (위치를 잡았으므로)
        source.volume = volume * volumeSFX; // 글로벌 SFX 볼륨 적용
        source.pitch = pitch;
        source.time = startTime; // 시작 지점 점프

        source.Play();

        // 4. 종료 시간 스케줄링 (구간 재생인 경우)
        if (endTime > 0 && endTime > startTime)
        {
            double duration = endTime - startTime;
            source.SetScheduledEndTime(AudioSettings.dspTime + duration);
        }
        // 일반 재생인 경우 (PlayOneShot 대신 Play를 쓰되, 클립 길이만큼만 재생 후 풀 반환은 SFX 스크립트가 처리한다고 가정)
        else
        {
            // SFX 프리팹이 스스로 비활성화되는 로직이 있다면 그대로 둡니다.
            // 만약 SFX 프리팹이 PlayOneShot만 기다린다면, 여기서 Invoke 등을 통해 비활성화 처리가 필요할 수 있습니다.
            // (기존 SFX 구조를 존중하여 Play()만 실행)
        }
    }


}
