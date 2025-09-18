using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;
    
    [Header("Jump")]
    public float jumpForce = 5f;
    public int maxJumpCount = 1; // 최대 점프 횟수(이후에 잠금 해제 요소로 더블 점프 추가 가능)

    [Header("Ground Check")]
    public LayerMask groundLayer; // 바닥 레이어
    private bool isGrounded = true; // 바닥에 닿았는지 여부

    // FSM 관련 변수
    private PlayerStateMachine_LSH fsm;
    private PlayerIdle_LSH idle;

    // Components
    private Animator anim;
    private Rigidbody2D rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        fsm = new PlayerStateMachine_LSH();
        idle = new PlayerIdle_LSH();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        fsm.Initialize(idle);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            
        }
    }
}
