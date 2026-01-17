using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct CreditSlide
{
    [Header("슬라이드 타입 설정")]
    public bool isImageMode; 

    [Header("이미지 설정")]
    public List<Sprite> images; 

    [Header("텍스트 설정")]
    [TextArea(3, 5)] public string text; 
    
    [Header("시간 설정")]
    public float customDuration; 
}

public class EndingCreditController : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private CanvasGroup contentCanvasGroup; 
    [SerializeField] private Transform imageContainer;       
    [SerializeField] private GameObject imagePrefab;         
    [SerializeField] private TextMeshProUGUI displayText;    

    [Header("기본 설정")]
    [SerializeField] private float defaultDisplayTime = 3.0f; 
    [SerializeField] private float fadeInTime = 1.0f;         
    [SerializeField] private float fadeOutTime = 1.0f;        
    [SerializeField] private float delayBetween = 0.5f;       
    [SerializeField] private string lobbySceneName = "Lobby"; 

    [Header("이미지 크기 제한 (최대 폭/높이)")]
    // [신규] 이미지가 아무리 커도 이 사이즈 안쪽으로 리사이징 됨 (예: 800 x 500)
    [SerializeField] private Vector2 maxImageSize = new Vector2(800f, 500f);

    [Header("타자기 연출")]
    [SerializeField] private float typingSpeed = 0.05f;       
    [SerializeField] private string cursorChar = "_";         
    [SerializeField] private AudioSource audioSource;         
    [SerializeField] private AudioClip typingLoopClip;        

    [Header("크레딧 목록")]
    [SerializeField] private List<CreditSlide> credits; 

    private void Start()
    {
        contentCanvasGroup.alpha = 0f;
        contentCanvasGroup.blocksRaycasts = false; 
        
        foreach (Transform child in imageContainer) Destroy(child.gameObject);
        displayText.text = ""; 
        displayText.gameObject.SetActive(false); 

        StartCoroutine(PlayCredits());
    }

    private IEnumerator PlayCredits()
    {
        foreach (var slide in credits)
        {
            // --- [1] 모드에 따른 세팅 ---
            if (slide.isImageMode)
            {
                displayText.gameObject.SetActive(false);
                imageContainer.gameObject.SetActive(true);
                SetupImages(slide.images);
            }
            else
            {
                foreach (Transform child in imageContainer) Destroy(child.gameObject);
                imageContainer.gameObject.SetActive(false);
                displayText.gameObject.SetActive(true);
                displayText.text = ""; 
            }

            // --- [2] 페이드 인 ---
            yield return StartCoroutine(FadeRoutine(0f, 1f, fadeInTime));

            // --- [3] 내용 연출 ---
            if (!slide.isImageMode)
            {
                yield return StartCoroutine(TypewriterRoutine(slide.text));
            }

            // --- [4] 대기 ---
            float waitTime = (slide.customDuration > 0) ? slide.customDuration : defaultDisplayTime;
            
            if (!slide.isImageMode)
            {
                yield return StartCoroutine(BlinkCursorRoutine(waitTime, slide.text));
            }
            else
            {
                yield return new WaitForSeconds(waitTime);
            }

            // --- [5] 페이드 아웃 ---
            yield return StartCoroutine(FadeRoutine(1f, 0f, fadeOutTime));

            // --- [6] 정리 ---
            if (slide.isImageMode)
            {
                foreach (Transform child in imageContainer) Destroy(child.gameObject);
            }
            else
            {
                displayText.text = "";
            }
            
            yield return new WaitForSeconds(delayBetween);
        }
        
        GameManager.I.LoadSceneAsync(lobbySceneName);
    }

    // [핵심 수정] 이미지 크기 자동 조절 로직
    private void SetupImages(List<Sprite> images)
    {
        if (images == null || images.Count == 0) return;

        foreach (var sprite in images)
        {
            GameObject newObj = Instantiate(imagePrefab, imageContainer);
            Image img = newObj.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true; 
            
            // 1. 일단 원본 크기로 설정
            img.SetNativeSize(); 

            // 2. 만약 설정한 Max Size보다 크면 줄이기 (비율 유지)
            float currentW = img.rectTransform.sizeDelta.x;
            float currentH = img.rectTransform.sizeDelta.y;

            // 너비나 높이 둘 중 하나라도 제한을 넘으면 스케일 조정
            if (currentW > maxImageSize.x || currentH > maxImageSize.y)
            {
                // 가로 기준 비율 vs 세로 기준 비율 중 더 많이 줄여야 하는 쪽 선택
                float ratio = Mathf.Min(maxImageSize.x / currentW, maxImageSize.y / currentH);
                
                img.rectTransform.sizeDelta = new Vector2(currentW * ratio, currentH * ratio);
            }
        }
    }

    private IEnumerator TypewriterRoutine(string fullText)
    {
        if (string.IsNullOrEmpty(fullText)) yield break;

        if (audioSource != null && typingLoopClip != null)
        {
            audioSource.clip = typingLoopClip;
            audioSource.loop = true;
            audioSource.pitch = 1.0f; 
            audioSource.Play();
        }

        for (int i = 0; i <= fullText.Length; i++)
        {
            string currentText = fullText.Substring(0, i);
            // 커서 붙이기
            displayText.text = currentText + cursorChar;

            if (i < fullText.Length && (fullText[i] == '.' || fullText[i] == '\n'))
                yield return new WaitForSeconds(typingSpeed * 3f);
            else
                yield return new WaitForSeconds(typingSpeed);
        }

        if (audioSource != null) audioSource.Stop();
    }

    // [핵심 수정] 깜빡일 때 글자를 지우는 게 아니라 '투명하게' 만듭니다. (흔들림 방지)
    private IEnumerator BlinkCursorRoutine(float duration, string fullText)
    {
        float elapsed = 0f;
        bool showCursor = true;
        float blinkInterval = 0.5f; 
        float blinkTimer = 0f;

        // TMP 투명 태그 (글자 공간은 차지하되 안 보임)
        string transparentCursor = $"<color=#00000000>{cursorChar}</color>";

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                showCursor = !showCursor;
                
                // 보여줄 땐 그냥 커서, 안 보여줄 땐 투명 커서
                displayText.text = fullText + (showCursor ? cursorChar : transparentCursor);
            }
            yield return null;
        }
        
        // 끝나면 투명 커서 상태로 마무리 (공간 유지)
        displayText.text = fullText + transparentCursor;
    }

    private IEnumerator FadeRoutine(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            contentCanvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        contentCanvasGroup.alpha = end;
    }
}