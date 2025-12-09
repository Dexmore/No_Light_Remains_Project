using UnityEngine;

public class PlayerDeathUI : MonoBehaviour
{
    [Header("UI Object to Show")]
    [Tooltip("플레이어 사망 시 띄울 UI 패널이나 텍스트 오브젝트를 여기에 연결하세요.")]
    public GameObject deathScreenUI; 

    [SerializeField] private PlayerControl playerControl;
    private bool isDeadProcessed = false; // 중복 실행 방지용 플래그

    void Start()
    {
        // 씬에 있는 플레이어를 자동으로 찾습니다.
        playerControl = FindAnyObjectByType<PlayerControl>();

        if (playerControl == null)
        {
            Debug.LogError("PlayerControl 스크립트를 가진 오브젝트를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // 플레이어를 찾지 못했거나, 이미 죽음 처리가 되었다면 실행하지 않음
        if (playerControl == null || isDeadProcessed) return;

        // 플레이어의 현재 FSM 상태가 'PlayerDie' 상태인지 감지
        // 공유해주신 PlayerControl.cs에 die 변수가 public으로 선언되어 있어 접근 가능합니다.
        if (playerControl.fsm.currentState == playerControl.die)
        {
            ShowDieUI();
            isDeadProcessed = true; // 이후 중복 호출 차단
        }
    }

    void ShowDieUI()
    {
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(true);
            Debug.Log("플레이어 사망 확인: UI 활성화");
        }
    }
}