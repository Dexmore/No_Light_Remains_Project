using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UI_OutlineEffect : MonoBehaviour
{
    [Header("깜빡임 설정")]
    [Tooltip("깜빡이는 속도 (높을수록 빠름)")]
    [SerializeField] private float blinkSpeed = 4f;

    [Tooltip("최소 투명도 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.2f;

    [Tooltip("최대 투명도 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 1.0f;

    private Image _outlineImage;
    private Color _originalColor;

    private void Awake()
    {
        _outlineImage = GetComponent<Image>();
        _originalColor = _outlineImage.color;
    }

    // 오브젝트가 켜질 때(SetActive true)마다 실행됨
    private void OnEnable()
    {
        if (_outlineImage != null)
        {
            // 켜지는 순간 가장 밝게 시작
            Color c = _originalColor;
            c.a = maxAlpha;
            _outlineImage.color = c;
        }
    }

    private void Update()
    {
        if (_outlineImage == null) return;

        // 투명도가 부드럽게 왔다갔다 (PingPong)
        // unscaledTime을 사용하여 게임 일시정지 중에도 깜빡임 유지 가능
        float alpha = Mathf.PingPong(Time.unscaledTime * blinkSpeed, maxAlpha - minAlpha) + minAlpha;

        Color newColor = _originalColor;
        newColor.a = alpha;
        _outlineImage.color = newColor;
    }
}