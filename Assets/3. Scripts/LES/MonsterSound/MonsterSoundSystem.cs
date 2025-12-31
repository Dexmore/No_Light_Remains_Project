using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MonsterSoundSystem : MonoBehaviour
{
    [System.Serializable]
    public class SoundSetting
    {
        public string key;             // 애니메이션 이벤트 키 (예: Attack)
        public AudioClip clip;         // 오디오 파일
        [Range(0f, 1f)] public float volume = 1f; 
        [Range(0.1f, 3f)] public float pitch = 1f;

        [Header("재생 범위 설정 (초 단위)")]
        [Tooltip("0이면 처음부터 재생")]
        public float startTime = 0f;   
        [Tooltip("0이면 끝까지 재생")]
        public float endTime = 0f;     
    }

    [Header("사운드 리스트 설정")]
    [SerializeField] private List<SoundSetting> soundList = new List<SoundSetting>();
    
    [Header("거리 및 쿨타임 옵션")]
    [SerializeField] private float soundRange = 15f;
    [SerializeField] private float playInterval = 0.1f;
    [SerializeField] private bool showDebugRange = true;

    private AudioSource _audioSource;
    private static Transform _playerTransform;
    private float _lastPlayTime;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        
        // 2D 게임 최적화 설정
        _audioSource.spatialBlend = 0f; 
        _audioSource.playOnAwake = false;

        if (_playerTransform == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _playerTransform = playerObj.transform;
        }
    }

    // 애니메이션 이벤트에서 호출할 함수
    public void PlaySound(string key)
    {
        if (_playerTransform == null) return;

        // 1. 거리 및 쿨타임 체크
        float dist = Vector2.Distance(transform.position, _playerTransform.position);
        if (dist > soundRange) return;
        if (Time.time - _lastPlayTime < playInterval) return;

        // 2. 키에 맞는 설정 찾기
        SoundSetting setting = soundList.Find(s => s.key == key);
        if (setting == null || setting.clip == null) return;

        // 3. 재생 로직 분기 (구간 재생 vs 일반 재생)
        // 끝나는 시간(endTime)이 설정되어 있거나, 시작 시간(startTime)이 0보다 크면 '구간 재생' 모드
        bool isRangePlay = (setting.endTime > 0 && setting.endTime > setting.startTime) || setting.startTime > 0;

        // 거리 비례 볼륨 계산
        float finalVolume = Mathf.Clamp01(1f - (dist / soundRange)) * setting.volume;
        if (finalVolume <= 0.01f) return;

        if (isRangePlay)
        {
            // [구간 재생 모드]
            // 주의: PlayOneShot은 시작/종료 지점 설정이 불가능하므로 일반 Play()를 써야 함.
            // 단점: 같은 AudioSource에서 재생 중이던 다른 소리가 끊길 수 있음.
            _audioSource.clip = setting.clip;
            _audioSource.volume = finalVolume;
            _audioSource.pitch = setting.pitch;
            _audioSource.time = setting.startTime; // 시작 지점으로 점프
            _audioSource.Play();

            if (setting.endTime > 0)
            {
                // 지정된 시간만큼 재생 후 멈추도록 예약 (정밀 타이밍)
                double duration = setting.endTime - setting.startTime;
                _audioSource.SetScheduledEndTime(AudioSettings.dspTime + duration);
            }
        }
        else
        {
            // [일반 재생 모드] 
            // PlayOneShot 사용 -> 소리가 겹쳐도 자연스럽게 들림
            _audioSource.pitch = setting.pitch;
            _audioSource.PlayOneShot(setting.clip, finalVolume);
        }

        _lastPlayTime = Time.time;
    }

    private void OnDrawGizmosSelected()
    {
        if (showDebugRange)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, soundRange);
        }
    }
}