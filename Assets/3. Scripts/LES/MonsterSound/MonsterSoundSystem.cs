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
        [Range(0f, 3f)] public float volume = 1f; 

        // ✅ 피치 적용 여부를 결정하는 bool 스위치 추가
        public bool usePitch = false; 
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
        _audioSource.spatialBlend = 0f; 
        _audioSource.playOnAwake = false;

        if (_playerTransform == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _playerTransform = playerObj.transform;
        }
    }

    public void PlaySound(string key)
    {
        if (_playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, _playerTransform.position);
        if (dist > soundRange) return;
        if (Time.time - _lastPlayTime < playInterval) return;

        SoundSetting setting = soundList.Find(s => s.key == key);
        if (setting == null || setting.clip == null) return;

        bool isRangePlay = (setting.endTime > 0 && setting.endTime > setting.startTime) || setting.startTime > 0;
        float finalVolume = Mathf.Clamp01(1f - (dist / soundRange)) * setting.volume;
        if (finalVolume <= 0.01f) return;

        // ✅ 피치 결정 로직: usePitch가 true일 때만 설정값 사용, 아니면 원본(1f)
        float targetPitch = setting.usePitch ? setting.pitch : 1f;

        if (isRangePlay)
        {
            _audioSource.clip = setting.clip;
            _audioSource.volume = finalVolume;
            _audioSource.pitch = targetPitch; // 적용
            _audioSource.time = setting.startTime;
            _audioSource.Play();

            if (setting.endTime > 0)
            {
                double duration = setting.endTime - setting.startTime;
                _audioSource.SetScheduledEndTime(AudioSettings.dspTime + duration);
            }
        }
        else
        {
            _audioSource.pitch = targetPitch; // 적용
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