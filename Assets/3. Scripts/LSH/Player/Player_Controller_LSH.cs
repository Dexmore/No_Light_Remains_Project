<<<<<<< HEAD
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController_LSH : MonoBehaviour
{
=======
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController_LSH : MonoBehaviour
{
    public float height = 1.5f;
    public float width = 0.7f;
>>>>>>> KJH
    [Header("Move")]
    public float moveSpeed = 6f;
    public float airMoveMultiplier = 0.85f;
    [Header("Jump")]
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    [ReadOnlyInspector] public bool isGround;
    [ReadOnlyInspector] public bool isAvoid;
    [ReadOnlyInspector] public bool isParry;
    [ReadOnlyInspector] public State state;
    public enum State
    {
        Idle,
        Run,
        Attack,
        AttackCombo,
        Dash,
        Parry,
    }
    public InputActionAsset inputActionAsset;
    [HideInInspector] public PlayerStateMachine_LSH fsm;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Transform model;
    [HideInInspector] public Animator animator;
    [HideInInspector] public AttackRange attackRange;
    // States
    [HideInInspector] public PlayerIdle_LSH idle;
    [HideInInspector] public PlayerRun_LSH run;
    [HideInInspector] public PlayerAttack_LSH attack;
    [HideInInspector] public PlayerAttackCombo_LSH attackCombo;
<<<<<<< HEAD

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

=======
    [HideInInspector] public PlayerDash_LSH dash;
    [HideInInspector] public PlayerParry_LSH parry;
    void Awake()
    {
        TryGetComponent(out rb);
        model = transform.GetChild(0);
        animator = GetComponentInChildren<Animator>(true);
        attackRange = GetComponentInChildren<AttackRange>(true);
        fsm = new PlayerStateMachine_LSH();
        idle = new PlayerIdle_LSH(this, fsm);
        run = new PlayerRun_LSH(this, fsm);
        attack = new PlayerAttack_LSH(this, fsm);
        attackCombo = new PlayerAttackCombo_LSH(this, fsm);
        dash = new PlayerDash_LSH(this, fsm);
        parry = new PlayerParry_LSH(this, fsm);
    }
>>>>>>> KJH
    void Start()
    {
        fsm.ChangeState(idle);
    }
    // === Ground 체크 ===
    [HideInInspector] public Dictionary<Collider2D, Vector2> collisions = new Dictionary<Collider2D, Vector2>();
    void OnCollisionStay2D(Collision2D collision)
    {
        if ((collision.collider.gameObject.layer & groundLayer) != 0)
            if (!collisions.ContainsKey(collision.collider))
                collisions.Add(collision.collider, collision.contacts[0].point);
            else
                collisions[collision.collider] = collision.contacts[0].point;
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if ((collision.collider.gameObject.layer & groundLayer) != 0)
            if (collisions.ContainsKey(collision.collider))
                collisions.Remove(collision.collider);
    }
    void Update()
    {
<<<<<<< HEAD
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
=======
        fsm.Update();
        isGround = false;
        if (collisions.Count > 0)
            foreach (var element in collisions)
                if (Mathf.Abs(element.Value.y - transform.position.y) < 0.09f * height)
                {
                    isGround = true;
                    break;
                }
>>>>>>> KJH
    }
    void FixedUpdate()
    {
        fsm.FixedUpdate();
    }
    void OnEnable()
    {
<<<<<<< HEAD
        if (action == null) return 0f;

        var ect = action.expectedControlType;
        if (ect == "Axis") return action.ReadValue<float>();

        Vector2 v = action.ReadValue<Vector2>();
        if (v != Vector2.zero || ect == "Vector2") return v.x;

        return action.ReadValue<float>(); // 폴백
=======
        inputActionAsset.FindActionMap("Player").FindAction("Jump").performed += Input_Jump;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").performed += Input_LeftDash;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").canceled += InputCancel_LeftDash;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").performed += Input_RightDash;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").canceled += InputCancel_RightDash;
        inputActionAsset.FindActionMap("Player").FindAction("Parry").performed += Input_Parry;
        inputActionAsset.FindActionMap("Player").FindAction("Parry").canceled += InputCancel_Parry;
>>>>>>> KJH
    }
    void OnDisable()
    {
        inputActionAsset.FindActionMap("Player").FindAction("Jump").performed -= Input_Jump;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").performed -= Input_LeftDash;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").canceled -= InputCancel_LeftDash;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").performed -= Input_RightDash;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").canceled -= InputCancel_RightDash;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").canceled -= InputCancel_RightDash;
        inputActionAsset.FindActionMap("Player").FindAction("Parry").performed -= Input_Parry;
        inputActionAsset.FindActionMap("Player").FindAction("Parry").canceled -= InputCancel_Parry;
    }
    #region Jump
    [HideInInspector] public bool isJump;
    void Input_Jump(InputAction.CallbackContext callback)
    {
        if (!isGround) return;
        if (state != State.Idle && state != State.Run) return;
        if (isParryInput) return;
        if (!isJump)
        {
<<<<<<< HEAD
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
=======
            isJump = true;
            StartCoroutine(nameof(Jump));
        }
    }
    IEnumerator Jump()
    {
        yield return null;
        yield return YieldInstructionCache.WaitForSeconds(0.05f);
        if (state != State.Idle && state != State.Run)
        {
            isJump = false;
            yield break;
        }
        if (isParryInput)
        {
            isJump = false;
            yield break;
        }
        animator.Play("Player_Jump");
        rb.AddForce(Vector2.up * jumpForce * 50);
        bool isReachPeak = false;
        bool isFallAnimation = false;
        while (true)
        {
            yield return YieldInstructionCache.WaitForFixedUpdate;
            if (rb.linearVelocity.y < 0)
            {
                if (!isReachPeak)
                    isReachPeak = true;
            }
            if (isReachPeak && !isFallAnimation)
            {
                isJump = false;
                isFallAnimation = true;
                animator.Play("Player_Fall");
            }
            if (isGround)
            {
                isJump = false;
                animator.Play("Player_Idle");
                yield break;
            }
        }
    }
    #endregion
    #region Dash
    int leftDashInputCount = 0;
    void Input_LeftDash(InputAction.CallbackContext callback)
    {
        if (!isGround) return;
        if (leftDashInputCount == 0)
        {
            leftDashInputCount = 1;
            StopCoroutine(nameof(LeftDashRelease));
            StartCoroutine(nameof(LeftDashRelease));
        }
        else if (leftDashInputCount == 2)
        {
            StopCoroutine(nameof(LeftDash_co));
            StartCoroutine(nameof(LeftDash_co));
        }
    }
    void InputCancel_LeftDash(InputAction.CallbackContext callback)
    {
        if (!isGround) return;
        if (leftDashInputCount == 1)
            leftDashInputCount = 2;
    }
    IEnumerator LeftDashRelease()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.3f);
        leftDashInputCount = 0;
        rightDashInputCount = 0;
    }
    IEnumerator LeftDash_co()
    {
        float time = Time.time;
        while (Time.time - time < 0.3f)
        {
            yield return null;
            if (state == State.Idle || state == State.Run)
            {
                fsm.ChangeState(dash);
                break;
            }
        }
        leftDashInputCount = 0;
        rightDashInputCount = 0;
    }
    int rightDashInputCount = 0;
    void Input_RightDash(InputAction.CallbackContext callback)
    {
        if (!isGround) return;
        if (rightDashInputCount == 0)
        {
            rightDashInputCount = 1;
            StopCoroutine(nameof(RightDashRelease));
            StartCoroutine(nameof(RightDashRelease));
        }
        else if (rightDashInputCount == 2)
        {
            StopCoroutine(nameof(RightDash_co));
            StartCoroutine(nameof(RightDash_co));
        }
    }
    void InputCancel_RightDash(InputAction.CallbackContext callback)
    {
        if (!isGround) return;
        if (rightDashInputCount == 1)
            rightDashInputCount = 2;
    }
    IEnumerator RightDashRelease()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.3f);
        leftDashInputCount = 0;
        rightDashInputCount = 0;
    }
    IEnumerator RightDash_co()
    {
        float time = Time.time;
        while (Time.time - time < 0.3f)
        {
            yield return null;
            if (isGround && (state == State.Idle || state == State.Run))
            {
                fsm.ChangeState(dash);
                break;
            }
        }
        leftDashInputCount = 0;
        rightDashInputCount = 0;
    }
    #endregion
    #region Parry
    [HideInInspector] public bool isParryInput;
    void Input_Parry(InputAction.CallbackContext callback)
    {
        isParryInput = true;
        StopCoroutine(nameof(Parry_co));
        StartCoroutine(nameof(Parry_co));
    }
    void InputCancel_Parry(InputAction.CallbackContext callback)
    {
        isParryInput = false;
    }
    IEnumerator Parry_co()
    {
        float time = Time.time;
        while (Time.time - time < 0.3f)
        {
            yield return null;
            if (state == State.Idle || state == State.Run)
            {
                Debug.Log("Parry");
                fsm.ChangeState(parry);
                break;
            }
        }
    }
    #endregion
    #region Lantern

    #endregion
    #region Potion

    #endregion
    #region Inventory

    #endregion
    #region Interaction

    #endregion
    #region Hit

    #endregion
    #region Die
    
    #endregion




}
>>>>>>> KJH
