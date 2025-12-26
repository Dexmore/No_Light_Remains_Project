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

    [Tooltip("배경에 깔릴 검은 띠 이미지")]
    public Image backgroundBar;

    [Tooltip("쉐이더가 적용된 'DeathImage' (YOU DIED 이미지)")]
    public Image deathImage;

    [Tooltip("화면을 암전시킬 'FadePanel' (검은색 패널)")]
    public Image fadePanel;

    [Header("Timing Settings")]
    [Tooltip("검은 띠가 페이드 인 되는 시간")]
    public float barAppearDuration = 1.0f; // 부드러움을 위해 0.5초 -> 1.0초로 살짝 늘림

    [Tooltip("검은 띠의 최종 투명도 (0~1, 1이면 완전 검정)")]
    [Range(0f, 1f)]
    public float barFinalAlpha = 0.8f;

    [Tooltip("글자가 불타며 나타나는 시간")]
    public float dissolveDuration = 2.0f;

    [Tooltip("글자가 다 뜨고나서 유지되는 시간")]
    public float displayDuration = 3.0f;

    [Tooltip("화면이 검게 암전되는 시간")]
    public float fadeDuration = 1.0f;

    [Header("References")]
    public PlayerControl playerControl;

    private bool isDeadProcessed = false;
    private Material uiMat; 

    void Start()
    {
        if (playerControl == null)
            playerControl = FindAnyObjectByType<PlayerControl>();

        if (deathScreenUI != null)
            deathScreenUI.SetActive(false);

        // 1. 쉐이더 초기화
        if (deathImage != null)
        {
            uiMat = deathImage.material;
            if (uiMat.HasProperty("_DissolveAmount"))
                uiMat.SetFloat("_DissolveAmount", 1f);
        }

        // 2. 배경 띠 초기화 (투명하게만 설정)
        if (backgroundBar != null)
        {
            Color c = backgroundBar.color;
            c.a = 0f;
            backgroundBar.color = c;
            // 스케일 조절 코드 삭제됨 (원래 크기 유지)
        }

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
        // 죽는 모션 보는 시간
        yield return new WaitForSeconds(2.5f);

        if (deathScreenUI != null) deathScreenUI.SetActive(true);

        // --- [연출 1단계] 검은 띠 페이드 인 ---
        if (backgroundBar != null)
        {
            // 투명도만 서서히 올림 (0 -> 설정값)
            backgroundBar.DOFade(barFinalAlpha, barAppearDuration);
        }

        // --- [연출 2단계] 글자 등장 ---
        // 띠가 어느 정도 보이기 시작할 때 글자 연출 시작 (타이밍 조절)
        // 띠가 절반 정도(50%) 나타났을 때 글자 시작
        yield return new WaitForSeconds(barAppearDuration * 0.5f);

        if (deathImage != null && uiMat != null)
        {
            uiMat.DOFloat(0f, "_DissolveAmount", dissolveDuration).SetEase(Ease.OutQuad);
        }

        // 대기 (글자 나타나는 시간 + 유지 시간)
        yield return new WaitForSeconds(dissolveDuration + displayDuration);

        // --- [연출 3단계] 암전 ---
        if (fadePanel != null)
        {
            fadePanel.DOFade(1f, fadeDuration);
        }

        yield return new WaitForSeconds(fadeDuration);

        // --- [데이터 복구 및 씬 로드] ---
        if (DBManager.I != null)
        {
            DBManager.I.currData.currHealth = DBManager.I.currData.maxHealth;
            DBManager.I.currData.currPotionCount = DBManager.I.currData.maxPotionCount;
            DBManager.I.currData.currBattery = DBManager.I.currData.maxBattery;
        }

        if (playerControl == null) playerControl = FindAnyObjectByType<PlayerControl>();
        if (playerControl) playerControl.currHealth = DBManager.I.currData.maxHealth;

        yield return null;
        
        string currentSceneName = DBManager.I.currData.sceneName;
        GameManager.I.SetSceneFromDB();
        GameManager.I.LoadSceneAsync(currentSceneName, false, true);
    }
}