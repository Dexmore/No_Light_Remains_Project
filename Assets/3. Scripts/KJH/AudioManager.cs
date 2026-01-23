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

    private bool isChangingTrack = false;
    private AudioClip lastPlayedClip;

    [ReadOnlyInspector][SerializeField] float volumeBGM = 1f;
    [ReadOnlyInspector][SerializeField] float volumeSFX = 1f;

    protected override void Awake()
    {
        base.Awake();
        transform.GetChild(0).TryGetComponent(out ausBGM0);
        transform.GetChild(1).TryGetComponent(out ausBGM1);
        currentAus = ausBGM0;

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

        // 해당 씬에 설정된 BGM이 없으면 중지
        if (find == -1 || autoBGM[find].list.Count == 0)
        {
            StopBGM();
            return;
        }

        var sceneBGMList = autoBGM[find].list;

        // 이미 해당 씬의 리스트 중 하나가 재생 중인지 확인
        bool isAlreadyPlaying = false;
        if (currentAus && currentAus.clip != null)
        {
            isAlreadyPlaying = sceneBGMList.Any(x => x.audioClip == currentAus.clip);
        }

        // 중복 실행 방지: 자동 재생 코루틴 재시작
        StopCoroutine(nameof(Co_AutoBGMNextTrack));

        if (isAlreadyPlaying && currentAus.isPlaying)
        {
            // 이미 맞는 곡이 나오고 있다면 감시 코루틴만 다시 시작
            StartCoroutine(nameof(Co_AutoBGMNextTrack), sceneBGMList);
        }
        else
        {
            // 새 곡 재생 및 코루틴 시작
            isChangingTrack = false;
            AudioClip nextClip = GetRandomClipByWeight(sceneBGMList);
            PlayBGMWithFade(nextClip, 2.0f);
            StartCoroutine(nameof(Co_AutoBGMNextTrack), sceneBGMList);
        }
    }

    // --- 재생 및 정지 핵심 로직 ---

    public void StopBGM()
    {
        // 모든 관련 코루틴 이름 기반으로 완전 정지
        StopCoroutine(nameof(Co_AutoBGMNextTrack));
        StopCoroutine(nameof(PlayBGM_co));
        StopCoroutine(nameof(Co_FadeOut));
        StopAllCoroutines();

        if (ausBGM0 != null) { ausBGM0.Stop(); ausBGM0.clip = null; }
        if (ausBGM1 != null) { ausBGM1.Stop(); ausBGM1.clip = null; }

        isChangingTrack = false;
        lastPlayedClip = null;
    }

    public void PlayBGMWithFade(AudioClip clip, float duration = 1f)
    {
        if (clip == null) return;

        // 페이드 코루틴 중복 방지
        StopCoroutine(nameof(PlayBGM_co));

        AudioSource nextAus = (currentAus == ausBGM0) ? ausBGM1 : ausBGM0;
        nextAus.clip = clip;
        nextAus.volume = 0f;
        nextAus.Play();

        // 매개변수가 여러 개인 경우 관리를 위해 기존처럼 호출하되, 
        // PlayBGM_co 내부에서 관리하거나 아래와 같이 실행
        StartCoroutine(PlayBGM_co(currentAus, nextAus, duration));
        currentAus = nextAus;
    }
    public void PlayBGMWithFade(string bgmName, float duration = 1f)
    {
        // 1. 중복 실행 방지
        StopCoroutine(nameof(PlayBGM_co));

        // 2. 리스트에서 이름으로 클립 찾기
        int find = bgmList.FindIndex(x => x != null && x.name == bgmName);

        if (find == -1)
        {
            Debug.LogWarning($"BGM [{bgmName}]을 bgmList에서 찾을 수 없습니다.");
            return;
        }

        // 3. 찾은 클립으로 실제 재생 함수 호출
        PlayBGMWithFade(bgmList[find], duration);
    }

    public void FadeOutBGM(float duration)
    {
        StopCoroutine(nameof(Co_FadeOut));
        if (currentAus != null)
            StartCoroutine(nameof(Co_FadeOut), duration);
    }

    // --- 코루틴 구현부 ---

    IEnumerator Co_AutoBGMNextTrack(List<MyStruct2> list)
    {
        while (true)
        {
            if (!isChangingTrack && currentAus != null && currentAus.clip != null && currentAus.isPlaying)
            {
                // 곡 종료 약 1.6초 전 다음 곡 준비
                if (currentAus.time >= currentAus.clip.length - 1.6f)
                {
                    isChangingTrack = true;
                    AudioClip nextClip = GetRandomClipByWeight(list);
                    PlayBGMWithFade(nextClip, 1.5f);

                    yield return new WaitForSeconds(2.0f);
                    isChangingTrack = false;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator PlayBGM_co(AudioSource fadeOutAus, AudioSource fadeInAus, float duration)
    {
        float startTime = Time.time;
        float startVol = (fadeOutAus != null) ? fadeOutAus.volume : 0f;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            if (fadeInAus != null) fadeInAus.volume = Mathf.Lerp(0f, volumeBGM, t);
            if (fadeOutAus != null) fadeOutAus.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }

        if (fadeInAus != null) fadeInAus.volume = volumeBGM;
        if (fadeOutAus != null) { fadeOutAus.volume = 0f; fadeOutAus.Stop(); }
    }

    IEnumerator Co_FadeOut(float duration)
    {
        if (currentAus == null) yield break;
        float startVol = currentAus.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            currentAus.volume = Mathf.Lerp(startVol, 0, t / duration);
            yield return null;
        }
        currentAus.volume = 0;
        currentAus.Stop();
    }

    // --- 기타 헬퍼 함수 ---

    AudioClip GetRandomClipByWeight(List<MyStruct2> list)
    {
        if (list == null || list.Count == 0) return null;
        if (list.Count == 1) return list[0].audioClip;

        var targetList = list.Where(x => x.audioClip != lastPlayedClip).ToList();
        if (targetList.Count == 0) targetList = list;

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