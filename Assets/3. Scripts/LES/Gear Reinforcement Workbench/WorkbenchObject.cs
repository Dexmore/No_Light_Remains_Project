using UnityEngine;
using UnityEngine.InputSystem;

public class WorkbenchObject : Interactable
{
    #region Interactable Complement
    public override Type type => Type.Normal;
    public override bool isReady { get; set; } = true;
    public override bool isAuto => false;
    #endregion

    [Header("플레이어 연결 (자동으로 찾음)")]
    public PlayerControl playerControl; 
    
    [Header("연결 정보")]
    [SerializeField] private WorkbenchUI workbenchUI; 

    private InputActionMap _playerActionMap;

    private void Awake()
    {
        // 1. UI 찾기 (기존 로직 + 강화)
        if (workbenchUI == null)
        {
            workbenchUI = GetComponentInChildren<WorkbenchUI>(true);
            if (workbenchUI == null) workbenchUI = FindObjectOfType<WorkbenchUI>(true);
        }
        
        if (workbenchUI != null)
        {
            workbenchUI.OnClose -= HandleUIClose; 
            workbenchUI.OnClose += HandleUIClose;
        }

        // 2. [추가] 게임 시작하자마자 플레이어 찾기 시도
        FindPlayerControlForce();
    }

    private void OnDestroy()
    {
        if (workbenchUI != null) workbenchUI.OnClose -= HandleUIClose;
    }

    public override void Run()
    {
        Debug.Log("[WorkbenchObject] Run 호출됨");

        // 1. UI 연결 체크
        if (workbenchUI == null) 
        {
            workbenchUI = FindObjectOfType<WorkbenchUI>(true);
            if (workbenchUI == null)
            {
                Debug.LogError("[WorkbenchObject] ❌ WorkbenchUI를 찾을 수 없습니다!");
                return;
            }
        }

        // 2. UI 중복 실행 방지
        if (workbenchUI.IsUIActive()) return;

        // 3. 게임 매니저 체크
        if (GameManager.I != null && GameManager.I.isOpenPop) return;


        // 4. [핵심 수정] 플레이어 찾기 (없으면 찾을 때까지 뒤짐)
        if (playerControl == null)
        {
            FindPlayerControlForce();
        }

        // 그래도 못 찾았으면 에러 띄우고 중단 (NullReference 방지)
        if (playerControl == null)
        {
            Debug.LogError("[WorkbenchObject] ❌ PlayerControl을 찾지 못했습니다! 플레이어가 씬에 있는지, 태그가 'Player'인지 확인하세요.");
            return; 
        }

        // --- 여기서부터 정상 실행 ---
        Debug.Log("[WorkbenchObject] 플레이어 확인됨 -> 상호작용 시작");

        // 물리 정지
        playerControl.stop.duration = 9999999;
        playerControl.fsm.ChangeState(playerControl.stop);

        if (playerControl.rb != null)
        {
            playerControl.rb.linearVelocity = Vector2.zero;
            playerControl.rb.angularVelocity = 0f;
        }

        // 입력 차단
        if (playerControl.inputActionAsset != null)
        {
            _playerActionMap = playerControl.inputActionAsset.FindActionMap("Player");
            if (_playerActionMap != null)
            {
                _playerActionMap.Disable();
                Debug.Log("[WorkbenchObject] 플레이어 입력 차단됨");
            }
        }

        // 게임 매니저 상태 설정
        if (GameManager.I != null) GameManager.I.isOpenPop = true;

        // UI 열기
        OpenWorkbench();
    }

    // [신규 함수] 플레이어를 찾는 3단계 추적 로직
    private void FindPlayerControlForce()
    {
        // 시도 1: 가장 일반적인 방법 (켜져있는 놈 찾기)
        playerControl = FindObjectOfType<PlayerControl>();

        // 시도 2: 꺼져있는 놈도 포함해서 찾기 (유니티 버전에 따라 다를 수 있어 구형/신형 방식 모두 고려)
        if (playerControl == null)
        {
            // FindObjectOfType 오버로딩 (true = includeInactive)
            playerControl = FindObjectOfType<PlayerControl>(true);
        }

        // 시도 3: 태그("Player")로 찾기 (가장 확실함)
        if (playerControl == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerControl = playerObj.GetComponent<PlayerControl>();
            }
        }

        if (playerControl != null)
        {
            Debug.Log($"[WorkbenchObject] ✅ 플레이어 찾기 성공: {playerControl.name}");
        }
    }

    public void OpenWorkbench()
    {
        if (workbenchUI != null) workbenchUI.Open();
    }

    private void HandleUIClose()
    {
        Debug.Log("[WorkbenchObject] UI 닫힘 -> 플레이어 조작 복구 시작");

        if (GameManager.I != null) GameManager.I.isOpenPop = false;

        if (playerControl != null)
        {
            if(playerControl.fsm.currentState == playerControl.stop)
            {
                playerControl.stop.duration = 0f;
                // Idle 상태로 복귀가 필요한지 체크 (보통 stop duration이 끝나면 fsm이 알아서 처리하거나 수동 전환)
                playerControl.fsm.ChangeState(playerControl.idle); 
            }

            // 입력 복구
            if (_playerActionMap != null)
            {
                _playerActionMap.Enable();
                _playerActionMap = null;
            }
            else if (playerControl.inputActionAsset != null)
            {
                playerControl.inputActionAsset.FindActionMap("Player")?.Enable();
            }
            Debug.Log("[WorkbenchObject] 플레이어 입력 복구됨");
        }
    }
}