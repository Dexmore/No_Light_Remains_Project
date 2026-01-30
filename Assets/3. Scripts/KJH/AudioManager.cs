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
    public void StopBGM(float duration = 0f) // int에서 float로 변경하여 정밀도 향상
    {
        // 기존 자동재생 및 페이드 코루틴 중단
        StopCoroutine(nameof(Co_AutoBGMNextTrack));
        StopCoroutine(nameof(PlayBGM_co));

        if (duration > 0)
        {
            // 페이드 아웃 코루틴 실행
            FadeOutBGM(duration);
        }
        else
        {
            // 즉시 정지
            StopCoroutine(nameof(Co_FadeOut));
            if (ausBGM0 != null) { ausBGM0.Stop(); ausBGM0.clip = null; }
            if (ausBGM1 != null) { ausBGM1.Stop(); ausBGM1.clip = null; }
            isChangingTrack = false;
            lastPlayedClip = null;
        }
    }
    public void PlayBGMWithFade(string bgmName, float duration = 1f, int loopCount = 1)
    {
        // 리스트에서 이름으로 클립 찾기
        int find = bgmList.FindIndex(x => x != null && x.name == bgmName);

        if (find == -1)
        {
            Debug.LogWarning($"BGM [{bgmName}]을 bgmList에서 찾을 수 없습니다.");
            return;
        }

        // 찾은 클립과 함께 loopCount를 넘겨서 실제 재생 함수 호출
        PlayBGMWithFade(bgmList[find], duration, loopCount);
    }

    // 2. 실제 재생 및 루프 처리를 담당하는 함수
    public void PlayBGMWithFade(AudioClip clip, float duration = 1f, int loopCount = 1)
    {
        if (clip == null) return;

        // 진행 중인 모든 BGM 관련 코루틴 중지
        StopCoroutine(nameof(PlayBGM_co));
        StopCoroutine(nameof(Co_AutoBGMNextTrack));
        StopCoroutine(nameof(Co_BGMWithLoopCount));

        AudioSource nextAus = (currentAus == ausBGM0) ? ausBGM1 : ausBGM0;
        nextAus.clip = clip;
        nextAus.volume = 0f;

        // 루프 설정: loopCount가 1보다 크면 우선 루프를 켭니다.
        // (끝나는 시점은 Co_BGMWithLoopCount에서 제어)
        nextAus.loop = (loopCount > 1);
        nextAus.Play();

        StartCoroutine(PlayBGM_co(currentAus, nextAus, duration));
        currentAus = nextAus;

        // n번 재생 후 AutoBGM으로 복귀시키는 코루틴 실행
        StartCoroutine(Co_BGMWithLoopCount(clip, loopCount));
    }

    // 3. 루프 횟수 감시 및 AutoBGM 복귀 코루틴
    IEnumerator Co_BGMWithLoopCount(AudioClip clip, int loopCount)
    {
        int currentLoop = 0;

        while (currentLoop < loopCount)
        {
            // 곡이 거의 끝날 때까지 대기 (끝나기 0.1초 전)
            yield return new WaitUntil(() => currentAus.clip == clip && currentAus.time >= clip.length - 0.1f);

            currentLoop++;

            // 마지막 루프 차례라면 루프 속성을 끄고 대기
            if (currentLoop >= loopCount)
            {
                currentAus.loop = false;
                // 완전히 끝날 때까지 대기
                yield return new WaitUntil(() => !currentAus.isPlaying);
                break;
            }

            // 다음 루프를 위해 잠깐 대기 (곡이 자연스럽게 다시 시작되도록)
            yield return new WaitForSeconds(0.2f);
        }

        // 모든 루프 완료 후 해당 씬의 기본 BGM(AutoBGM)으로 복귀
        Debug.Log($"[{clip.name}] {loopCount}회 재생 완료. 자동 BGM 모드로 복귀합니다.");
        StartAutoBGM();
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