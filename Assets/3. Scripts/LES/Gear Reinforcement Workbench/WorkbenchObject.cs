using UnityEngine;

public class WorkbenchObject : Interactable
{
    #region Interactable Complement
    public override Type type => Type.Normal;
    public override bool isReady { get; set; } = true;
    public override bool isAuto => false;
    #endregion

    public PlayerControl playerControl; 
    
    [Header("연결 정보")]
    [SerializeField] private WorkbenchUI workbenchUI; 

    [Header("사운드 (선택)")]
    [SerializeField] private string interactSoundName = "Machine_Start";

    private void Awake()
    {
        if (workbenchUI == null)
            workbenchUI = GetComponentInChildren<WorkbenchUI>(true);
        
        if (workbenchUI != null)
        {
            workbenchUI.OnClose -= HandleUIClose; 
            workbenchUI.OnClose += HandleUIClose;
        }
    }

    private void OnDestroy()
    {
        if (workbenchUI != null) workbenchUI.OnClose -= HandleUIClose;
    }

    public override void Run()
    {
        Debug.Log("[WorkbenchObject] 상호작용 시작");

        if (playerControl == null) 
            playerControl = FindObjectOfType<PlayerControl>();

        // 1. 플레이어 완벽 정지 (속도 0으로 초기화 + 상태 변경)
        if (playerControl != null)
        {
            playerControl.rb.linearVelocity = Vector2.zero; // [중요] 미끄러짐 방지 (Unity 6)
            playerControl.fsm.ChangeState(playerControl.stop);
        }

        // 2. 게임 매니저 상태 변경 (다른 팝업/설정창이 뜨지 않도록 막음)
        if (GameManager.I != null) GameManager.I.isOpenPop = true;

        // 3. UI 열기
        OpenWorkbench();
    }

    public void OpenWorkbench()
    {
        if (workbenchUI != null) workbenchUI.Open();
    }

    // UI가 닫힐 때 호출
    private void HandleUIClose()
    {
        // 1. 플레이어 상태 복구
        if (playerControl != null)
        {
            if(playerControl.fsm.currentState == playerControl.stop)
                 playerControl.fsm.ChangeState(playerControl.idle); 
        }

        // 2. 게임 매니저 상태 복구 (이제 ESC 누르면 설정창 열림)
        if (GameManager.I != null) GameManager.I.isOpenPop = false;

        Debug.Log("[WorkbenchObject] UI 닫힘: 조작 복구 완료");
    }
}