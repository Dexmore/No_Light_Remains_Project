using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 필수
using UnityEngine.SceneManagement; // 씬 이동 필수
using System.Collections;

public class PlayerDeathUI : MonoBehaviour
{
    [Header("UI Objects")]
    [Tooltip("전체 UI 캔버스 (DIECanvas)")]
    public GameObject deathScreenUI; 

    [Tooltip("쉐이더가 적용된 'DeathImage' (YOU DIED 이미지)")]
    public Image deathImage; 
    
    [Tooltip("화면을 암전시킬 'FadePanel' (검은색 패널)")]
    public Image fadePanel; 

    [Header("Timing Settings (조절 가능)")]
    [Tooltip("글자가 불타며 나타나는 시간 (기본 2초)")]
    public float dissolveDuration = 2.0f;

    [Tooltip("글자가 다 뜨고나서 유지되는 시간 (기본 3초)")]
    public float displayDuration = 3.0f;

    [Tooltip("화면이 검게 암전되는 시간 (기본 1초)")]
    public float fadeDuration = 1.0f;

    [Header("References")]
    [Tooltip("플레이어 컨트롤러 (자동으로 못 찾을 경우 직접 연결)")]
    public PlayerControl playerControl;

    private bool isDeadProcessed = false;
    private Material uiMat; // 쉐이더 제어용 재질

    void Start()
    {
        // 1. 플레이어 연결 (비어있으면 자동 찾기)
        if (playerControl == null)
            playerControl = FindAnyObjectByType<PlayerControl>();

        // 2. 초기화: UI 꺼두기
        if (deathScreenUI != null)
            deathScreenUI.SetActive(false);

        // 3. 죽음 이미지 쉐이더 초기화 (투명하게 숨김)
        if(deathImage != null)
        {
            uiMat = deathImage.material;
            // 1 = 완전 투명, 0 = 완전 보임
            if(uiMat.HasProperty("_DissolveAmount"))
                uiMat.SetFloat("_DissolveAmount", 1f); 
        }

        // 4. 페이드 패널 초기화 (투명하게)
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = new Color(0, 0, 0, 0); 
        }
    }

    void Update()
    {
        if (playerControl == null || isDeadProcessed) return;

        // 플레이어 상태가 'die'가 되면 실행
        if (playerControl.fsm.currentState == playerControl.die)
        {
            isDeadProcessed = true;
            StartCoroutine(ProcessDeathSequence());
        }
    }

    // [핵심 로직] 시간 설정을 반영한 시퀀스
    IEnumerator ProcessDeathSequence()
    {
        // 1. UI 캔버스 켜기
        if (deathScreenUI != null) deathScreenUI.SetActive(true);

        // 2. 불타는 연출 시작 (글자 나타나기)
        if (deathImage != null && uiMat != null)
        {
            // 설정한 dissolveDuration 동안 나타남
            uiMat.DOFloat(0f, "_DissolveAmount", dissolveDuration).SetEase(Ease.OutQuad);
        }

        // [중요] 글자가 나타나는 시간 + 대기 시간(3초)만큼 기다림
        // 즉, 글자가 다 뜨고 나서도 3초 동안 멍하니 화면을 보여줌
        yield return new WaitForSeconds(dissolveDuration + displayDuration);

        // 3. 페이드 아웃 (화면 암전)
        if (fadePanel != null)
        {
            // 설정한 fadeDuration 동안 검게 변함
            fadePanel.DOFade(1f, fadeDuration);
        }

        // 4. 암전이 완료될 때까지 대기
        yield return new WaitForSeconds(fadeDuration);

        // 5. 데이터 복구 (체력 만피 채우기)
        if (DBManager.I != null)
        {
            DBManager.I.currData.currHealth = DBManager.I.currData.maxHealth;
        }

        // 6. 현재 씬 재시작
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}