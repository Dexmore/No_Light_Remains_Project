using UnityEngine;
using System.Collections.Generic;

// [유지] 평소에는 몬스터 본체의 스피커를 사용하므로 컴포넌트 필요
[RequireComponent(typeof(AudioSource))]
public class MonsterSoundSystem : MonoBehaviour
{
    [System.Serializable]
    public class SoundSetting
    {
        public string key;
        public AudioClip clip;
        [Range(0f, 3f)] public float volume = 1f; 
        public bool usePitch = false; 
        [Range(0.1f, 3f)] public float pitch = 1f;
        
        [Header("옵션")]
        [Tooltip("체크하면 몬스터가 죽어도 소리가 끊기지 않습니다. (죽음 소리 전용)")]
        public bool playOnManager = false; // ✅ 새로 추가된 기능 (기존 데이터 안 날아감)

        [Header("재생 범위 (0이면 전체)")]
        public float startTime = 0f;   
        public float endTime = 0f;     
    }

    [SerializeField] private List<SoundSetting> soundList = new List<SoundSetting>();
    
    // 성능 최적화용 딕셔너리
    private Dictionary<string, SoundSetting> _soundDict = new Dictionary<string, SoundSetting>();

    [Header("거리 및 쿨타임")]
    [SerializeField] private float soundRange = 15f;
    [SerializeField] private float playInterval = 0.1f;
    [SerializeField] private bool showDebugRange = true;

    private AudioSource _audioSource;
    private static Transform _playerTransform;
    private float _lastPlayTime;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1f; // 3D 사운드 유지 (현실감)
        _audioSource.playOnAwake = false;

        foreach (var setting in soundList)
        {
            if (!_soundDict.ContainsKey(setting.key))
                _soundDict.Add(setting.key, setting);
        }
    }

    public void PlaySound(string key)
    {
        // 1. 플레이어 거리 체크 로직 (기존 동일)
        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;
            else return;
        }

        float dist = Vector2.Distance(transform.position, _playerTransform.position);
        if (dist > soundRange) return;
        if (Time.time - _lastPlayTime < playInterval) return;

        // 2. 데이터 가져오기
        if (!_soundDict.TryGetValue(key, out SoundSetting setting) || setting.clip == null) return;

        // 3. 볼륨 및 피치 계산
        float finalVolume = Mathf.Clamp01(1f - (dist / soundRange)) * setting.volume;
        if (finalVolume <= 0.01f) return;

        float finalPitch = setting.usePitch ? setting.pitch : 1f;
        bool isRangePlay = (setting.endTime > 0 && setting.endTime > setting.startTime) || setting.startTime > 0;

        // =================================================================
        // [핵심] 하이브리드 분기 처리
        // =================================================================
        if (setting.playOnManager) 
        {
            // A. [매니저 위임] 죽는 소리 등 (체크박스 켠 경우)
            // 몬스터가 파괴되어도 매니저가 대신 소리를 내줌
            if (AudioManager.I != null)
            {
                AudioManager.I.PlayDirectSFX(
                    setting.clip, 
                    transform.position, 
                    finalVolume, 
                    finalPitch, 
                    setting.startTime, 
                    setting.endTime
                );
            }
        }
        else 
        {
            // B. [로컬 재생] 걷기, 공격 등 (체크박스 끈 경우)
            // 몬스터 몸체에서 소리가 나므로 이동할 때 자연스럽게 따라다님 (현실감 Up)
            if (isRangePlay)
            {
                _audioSource.clip = setting.clip;
                _audioSource.volume = finalVolume;
                _audioSource.pitch = finalPitch;
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
                _audioSource.pitch = finalPitch;
                _audioSource.PlayOneShot(setting.clip, finalVolume);
            }
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