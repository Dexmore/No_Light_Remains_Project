using UnityEngine;
using UnityEngine.InputSystem;

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

    private InputActionMap _playerActionMap;

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
        Debug.Log("[WorkbenchObject] Run 호출됨");

        // 1. workbenchUI 스크립트 연결 체크
        if (workbenchUI == null) 
        {
            Debug.LogError("[WorkbenchObject] WorkbenchUI가 연결되지 않았습니다!");
            return;
        }

        // 2. [수정] IsActive 대신 IsUIActive() 함수로 확인
        if (workbenchUI.IsUIActive()) 
        {
            Debug.Log("[WorkbenchObject] UI가 이미 켜져 있어(IsUIActive==true) 중복 실행 방지");
            return;
        }

        // 3. 게임 매니저 팝업 상태 체크
        if (GameManager.I != null && GameManager.I.isOpenPop) 
        {
            Debug.Log("[WorkbenchObject] GameManager.isOpenPop 상태라 실행 중단");
            return;
        }

        Debug.Log("[WorkbenchObject] 모든 조건 통과 -> 상호작용 시작");

        if (playerControl == null) 
            playerControl = FindObjectOfType<PlayerControl>();

        if (playerControl != null)
        {
            // 물리 정지
            playerControl.stop.duration = 9999999;
            playerControl.fsm.ChangeState(playerControl.stop);

            if (playerControl.rb != null)
            {
                playerControl.rb.linearVelocity = Vector2.zero;
                playerControl.rb.angularVelocity = 0f;
            }

            // [핵심] 입력 차단
            if (playerControl.inputActionAsset != null)
            {
                _playerActionMap = playerControl.inputActionAsset.FindActionMap("Player");
                if (_playerActionMap != null)
                {
                    _playerActionMap.Disable();
                    Debug.Log("[WorkbenchObject] 플레이어 입력 차단됨 (Input System Disable)");
                }
            }
        }

        // 게임 매니저 상태 설정
        if (GameManager.I != null) GameManager.I.isOpenPop = true;

        // UI 열기
        OpenWorkbench();
    }

    public void OpenWorkbench()
    {
        if (workbenchUI != null) workbenchUI.Open();
    }

    private void HandleUIClose()
    {
        Debug.Log("[WorkbenchObject] UI 닫힘 -> 플레이어 조작 복구 시작");

        // 게임 매니저 상태 복구
        if (GameManager.I != null) GameManager.I.isOpenPop = false;

        // 플레이어 조작 복구
        if (playerControl != null)
        {
            if(playerControl.fsm.currentState == playerControl.stop)
            {
                playerControl.stop.duration = 0f;
            }

            // 입력 복구
            if (_playerActionMap != null)
            {
                _playerActionMap.Enable();
                _playerActionMap = null;
                Debug.Log("[WorkbenchObject] 플레이어 입력 복구됨 (Input System Enable)");
            }
            else if (playerControl.inputActionAsset != null)
            {
                playerControl.inputActionAsset.FindActionMap("Player")?.Enable();
            }
        }
    }
}