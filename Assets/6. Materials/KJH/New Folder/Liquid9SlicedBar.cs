using UnityEngine;
using UnityEngine.UI;

public class Liquid9SlicedBar : MonoBehaviour, IMaterialModifier
{
    // --- 셰이더로 전달할 값 ---
    [Range(0f, 1f)] [SerializeField] private float _value = 0.5f;

    // --- 셰이더 속성 ID 캐시 ---
    // (LiquidValue는 필요할 경우에만 사용되지만, 구조를 위해 유지합니다.)
    private const string LiquidValuePropertyName = "_LiquidValue";
    private static readonly int LiquidValuePropertyId = Shader.PropertyToID(LiquidValuePropertyName);

    private const string CenterXYRatioPropertyName = "_CenterXYRatio";
    private static readonly int CenterXYRatioPropertyId = Shader.PropertyToID(CenterXYRatioPropertyName);

    // --- 컴포넌트 및 Material 인스턴스 ---
    private RectTransform _rectTransform;
    private Image _image;
    private Material _modifiedMaterial;

    // 셰이더로 전달할 9-Slice 늘어난 비율 (RatioX, RatioY)
    private float _centerXYRatio = 1f;

    // --- 값 설정 (외부 접근 및 UI 업데이트 요청) ---
    public float Value
    {
        get => _value;
        set
        {
            _value = Mathf.Clamp01(value);
            // Material 값 변경을 위해 SetMaterialDirty()를 호출합니다.
            _image?.SetMaterialDirty();
        }
    }

    // --- Unity 라이프사이클 (시작 시 1회 계산) ---

    protected void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();

        if (_image == null || _rectTransform == null || _image.sprite == null || _image.type != Image.Type.Sliced)
        {
            Debug.LogError("Liquid9SlicedBar requires an Image component set to Sliced type.", this);
            enabled = false;
            return;
        }

        // 게임 시작 시 비율을 한 번 계산합니다.
        CalculateCenterRatio();

        // GetModifiedMaterial이 Material을 초기화하고 값을 설정하도록 유도합니다.
        _image.SetMaterialDirty();
    }

    // OnValidate는 Editor에서만 사용되며, 런타임과는 무관합니다.
    protected void OnValidate()
    {
        // OnValidate 시점에는 _image가 null일 수 있으므로 강제로 가져옵니다.
        // 만약 _image가 아직 붙어있지 않거나, Scene에 없을 경우 null이 됩니다.
        _image = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();

        // 컴포넌트가 모두 존재하고, 플레이 모드가 아닐 때만 비율을 계산합니다.
        if (!Application.isPlaying && _image != null && _image.sprite != null)
        {
            CalculateCenterRatio();
        }
        else if (Application.isPlaying)
        {
            // 런타임 시에는 Awake에서 이미 _image가 캐시되었을 것입니다.
            CalculateCenterRatio();
        }

        // 이 줄은 _image가 null이면 실행되지 않아야 안전합니다.
        if (_image != null)
        {
            Value = _value; // Value Setter를 통해 SetMaterialDirty() 호출
        }
    }


    // --- 9-Slice 늘어난 비율 계산 로직 (Awake에서 한 번만 호출) ---

    private void CalculateCenterRatio()
    {
        // 안전 장치 강화: 필요한 컴포넌트나 스프라이트가 null인지 다시 확인합니다.
        if (_image == null || _image.sprite == null || _rectTransform == null)
        {
            // Debug.LogWarning("CalculateCenterRatio: Required components or sprite are missing.");
            _centerXYRatio = 1f; // 안전 값 설정
            return;
        }

        Vector4 border = _image.sprite.border;
        float ppu = _image.sprite.pixelsPerUnit;

        // 1. 보더 크기 (Unity 단위)
        float borderX = (border.x + border.z) / ppu;
        float borderY = (border.y + border.w) / ppu;

        // 2. RectTransform의 현재 전체 크기 (Awake 시점의 크기)
        float currentWidth = _rectTransform.rect.width;
        float currentHeight = _rectTransform.rect.height;

        // 3. 중앙 영역이 늘어난 후의 실제 크기
        float stretchedWidth = currentWidth - borderX;
        float stretchedHeight = currentHeight - borderY;

        // 4. 중앙 영역의 원본 크기 (Unity 단위)
        float originalCenterX = (_image.sprite.rect.width - border.x - border.z) / ppu;
        float originalCenterY = (_image.sprite.rect.height - border.y - border.w) / ppu;

        // 5. 늘어난 비율 (현재 크기 / 원본 크기)
        if (originalCenterX <= 0 || originalCenterY <= 0)
        {
            _centerXYRatio = 1f;
            return;
        }

        float ratioX = stretchedWidth / originalCenterX;
        float ratioY = stretchedHeight / originalCenterY;

        // 셰이더에 전달할 비율 설정
        _centerXYRatio = ratioX/ratioY;
        //Debug.Log(_centerXYRatio);
    }

    // --- IMaterialModifier 구현 (핵심) ---
    public Material GetModifiedMaterial(Material baseMaterial)
    {
        // 1. Material 복사본 재활용 또는 생성
        if (_modifiedMaterial == null)
        {
            // 인스턴스가 없으면 원본 Material을 복사하여 생성
            _modifiedMaterial = new Material(baseMaterial);
        }

        // 2. 값 설정
        // _value와 _centerXYRatio는 이미 Awake에서 최종 값이 결정되어 저장되어 있습니다.
        if (_modifiedMaterial.HasFloat(LiquidValuePropertyId))
            _modifiedMaterial.SetFloat(LiquidValuePropertyId, _value);

        if (_modifiedMaterial.HasFloat(CenterXYRatioPropertyId))
            _modifiedMaterial.SetFloat(CenterXYRatioPropertyId, _centerXYRatio);

        // 3. 수정된 Material 반환
        return _modifiedMaterial;
    }

    // 오브젝트 파괴 시 Material 인스턴스 제거 (메모리 누수 방지)
    protected void OnDestroy()
    {
        if (_modifiedMaterial != null)
        {
            Destroy(_modifiedMaterial);
            _modifiedMaterial = null;
        }
    }
}