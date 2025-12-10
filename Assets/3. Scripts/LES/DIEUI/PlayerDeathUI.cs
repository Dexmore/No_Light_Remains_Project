using UnityEngine;
using UnityEngine.UI; // 이미지 제어용
using DG.Tweening;    // 애니메이션용 (개발자님 프로젝트에 이미 있음)

public class PlayerDeathUI : MonoBehaviour
{
    [Header("Target Image")]
    [Tooltip("쉐이더가 적용된 'DeathImage'를 연결하세요.")]
    public Image deathImage; 

    private PlayerControl playerControl;
    private bool isDeadProcessed = false;
    private Material uiMat; // 복제된 재질을 저장할 변수

    void Start()
    {
        playerControl = FindAnyObjectByType<PlayerControl>();
        
        if(deathImage != null)
        {
            // 중요: 원본 재질을 건드리지 않기 위해 인스턴스를 가져옵니다.
            uiMat = deathImage.material;
            // 게임 시작 시 완전히 투명하게(1) 숨겨둡니다.
            uiMat.SetFloat("_DissolveAmount", 1f);
        }
    }

    void Update()
    {
        if (playerControl == null || isDeadProcessed) return;

        if (playerControl.fsm.currentState == playerControl.die)
        {
            ShowDieEffect();
            isDeadProcessed = true;
        }
    }

    void ShowDieEffect()
    {
<<<<<<< Updated upstream
        if (deathImage != null)
=======
        yield return new WaitForSeconds(2.5f);
        // 1. UI 캔버스 켜기
        if (deathScreenUI != null) deathScreenUI.SetActive(true);

        // 2. 불타는 연출 시작 (글자 나타나기)
        if (deathImage != null && uiMat != null)
>>>>>>> Stashed changes
        {
            // 1. 오브젝트 활성화
            deathImage.gameObject.SetActive(true);

            // 2. Dissolve Amount를 1(투명)에서 0(보임)으로 2초 동안 변경
            // "_DissolveAmount"는 쉐이더 코드에 있는 변수 이름입니다.
            uiMat.DOFloat(0f, "_DissolveAmount", 2.5f)
                 .SetEase(Ease.OutQuad); // 부드러운 속도
            
            Debug.Log("불타는 효과 시작!");
        }
    }
}