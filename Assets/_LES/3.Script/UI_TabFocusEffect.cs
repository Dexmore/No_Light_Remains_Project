using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// 탭 버튼(Button) 오브젝트에 추가하세요.
public class UI_TabFocusEffect : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("텍스트 색상 설정")]
    [SerializeField] private TextMeshProUGUI tabText; // 탭 이름 텍스트
    [SerializeField] private Color normalColor = new Color(0.5f, 0.5f, 0.5f, 1f); // 평소(비활성): 회색
    [SerializeField] private Color focusedColor = Color.white; // 선택됨(활성): 흰색
    [SerializeField] private float fadeSpeed = 10f;

    [Header("크기 효과 (선택사항)")]
    [SerializeField] private bool useScaleEffect = false;
    [SerializeField] private float focusedScale = 1.1f;

    private Color _targetColor;
    private Vector3 _targetScale;
    private Vector3 _originalScale;

    private void Awake()
    {
        if (tabText == null) tabText = GetComponentInChildren<TextMeshProUGUI>();
        
        _targetColor = normalColor;
        _originalScale = transform.localScale;
        _targetScale = _originalScale;
        
        // 시작 시 색상 적용
        if (tabText != null) tabText.color = normalColor;
    }

    private void Update()
    {
        if (tabText != null)
        {
            tabText.color = Color.Lerp(tabText.color, _targetColor, Time.unscaledDeltaTime * fadeSpeed);
        }
        
        if (useScaleEffect)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * fadeSpeed);
        }
    }

    // 1. 키보드로 탭 선택 시 (또는 마우스 오버) -> 밝게!
    public void OnSelect(BaseEventData eventData)
    {
        _targetColor = focusedColor;
        if (useScaleEffect) _targetScale = _originalScale * focusedScale;
    }

    // 2. 키보드로 아래로 내려가거나 다른 탭으로 갈 시 -> 어둡게!
    public void OnDeselect(BaseEventData eventData)
    {
        _targetColor = normalColor;
        if (useScaleEffect) _targetScale = _originalScale;
    }
    
    // 마우스 호환용
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스 올리면 선택된 것처럼 밝게
        _targetColor = focusedColor;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        // 선택된 상태(EventSystem)가 아니라면 다시 어둡게
        if (EventSystem.current.currentSelectedGameObject != gameObject)
        {
            _targetColor = normalColor;
        }
    }
}