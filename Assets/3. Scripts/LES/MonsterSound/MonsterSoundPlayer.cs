using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MonsterSoundPlayer : MonoBehaviour
{
    [Header("데이터 및 설정")]
    [SerializeField] private MonsterSoundData soundData;
    [SerializeField] private float soundRange = 15f;
    [SerializeField] private float playInterval = 0.1f;

    [Header("디버그 옵션")]
    [SerializeField] private bool showSoundRangeDebug = true;

    private AudioSource _audioSource;
    private static Transform _playerTransform; 
    private float _lastPlayTime;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;
        }

        // 기본 오디오 소스 설정
        _audioSource.spatialBlend = 0f; // 볼륨 계산을 코드로 직접 하므로 0(2D) 권장
    }

    public void PlaySound(string eventKey)
    {
        if (soundData == null || _playerTransform == null) return;

        // 1. 거리 및 쿨타임 체크
        float distance = Vector2.Distance(transform.position, _playerTransform.position);
        if (distance > soundRange || Time.time - _lastPlayTime < playInterval) return;

        // 2. 데이터 가져오기
        var settings = soundData.GetRandomSettings(eventKey);
        if (settings.clip == null) return;

        // 3. 최종 볼륨 계산
        // (거리 감쇠 비율) * (클립 자체 설정 볼륨)
        float distanceVolume = Mathf.Clamp01(1f - (distance / soundRange));
        float finalVolume = distanceVolume * settings.volume;

        if (finalVolume > 0.01f) // 소리가 너무 작으면 재생 안 함
        {
            _audioSource.PlayOneShot(settings.clip, finalVolume);
            _lastPlayTime = Time.time;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showSoundRangeDebug || soundRange <= 0) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, soundRange);
    }
}