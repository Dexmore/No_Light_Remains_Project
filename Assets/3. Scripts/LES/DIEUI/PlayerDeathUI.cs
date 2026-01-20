using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 
using UnityEngine.SceneManagement; 
using System.Collections;

public class PlayerDeathUI : MonoBehaviour
{
    [Header("UI Objects")]
    [Tooltip("전체 UI 캔버스 (DIECanvas)")]
    public GameObject deathScreenUI;

    [Tooltip("쉐이더가 적용된 'DeathImage' (YOU DIED 이미지)")]
    public Image deathImage;

    // [추가됨] 글자 뒤에 있는 검정 띠 (배경 이미지)
    [Tooltip("글자 뒤에 깔리는 검정 띠 이미지")]
    public Image blackBand; 

    [Tooltip("화면을 암전시킬 'FadePanel' (검은색 패널)")]
    public Image fadePanel;

    [Header("Timing Settings")]
    public float dissolveDuration = 2.0f;
    public float displayDuration = 3.0f;
    public float fadeDuration = 1.0f;

    [Header("References")]
    public PlayerControl playerControl;

    private bool isDeadProcessed = false;
    private Material uiMat; 
    private float bandTargetAlpha; // 검정 띠의 원래 투명도를 저장할 변수

    void Start()
    {
        if (playerControl == null)
            playerControl = FindAnyObjectByType<PlayerControl>();

        if (deathScreenUI != null)
            deathScreenUI.SetActive(false);

        // 1. 죽음 이미지 쉐이더 초기화
        if (deathImage != null)
        {
            uiMat = deathImage.material;
            if (uiMat.HasProperty("_DissolveAmount"))
                uiMat.SetFloat("_DissolveAmount", 1f);
        }

        // 2. 검정 띠 초기화 (추가된 로직)
        if (blackBand != null)
        {
            // 인스펙터에서 설정한 원래 투명도를 기억해둠 (예: 0.8 등)
            bandTargetAlpha = blackBand.color.a;
            
            // 시작할 때는 투명하게(0) 만들어서 안 보이게 함
            Color c = blackBand.color;
            c.a = 0f;
            blackBand.color = c;
        }

        // 3. 페이드 패널 초기화
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = new Color(0, 0, 0, 0);
        }
    }

    void Update()
    {
        if (playerControl == null || isDeadProcessed) return;

        if (playerControl.fsm.currentState == playerControl.die)
        {
            isDeadProcessed = true;
            StartCoroutine(ProcessDeathSequence());
        }
    }

    IEnumerator ProcessDeathSequence()
    {
        yield return new WaitForSeconds(2.5f);

        if (deathScreenUI != null) deathScreenUI.SetActive(true);

        // [동시 실행] 글자 이펙트 + 검정 띠 페이드 인
        
        // 1. 글자 (쉐이더)
        if (deathImage != null && uiMat != null)
        {
            uiMat.DOFloat(0f, "_DissolveAmount", dissolveDuration)
                 .SetEase(Ease.OutQuad)
                 .SetLink(gameObject);
        }

        // 2. 검정 띠 (추가된 로직)
        if (blackBand != null)
        {
            // 기억해둔 원래 투명도(bandTargetAlpha)까지 부드럽게 복구
            blackBand.DOFade(bandTargetAlpha, dissolveDuration)
                     .SetEase(Ease.OutQuad)
                     .SetLink(gameObject);
        }

        yield return new WaitForSeconds(dissolveDuration + displayDuration);

        // 3. 화면 암전
        if (fadePanel != null)
        {
            fadePanel.DOFade(1f, fadeDuration).SetLink(gameObject);
        }

        yield return new WaitForSeconds(fadeDuration);

        // 데이터 복구 및 씬 로드
        if (DBManager.I != null)
        {
            DBManager.I.currData.currHealth = DBManager.I.currData.maxHealth;
            DBManager.I.currData.currPotionCount = DBManager.I.currData.maxPotionCount;
            DBManager.I.currData.currBattery = DBManager.I.currData.maxBattery;
        }

        if (playerControl == null) playerControl = FindAnyObjectByType<PlayerControl>();
        if (playerControl) playerControl.currHealth = DBManager.I.currData.maxHealth;

        DOTween.KillAll(); 

        yield return null;
        
        string currentSceneName = DBManager.I.currData.sceneName;
        GameManager.I.SetSceneFromDB();
        GameManager.I.LoadSceneAsync(currentSceneName, false, true);
    }

    private void OnDestroy()
    {
        transform.DOKill(); 
        if(uiMat != null) DOTween.Kill(uiMat);
    }
}