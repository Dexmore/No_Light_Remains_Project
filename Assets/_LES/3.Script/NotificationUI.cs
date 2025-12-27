using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class NotificationUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("설정")]
    [SerializeField] private float displayDuration = 1.5f; // 메시지가 떠 있는 시간
    [SerializeField] private float fadeDuration = 0.5f;    // 사라지는 데 걸리는 시간

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        
        // 시작 시 안 보이게 설정
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    // 외부에서 이 함수를 호출하여 메시지를 띄웁니다.
    public void ShowMessage(string message)
    {
        // [핵심 수정] 코루틴을 시작하려면 오브젝트가 켜져 있어야 합니다.
        // 따라서 여기서 먼저 켜줍니다.
        gameObject.SetActive(true); 

        // 메시지 설정
        if (messageText != null) messageText.text = message;

        // 기존에 실행 중이던 페이드 아웃이 있다면 멈춤
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        // 이제 안전하게 코루틴 시작
        _fadeCoroutine = StartCoroutine(ProcessNotification());
    }

    private IEnumerator ProcessNotification()
    {
        // (여기 있던 gameObject.SetActive(true)는 위로 옮겨졌습니다)
        _canvasGroup.alpha = 1f; // 즉시 보이게

        // 대기 (떠 있는 시간)
        yield return new WaitForSeconds(displayDuration);

        // 페이드 아웃 (서서히 사라짐)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        // 완전히 끄기
        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}