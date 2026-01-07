using UnityEngine;
using System.Collections;

public class AfterImageEffect : MonoBehaviour
{
    private SpriteRenderer[] childSRs;
    private int currentIndex = 0;

    [Header("Settings")]
    public float lifeTime = 0.5f;      // 잔상이 머무는 시간
    public Color ghostColor = new Color(0f, 0.8f, 1f, 1f); // 팁 1: 약간 푸른빛

    void Awake()
    {
        // 프리팹 내 11개의 자식 SpriteRenderer를 미리 가져옴
        childSRs = GetComponentsInChildren<SpriteRenderer>(true);

        // 초기 상태에서는 모두 꺼둠
        foreach (var sr in childSRs) sr.gameObject.SetActive(false);
    }

    public void Init(SpriteRenderer targetSR, float duration, float fps)
    {
        StartCoroutine(ExecuteEffect(targetSR, duration, fps));
    }

    IEnumerator ExecuteEffect(SpriteRenderer targetSR, float duration, float fps)
    {
        float interval = 1f / fps;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 타겟이 중간에 사라질 경우를 대비한 예외 처리
            if (targetSR == null) break;

            var sr = childSRs[currentIndex];

            // 위치, 회전, 스케일 및 스프라이트 복사
            sr.transform.position = targetSR.transform.position;
            sr.transform.rotation = targetSR.transform.rotation;
            sr.transform.localScale = targetSR.transform.lossyScale;
            sr.sprite = targetSR.sprite;
            sr.flipX = targetSR.flipX;

            // 팁 2: Z-Fighting 방지 (순번에 따라 레이어 순서를 미세하게 낮춤)
            // 가장 최근 잔상이 가장 위에 오도록 (원본 - 1 - 인덱스)
            sr.sortingOrder = targetSR.sortingOrder - 1 - currentIndex;

            // 잔상 연출 코루틴 실행
            StartCoroutine(FadeOut(sr));

            // 인덱스 순환 (0~10)
            currentIndex = (currentIndex + 1) % childSRs.Length;

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // 모든 효과 종료 후 프리팹 파괴 (가장 마지막 잔상의 lifeTime을 고려해 1초 뒤)
        Destroy(gameObject, 1f);
    }

    IEnumerator FadeOut(SpriteRenderer sr)
    {
        sr.gameObject.SetActive(true);
        float current = 0;
        Color originColor = sr.color;
        while (current < lifeTime)
        {
            current += Time.deltaTime;
            float progress = current / lifeTime;

            // 팁 1: 지정된 색상에서 투명해지도록 설정
            Color c = Color.Lerp(originColor, ghostColor, 0.3f);
            c.a = Mathf.Lerp(1f, 0f, progress);
            sr.color = c;

            yield return null;
        }

        sr.gameObject.SetActive(false);
    }
}