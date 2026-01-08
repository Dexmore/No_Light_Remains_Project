using UnityEngine;
using UnityEngine.UI;

public class SystemGaugeBar : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private Image targetImage;

    [Header("기본 움직임")]
    [SerializeField] private float noiseSpeed = 2.0f;   // 펄린 노이즈 변화 속도
    [SerializeField] private float smoothSpeed = 5.0f;  // [신규] 값이 변할 때 따라가는 속도 (낮을수록 부드러움)

    [Header("글리치(오류) 연출")]
    [SerializeField] private bool useGlitch = true;
    [SerializeField] [Range(0, 100)] private int glitchChance = 5; // 발생 확률
    [SerializeField] [Range(0.0f, 1.0f)] private float glitchIntensity = 0.2f; // [신규] 튀는 정도 (0.1 = 살짝 떨림, 1.0 = 미친듯이 튐)

    [Header("색상 연출")]
    [SerializeField] private bool useColorChange = true;
    [SerializeField] private Color normalColor = new Color(0, 1, 0, 0.5f);
    [SerializeField] private Color highLoadColor = new Color(1, 0, 0, 0.8f);
    [SerializeField] private float colorSmoothSpeed = 3.0f; // [신규] 색상 변화 속도

    private float _seed;
    private float _targetValue; // 목표치 (여기로 이동하려고 노력함)

    private void Start()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        _seed = Random.Range(0f, 100f);
        
        if (targetImage != null && targetImage.type != Image.Type.Filled)
        {
            Debug.LogWarning("SystemGaugeBar: 이미지를 Filled 타입으로 바꿔주세요.");
        }
    }

    private void Update()
    {
        if (targetImage == null) return;

        // 1. 목표값 계산 (펄린 노이즈)
        // 노이즈가 너무 0이나 1에 몰리지 않게 0.2~0.8 범위로 살짝 보정해주면 더 자연스럽습니다.
        float rawNoise = Mathf.PerlinNoise(_seed + Time.time * noiseSpeed, 0f);
        _targetValue = rawNoise; 

        // 2. 글리치 적용 (현재 값에서 +- intensity 만큼만 튐)
        if (useGlitch && Random.Range(0, 100) < glitchChance)
        {
            // [수정] 완전 랜덤이 아니라, 현재 값 기준에서 살짝 흔들기
            float randomJump = Random.Range(-glitchIntensity, glitchIntensity);
            _targetValue = Mathf.Clamp01(_targetValue + randomJump);
        }

        // 3. [핵심] 부드럽게 이동 (Lerp)
        // 현재 fillAmount에서 목표값(_targetValue)으로 smoothSpeed만큼 서서히 이동
        targetImage.fillAmount = Mathf.Lerp(targetImage.fillAmount, _targetValue, Time.deltaTime * smoothSpeed);

        // 4. 색상도 부드럽게 변경
        if (useColorChange)
        {
            Color targetColor = Color.Lerp(normalColor, highLoadColor, targetImage.fillAmount);
            targetImage.color = Color.Lerp(targetImage.color, targetColor, Time.deltaTime * colorSmoothSpeed);
        }
    }
}