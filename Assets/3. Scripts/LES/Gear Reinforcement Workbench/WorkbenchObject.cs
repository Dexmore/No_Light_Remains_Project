using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

public class WorkbenchObject : Interactable
{
    #region Interactable Complement
    public override Type type => Type.Normal;
    public override bool isReady { get; set; } = true;
    public override bool isAuto => false;
    #endregion

    PlayerControl playerControl;
    
    [Header("연결 정보")]
    [SerializeField] private WorkbenchUI workbenchUI; // 하이어라키에 있는 UI 프리팹 혹은 캔버스 객체
    [SerializeField] private Animator animator; // (선택) 기계가 작동할 때 애니메이션

    [Header("사운드 (선택)")]
    [SerializeField] private string interactSoundName = "Machine_Start";

    /// <summary>
    /// 기존 상호작용 스크립트에서 이 함수를 호출해주세요.
    /// 예: interactionEvent.Invoke();
    /// </summary>
    public void OpenWorkbench()
    {
        if (workbenchUI != null)
        {
            // 1. UI 열기
            workbenchUI.Open();

            // 2. 사운드 재생 (기존 시스템 활용 가정)
            // AudioManager.I?.PlaySFX(interactSoundName);

            // 3. 기계 애니메이션 (화면 켜짐 등)
            if (animator != null) animator.SetTrigger("OnOperate");
            
            Debug.Log("강화 작업대 UI가 열렸습니다.");
        }
        else
        {
            Debug.LogError("WorkbenchUI가 연결되지 않았습니다! 인스펙터를 확인해주세요.");
        }
    }

    public override void Run()
    {
        if (playerControl != null && playerControl.fsm.currentState != playerControl.stop)
            playerControl.fsm.ChangeState(playerControl.stop);
            
        Debug.Log("강화창 UI 열기");
    }
}