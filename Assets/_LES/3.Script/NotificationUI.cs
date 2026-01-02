using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI; // [필수] LayoutRebuilder 사용을 위해 추가

[RequireComponent(typeof(CanvasGroup))]
public class NotificationUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private RectTransform backgroundRect; // [추가] 크기가 조절될 패널(자기 자신)

    [Header("설정")]
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (backgroundRect == null) backgroundRect = GetComponent<RectTransform>(); // 연결 안 했으면 자기 자신 가져오기

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        // 1. 오브젝트 켜기
        gameObject.SetActive(true);

        // 2. 텍스트 변경
        if (messageText != null) messageText.text = message;

        // 3. [핵심] 텍스트 길이에 맞춰 레이아웃 즉시 갱신
        // (이걸 안 하면 한 프레임 뒤에 크기가 바뀌어서 깜빡거릴 수 있음)
        if (backgroundRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRect);
        }

        // 4. 코루틴 시작 (기존 애니메이션 중단 후 재시작)
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
        gameObject.SetActive(false);
    }
}