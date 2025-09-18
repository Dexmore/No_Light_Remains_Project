using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController_LSH : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public float airMoveMultiplier = 0.85f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Animation (Optional)")]
    public Animator animator;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public PlayerStateMachine_LSH fsm;

    // States
    [HideInInspector] public PlayerIdle_LSH idle;
    [HideInInspector] public PlayerRun_LSH run;
    [HideInInspector] public PlayerJump_LSH jump;
    [HideInInspector] public PlayerFall_LSH fall;

    // === 입력 (새 Input System) ===
    private InputAction moveAction;
    private InputAction jumpAction;

    // === 정교 Grounded 판정(노멀 검사) ===
    [Header("Ground Sensor (정교 판정)")]
    [SerializeField] private Collider2D feetCollider;     // 발바닥 전용 콜라이더(Trigger 꺼짐 권장)
    [Range(0f, 1f)] public float groundNormalMinY = 0.6f; // 바닥으로 인정할 최소 노멀 y (≈53° 이하)
    [SerializeField] private LayerMask groundLayer;

    private ContactFilter2D _groundFilter;
    private readonly Collider2D[] _overlapHits = new Collider2D[8];     // NonAlloc 캐시
    private readonly ContactPoint2D[] _contactPts = new ContactPoint2D[8];

    // Runtime 노출 값 (States에서 참조)
    public bool Grounded { get; private set; }
    public float XInput { get; private set; }
    public bool JumpPressed { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        fsm = new PlayerStateMachine_LSH();
        idle = new PlayerIdle_LSH(this, fsm);
        run  = new PlayerRun_LSH(this, fsm);
        jump = new PlayerJump_LSH(this, fsm);
        fall = new PlayerFall_LSH(this, fsm);

        // Ground 레이어 전용 필터 (Trigger 제외)
        _groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundLayer,
            useTriggers = false
        };
    }

    void OnEnable()
    {
        // ---- 새 Input System 액션 생성/바인딩 ----
        // 1D 이동 축 : 키보드(AD/←→) + 패드 왼스틱 X
        moveAction = new InputAction(
            name: "Move",
            type: InputActionType.Value,
            binding: "<Gamepad>/leftStick/x"
        );
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/d")
            .With("Positive", "<Keyboard>/rightArrow");

        // 점프 버튼 : Space + 패드 South
        jumpAction = new InputAction(name: "Jump", type: InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        moveAction.Enable();
        jumpAction.Enable();
    }

    void OnDisable()
    {
        jumpAction?.Disable();
        moveAction?.Disable();
    }

    void Start()
    {
        fsm.Initialize(idle);
    }

    void Update()
    {
        // ---- 입력 수집 (새 Input System) ----
        XInput = moveAction.ReadValue<float>();          // -1 ~ +1
        JumpPressed = jumpAction.WasPressedThisFrame();  // 이 프레임에만 true

        // ---- 지상 체크 (정교: 노멀 검사) ----
        Grounded = CheckGroundedPrecise();

        // FSM 틱
        fsm.PlayerKeyInput();
        fsm.UpdateState();

        // 애니메이터(선택)
        if (animator)
        {
            animator.SetFloat("speedX", Mathf.Abs(rb.linearVelocity.x));
            animator.SetFloat("speedY", rb.linearVelocity.y);
            animator.SetBool("grounded", Grounded);
        }
        // WasPressedThisFrame은 자동 원샷이라 수동 초기화 불필요
    }

    void FixedUpdate()
    {
        fsm.UpdatePhysics();
    }

    // === 정교 지상 판정 ===
    private bool CheckGroundedPrecise()
    {
        if (feetCollider == null) return false;

        int count = feetCollider.Overlap(_groundFilter, _overlapHits); // 무할당
        for (int i = 0; i < count; i++)
        {
            var col = _overlapHits[i];
            if (col == null) continue;

            int cpCount = col.GetContacts(_contactPts); // 무할당
            for (int j = 0; j < cpCount; j++)
            {
                Vector2 n = _contactPts[j].normal; // 플레이어 관점의 접촉 노멀
                if (n.y >= groundNormalMinY)       // 위로 받쳐주는 성분이 충분히 큰가?
                    return true;
            }
        }
        return false;
    }
}
