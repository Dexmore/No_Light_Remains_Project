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
    [HideInInspector] public PlayerAttack_LSH attack;
    [HideInInspector] public PlayerAttackCombo_LSH attackCombo;

    // === 입력 (액션 에셋 참조) ===
    [Header("Input (use bound actions)")]
    [SerializeField] private InputActionReference moveActionRef;
    [SerializeField] private InputActionReference jumpActionRef;
    [SerializeField] private InputActionReference attackActionRef;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    private float _baseScaleX;

    // === Ground 체크 ===
    [Header("Ground Sensor (정교 판정)")]
    [SerializeField] private LayerMask groundLayer;
    [Range(0f, 1f)] public float groundNormalMinY = 0.6f;
    private readonly ContactPoint2D[] _contactPts = new ContactPoint2D[8];

    // Animator 해시/존재여부 캐시
    private static readonly int HashSpeedX = Animator.StringToHash("speedX");
    private static readonly int HashSpeedY = Animator.StringToHash("speedY");
    private static readonly int HashGround = Animator.StringToHash("grounded");
    private bool _hasSpeedX, _hasSpeedY, _hasGround;
    private float _lastSpeedX, _lastSpeedY; private bool _lastGround;

    // Runtime (FSM에서 참조)
    public bool Grounded { get; private set; }
    public float XInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        fsm  = new PlayerStateMachine_LSH();
        idle = new PlayerIdle_LSH(this, fsm);
        run  = new PlayerRun_LSH(this, fsm);
        jump = new PlayerJump_LSH(this, fsm);
        fall = new PlayerFall_LSH(this, fsm);
        attack = new PlayerAttack_LSH(this, fsm);
        attackCombo = new PlayerAttackCombo_LSH(this, fsm);


        _baseScaleX = Mathf.Abs(transform.localScale.x);

        // Animator 파라미터 존재여부 캐시
        CacheAnimatorParams();
    }

    void OnEnable()
    {
        if (moveActionRef == null || moveActionRef.action == null || jumpActionRef == null || jumpActionRef.action == null)
        {
            Debug.LogError("[PlayerController_LSH] InputActionReference가 비었습니다. 인스펙터에서 Move/Jump를 할당하세요.");
            enabled = false;
            return;
        }
        
        if (attackActionRef.action == null)
        {
            attackAction = null;
            Debug.LogError("[PlayerController_LSH] Attack 액션이 비어 있습니다. 공격 모션 입력이 동작하지 않습니다.");
        }

        moveAction = moveActionRef.action;
        jumpAction = jumpActionRef.action;
        attackAction = attackActionRef.action;

        moveAction.Enable();
        jumpAction.Enable();
        attackAction.Enable();
    }

    void OnDisable()
    {
        attackAction?.Disable();
        jumpAction?.Disable();
        moveAction?.Disable();
    }

    void Start()
    {
        fsm.Initialize(idle);
    }

    void Update()
    {
        // 입력
        XInput = ReadMoveX(moveAction);                 // -1 ~ +1
        JumpPressed = jumpAction.WasPressedThisFrame(); // 1프레임 true
        AttackPressed = attackAction != null && attackAction.WasPressedThisFrame();

        // 지면 체크
        Grounded = CheckGroundedPrecise();

        // FSM
        fsm.PlayerKeyInput();
        fsm.UpdateState();

        // Animator 파라미터(존재할 때 & 값 바뀔 때만 세팅)
        if (animator)
        {
            float sx = Mathf.Abs(rb.linearVelocity.x);
            float sy = rb.linearVelocity.y;

            if (_hasSpeedX && !Mathf.Approximately(_lastSpeedX, sx))
            {
                animator.SetFloat(HashSpeedX, sx);
                _lastSpeedX = sx;
            }

            if (_hasSpeedY && !Mathf.Approximately(_lastSpeedY, sy))
            {
                animator.SetFloat(HashSpeedY, sy);
                _lastSpeedY = sy;
            }
            if (_hasGround && _lastGround != Grounded)
            {
                animator.SetBool (HashGround, Grounded);
                _lastGround = Grounded;
            }
        }
    }

    void FixedUpdate()
    {
        fsm.UpdatePhysics();
    }

    private static float ReadMoveX(InputAction action)
    {
        if (action == null) return 0f;

        var ect = action.expectedControlType;
        if (ect == "Axis") return action.ReadValue<float>();

        Vector2 v = action.ReadValue<Vector2>();
        if (v != Vector2.zero || ect == "Vector2") return v.x;

        return action.ReadValue<float>(); // 폴백
    }

    private bool CheckGroundedPrecise()
    {
        var filter = new ContactFilter2D { useLayerMask = true, layerMask = groundLayer, useTriggers = false };
        int cp = rb.GetContacts(filter, _contactPts);
        for (int i = 0; i < cp; i++)
            if (_contactPts[i].normal.y >= groundNormalMinY) return true;
        return false;
    }

    public void UpdateFacing(float x)
    {
        if (x > 0.01f)
        {
            var s = transform.localScale;
            if (s.x < 0f)
            {
                s.x = _baseScaleX; transform.localScale = s;
            }
        }
        else if (x < -0.01f)
        {
            var s = transform.localScale;
            if (s.x > 0f)
            {
                s.x = -_baseScaleX; transform.localScale = s;
            }
        }
    }

    public void TriggerAttack()  => animator?.SetTrigger("Attack");
    public void TriggerAttack2() => animator?.SetTrigger("Attack2");
    public void TriggerHit()     => animator?.SetTrigger("Hit");
    public void TriggerDie()     => animator?.SetTrigger("Die");

    // ---- Helpers ----
    private void CacheAnimatorParams()
    {
        if (!animator) return;
        _hasSpeedX = HasParam(animator, "speedX", AnimatorControllerParameterType.Float);
        _hasSpeedY = HasParam(animator, "speedY", AnimatorControllerParameterType.Float);
        _hasGround = HasParam(animator, "grounded", AnimatorControllerParameterType.Bool);
    }

    private static bool HasParam(Animator anim, string name, AnimatorControllerParameterType type)
    {
        foreach (var p in anim.parameters)
            if (p.name == name && p.type == type) return true;
        return false;
    }
}