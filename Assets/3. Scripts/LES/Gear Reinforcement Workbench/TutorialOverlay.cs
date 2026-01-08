using UnityEngine;
using UnityEngine.UI;
using System.Collections; // 코루틴 사용을 위해 추가

public class TutorialOverlay : MonoBehaviour, ICanvasRaycastFilter
{
    [Header("패널 연결")]
    [SerializeField] private RectTransform topPanel;
    [SerializeField] private RectTransform bottomPanel;
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private RectTransform rightPanel;

    private RectTransform _currentTarget; 
    private RectTransform _myRect;      
    private Camera _uiCamera;
    
    // [추가] 투명도 조절을 위한 CanvasGroup
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _myRect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>(); // 컴포넌트 가져오기
        
        // 만약 없다면 자동으로 추가 (안전장치)
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            _uiCamera = canvas.worldCamera;
    }

    // [수정] FocusOn 함수: 페이드 효과와 별개로 구멍 위치만 갱신
    public void FocusOn(RectTransform target)
    {
        // 켜는 건 FadeIn에서 할 것이므로 여기서는 생략 가능하지만, 
        // 혹시 모르니 켜둡니다. (투명도가 0이면 안 보일 뿐)
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        
        _currentTarget = target;
        RefreshMask();
    }

    // [신규] 페이드 인 (서서히 나타나기)
    public void PlayFadeIn(float duration)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeRoutine(0f, 1f, duration));
    }

    // [신규] 페이드 아웃 (서서히 사라지기)
    public void PlayFadeOut(float duration)
    {
        StartCoroutine(FadeRoutine(1f, 0f, duration, () => 
        {
            gameObject.SetActive(false); // 다 사라지면 끄기
            _currentTarget = null;
        }));
    }

    private IEnumerator FadeRoutine(float start, float end, float duration, System.Action onComplete = null)
    {
        float elapsed = 0f;
        _canvasGroup.alpha = start;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = end;
        onComplete?.Invoke();
    }
    
    // ... (Hide, LateUpdate, RefreshMask, IsRaycastLocationValid 등 기존 코드 그대로 유지) ...
    // 단, 기존 Hide() 함수는 이제 PlayFadeOut을 쓸 거라 잘 안 쓰겠지만 남겨둬도 됩니다.
    public new void Hide() // new 키워드는 경고 방지용 (없어도 됨)
    {
        gameObject.SetActive(false);
        _currentTarget = null;
    }

    // ... (아래 RefreshMask, WorldToLocalPoint, IsRaycastLocationValid 등은 기존 코드 그대로 사용)
    private void LateUpdate()
    {
        if (_currentTarget != null) RefreshMask();
    }

    private void RefreshMask()
    {
        if (_currentTarget == null) return;

        Vector3[] worldCorners = new Vector3[4];
        _currentTarget.GetWorldCorners(worldCorners);

        Vector2 bottomLeft = WorldToLocalPoint(worldCorners[0]);
        Vector2 topRight = WorldToLocalPoint(worldCorners[2]);

        float minY = bottomLeft.y;
        float maxY = topRight.y;
        float minX = bottomLeft.x;
        float maxX = topRight.x;

        float canvasWidth = _myRect.rect.width;
        float canvasHeight = _myRect.rect.height;

        topPanel.sizeDelta = new Vector2(canvasWidth, canvasHeight / 2 - maxY);
        topPanel.anchoredPosition = new Vector2(0, maxY + topPanel.sizeDelta.y / 2);

        bottomPanel.sizeDelta = new Vector2(canvasWidth, minY - (-canvasHeight / 2));
        bottomPanel.anchoredPosition = new Vector2(0, minY - bottomPanel.sizeDelta.y / 2);

        leftPanel.sizeDelta = new Vector2(minX - (-canvasWidth / 2), maxY - minY);
        leftPanel.anchoredPosition = new Vector2(minX - leftPanel.sizeDelta.x / 2, (minY + maxY) / 2);

        rightPanel.sizeDelta = new Vector2(canvasWidth / 2 - maxX, maxY - minY);
        rightPanel.anchoredPosition = new Vector2(maxX + rightPanel.sizeDelta.x / 2, (minY + maxY) / 2);
    }

    private Vector2 WorldToLocalPoint(Vector3 worldPoint)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _myRect, 
            RectTransformUtility.WorldToScreenPoint(_uiCamera, worldPoint), 
            _uiCamera, 
            out localPoint
        );
        return localPoint;
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (_currentTarget == null || !gameObject.activeSelf) return true; 

        bool isInside = RectTransformUtility.RectangleContainsScreenPoint(_currentTarget, sp, eventCamera);
        return !isInside; 
    }
}