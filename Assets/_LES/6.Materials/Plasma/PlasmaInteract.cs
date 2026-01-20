using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // [필수] New Input System 사용

[RequireComponent(typeof(Image))]
public class PlasmaInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler
{
    private Material _material;
    private RectTransform _rectTransform;
    private bool _isHovering = false;

    void Awake()
    {
        Image img = GetComponent<Image>();
        // 재질 복사 (인스턴싱) - 다른 슬롯과 색상이 겹치지 않게 함
        _material = Instantiate(img.material);
        img.material = _material;
        _rectTransform = GetComponent<RectTransform>();
    }

    public void SetThemeColor(Color core, Color glow)
    {
        if (_material == null)
        {
            Image img = GetComponent<Image>();
            _material = Instantiate(img.material);
            img.material = _material;
        }

        _material.SetColor("_CoreColor", core);
        _material.SetColor("_GlowColor", glow);
    }

    void Update()
    {
        if (_isHovering)
        {
            Vector2 localPoint;
            
            // [수정] New Input System 방식으로 마우스 좌표 가져오기
            Vector2 mousePos = Vector2.zero;
            if (Mouse.current != null)
            {
                mousePos = Mouse.current.position.ReadValue();
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform, 
                mousePos, // 수정된 좌표 사용
                null, 
                out localPoint
            );

            float u = (localPoint.x / _rectTransform.rect.width) + 0.5f;
            float v = (localPoint.y / _rectTransform.rect.height) + 0.5f;

            _material.SetVector("_TargetPos", new Vector4(u, v, 0, 0));
            _material.SetFloat("_InteractPower", 1.0f);
        }
        else
        {
            _material.SetFloat("_InteractPower", Mathf.Lerp(_material.GetFloat("_InteractPower"), 0.0f, Time.deltaTime * 5.0f));
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => _isHovering = true;
    public void OnPointerExit(PointerEventData eventData) => _isHovering = false;
    public void OnDrag(PointerEventData eventData) { }
}