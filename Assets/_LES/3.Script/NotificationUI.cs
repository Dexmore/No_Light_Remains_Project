using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class NotificationUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private RectTransform backgroundRect;

    [Header("설정")]
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (backgroundRect == null) backgroundRect = GetComponent<RectTransform>();

        // 시작 시 투명하게 만들어서 안 보이게 함
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        
        // [삭제됨] gameObject.SetActive(false); <-- 이 줄 때문에 켜지자마자 꺼져서 오류가 났던 것입니다.
    }

    public void ShowMessage(string message)
    {
        // 1. 오브젝트 켜기
        gameObject.SetActive(true);

        // 2. 텍스트 변경
        if (messageText != null) messageText.text = message;

        // 3. 배경 크기 즉시 갱신
        if (backgroundRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRect);
        }

        // 4. 코루틴 시작
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(ProcessNotification());
    }

    private IEnumerator ProcessNotification()
    {
        _canvasGroup.alpha = 1f; // 즉시 보임

        yield return new WaitForSeconds(displayDuration);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false); // 다 끝나면 끄기
    }
}