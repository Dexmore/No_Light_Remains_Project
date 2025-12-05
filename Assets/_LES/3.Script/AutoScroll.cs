using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class AutoScroll : MonoBehaviour
{
    [Header("반응 속도 설정")]
    [Tooltip("낮을수록 빠릿하게(0.02), 높을수록 부드럽게(0.1) 따라갑니다.")]
    [SerializeField] private float smoothTime = 0.05f; 
    
    [Tooltip("스크롤 최고 속도")]
    [SerializeField] private float maxSpeed = 10000f; 

    [Tooltip("아이템과 화면 끝 사이의 여백")]
    [SerializeField] private float scrollMargin = 30f;

    private ScrollRect _scrollRect;
    private RectTransform _contentRect;
    private RectTransform _viewportRect; // [추가] 변수 선언 누락 수정
    
    private Vector2 _pixelVelocity; 
    private float _normalizedVelocity;

    public bool IsScrolledToTop 
    { 
        get 
        {
            if (_contentRect.rect.height <= _viewportRect.rect.height) return true;
            return _scrollRect.verticalNormalizedPosition >= 0.99f; 
        } 
    }

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _contentRect = _scrollRect.content;
        
        // [수정] viewport가 null이면 transform을 사용하도록 안전장치 추가
        _viewportRect = _scrollRect.viewport != null ? _scrollRect.viewport : GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return;
        
        if (!selected.transform.IsChildOf(_contentRect)) return;

        RectTransform selectedRect = selected.GetComponent<RectTransform>();
        int childIndex = selectedRect.GetSiblingIndex();

        // 1. 맨 첫 번째 아이템 -> 맨 위로 강제 이동
        if (childIndex == 0)
        {
            if (_scrollRect.verticalNormalizedPosition < 0.9999f)
            {
                float newPos = Mathf.SmoothDamp(_scrollRect.verticalNormalizedPosition, 1f, ref _normalizedVelocity, smoothTime, maxSpeed, Time.unscaledDeltaTime);
                _scrollRect.verticalNormalizedPosition = newPos;
            }
            _pixelVelocity = Vector2.zero;
            return;
        }
        // 2. 맨 마지막 아이템 -> 맨 아래로 강제 이동
        else if (childIndex == _contentRect.childCount - 1)
        {
            if (_scrollRect.verticalNormalizedPosition > 0.0001f)
            {
                float newPos = Mathf.SmoothDamp(_scrollRect.verticalNormalizedPosition, 0f, ref _normalizedVelocity, smoothTime, maxSpeed, Time.unscaledDeltaTime);
                _scrollRect.verticalNormalizedPosition = newPos;
            }
            _pixelVelocity = Vector2.zero;
            return;
        }

        _normalizedVelocity = 0f;
        UpdateScroll(selectedRect);
    }

    private void UpdateScroll(RectTransform target)
    {
        Vector3[] viewCorners = new Vector3[4];
        _viewportRect.GetWorldCorners(viewCorners); // _viewportRect 사용
        
        Vector3[] targetCorners = new Vector3[4];
        target.GetWorldCorners(targetCorners);

        float viewTop = viewCorners[1].y;
        float viewBottom = viewCorners[0].y;
        
        float targetTop = targetCorners[1].y;
        float targetBottom = targetCorners[0].y;

        float difference = 0f;

        if (targetTop > viewTop - scrollMargin)
        {
            difference = targetTop - (viewTop - scrollMargin);
        }
        else if (targetBottom < viewBottom + scrollMargin)
        {
            difference = targetBottom - (viewBottom + scrollMargin);
        }

        if (Mathf.Abs(difference) < 0.1f) return;

        float dynamicSmoothTime = smoothTime;
        if (Mathf.Abs(difference) > 100f) 
        {
            dynamicSmoothTime = smoothTime * 0.5f; 
        }

        Vector2 targetPos = _contentRect.anchoredPosition;
        targetPos.y -= difference;

        _contentRect.anchoredPosition = Vector2.SmoothDamp(
            _contentRect.anchoredPosition, 
            targetPos, 
            ref _pixelVelocity, 
            dynamicSmoothTime, 
            maxSpeed, 
            Time.unscaledDeltaTime
        );
    }
}