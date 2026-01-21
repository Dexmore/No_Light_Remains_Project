using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 아이템과 기록물 슬롯에 붙여주세요.
public class UI_FocusScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("크기 설정")]
    [SerializeField] private float defaultScale = 1.0f;
    [SerializeField] private float focusedScale = 1.15f; // 1.15배 커짐
    [SerializeField] private float animSpeed = 10f;

    private Vector3 _targetScale;
    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _targetScale = Vector3.one * defaultScale;
    }

    private void Update()
    {
        // 부드럽게 크기 변화 (Lerp)
        _rect.localScale = Vector3.Lerp(_rect.localScale, _targetScale, Time.unscaledDeltaTime * animSpeed);
    }
    
    // 마우스 올렸을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetScale = Vector3.one * focusedScale;
        // (선택 사항) 커질 때 앞으로 튀어나오게 해서 다른 슬롯에 가려지지 않게 함
        // transform.SetAsLastSibling(); 
    }

    // 마우스 나갔을 때 (단, 선택된 상태면 줄어들지 않음)
    public void OnPointerExit(PointerEventData eventData)
    {
        // 현재 내가 '선택(Focus)'된 상태가 아니라면 줄어듦
        if (EventSystem.current.currentSelectedGameObject != gameObject)
        {
            _targetScale = Vector3.one * defaultScale;
        }
    }

    // 키보드/패드로 선택했을 때
    public void OnSelect(BaseEventData eventData)
    {
        _targetScale = Vector3.one * focusedScale;
    }

    // 선택 해제됐을 때
    public void OnDeselect(BaseEventData eventData)
    {
        _targetScale = Vector3.one * defaultScale;
    }
    
    // (선택 사항) 비활성화 될 때 크기 초기화
    private void OnDisable()
    {
        _rect.localScale = Vector3.one * defaultScale;
    }
}