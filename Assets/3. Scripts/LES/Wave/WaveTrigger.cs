using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))] // 실수 방지: 콜라이더 자동 추가
public class WaveTrigger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("웨이브를 실행할 매니저를 연결하세요.")]
    public WaveManager targetWaveManager;

    [Tooltip("이 태그를 가진 오브젝트(플레이어)가 닿아야 발동합니다.")]
    public string targetTag = "Player";

    private void Awake()
    {
        // 실수로 IsTrigger를 안 켰을 경우를 대비해 코드로 강제 설정
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 플레이어인지 확인
        if (collision.CompareTag(targetTag))
        {
            // 2. 매니저가 연결되어 있는지 확인
            if (targetWaveManager != null)
            {
                Debug.Log("⚠️ Player Entered Trigger Area! Starting Battle...");
                targetWaveManager.StartBattle((Vector2)transform.position);

                // 3. [핵심] 1회성 보장: 발동 즉시 이 트리거 오브젝트를 삭제
                // (Setactive(false)보다 Destroy가 확실하게 메모리에서 날려버림)
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("WaveTrigger에 WaveManager가 연결되지 않았습니다!");
            }
        }
    }
    
    // 에디터에서 트리거 구역을 눈으로 쉽게 보기 위한 기즈모
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 붉은색 반투명
        // 현재 오브젝트의 콜라이더 크기에 맞춰 그림
        var col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.offset, col.size);
        }
    }
}