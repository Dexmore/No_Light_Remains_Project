using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MonsterSoundData", menuName = "GameData/MonsterSound")]
public class MonsterSoundData : ScriptableObject
{
    [System.Serializable]
    public struct SoundSettings // 클립별 설정을 담는 구조체
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume; // 개별 볼륨 조절 (0~1)
    }

    [System.Serializable]
    public struct SoundEntry
    {
        public string key;
        public SoundSettings[] settings; // 클립과 볼륨 세트의 배열
    }

    public List<SoundEntry> soundEntries = new List<SoundEntry>();

    // 랜덤 클립과 해당 볼륨을 함께 반환하기 위해 구조체 반환
    public SoundSettings GetRandomSettings(string key)
    {
        var entry = soundEntries.Find(e => e.key == key);
        if (entry.settings != null && entry.settings.Length > 0)
        {
            return entry.settings[Random.Range(0, entry.settings.Length)];
        }
        return default;
    }
}