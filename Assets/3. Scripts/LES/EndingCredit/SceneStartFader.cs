using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 로비 씬의 검은 패널에 붙이세요.
public class SceneStartFader : MonoBehaviour
{
    [SerializeField] private float startDelay = 1.0f; // [신규] 씬 로드 후 1초 동안은 계속 검은 화면 유지
    [SerializeField] private float fadeDuration = 4.0f; // 4초 동안 천천히 밝아짐
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // [중요] 시작하자마자 무조건 검은색
        _canvasGroup.alpha = 1f; 
        _canvasGroup.blocksRaycasts = true;
    }

    private void Start()
    {
        // OnEnable 대신 Start에서 확실하게 코루틴 시작
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        // [중요] 로비가 완전히 로드되고 안정될 때까지 대기
        yield return new WaitForSeconds(startDelay);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            
            // 곡선 이동 (천천히 시작했다가 빨라지거나, 부드럽게)
            float t = elapsed / fadeDuration;
            // SmoothStep: 끝부분이 부드러움
            float alpha = Mathf.Lerp(1f, 0f, t * t * (3f - 2f * t));
            
            _canvasGroup.alpha = alpha;
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false; 
        
        gameObject.SetActive(false); 
    }
}