using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController_LSH : MonoBehaviour, IDamageable_LSH, IParry_LSH
{
    [Header("Player HP")]
    public int maxHealth = 1000;
    public int currentHealth;

    [Header("Light Resource")]
    public int maxLight = 100;
    public int currentLight = 50;
    public int parryLightGain = 15;
    public void AddLight(int amount)
    {
        currentLight = Mathf.Clamp(currentLight + amount, 0, maxLight);
        Debug.Log($"[Light] +{amount} => {currentLight}/{maxLight}");
    }

    [Header("Move")]
    public float moveSpeed = 6f;
    public float airMoveMultiplier = 0.85f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Animation (Optional)")]
    public Animator animator;

    // === 랜턴 관련 ===
    [Header("Lantern")]
    public bool lanternOn = false;   // 현재 상태

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public PlayerStateMachine_LSH fsm;

    // States (기존)
    [HideInInspector] public PlayerIdle_LSH idle;
    [HideInInspector] public PlayerRun_LSH run;
    [HideInInspector] public PlayerJump_LSH jump;
    [HideInInspector] public PlayerFall_LSH fall;
    [HideInInspector] public PlayerAttack_LSH attack;
    [HideInInspector] public PlayerAttackCombo_LSH attackCombo;
    // 새 상태
    [HideInInspector] public PlayerParry_LSH parry;

    // === 입력 (액션 에셋 참조) ===
    [Header("Input (use bound actions)")]
    [SerializeField] private InputActionReference moveActionRef;
    [SerializeField] private InputActionReference jumpActionRef;
    [SerializeField] private InputActionReference attackActionRef;
    [SerializeField] private InputActionReference parryActionRef; // ⬅ 패링
    [SerializeField] private InputActionReference lanternActionRef; // ⬅ 랜턴
    [SerializeField] private InputActionReference dashActionRef; // ⬅ 대시
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction parryAction;
    private InputAction lanternAction;
    private InputAction dashAction;

    private float _baseScaleX;

    // === Ground 체크 ===
    [Header("Ground Sensor (정교 판정)")]
    [SerializeField] private LayerMask groundLayer;
    [Range(0f, 1f)] public float groundNormalMinY = 0.6f;
    private readonly ContactPoint2D[] _contactPts = new ContactPoint2D[8];

    // === 공격 판정(OverlapCircle) ===
    [Header("Attack Hit (OverlapCircle)")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange1 = 0.8f;
    [SerializeField] private float attackRange2 = 0.8f;
    [SerializeField] private LayerMask enemyLayers;

    [Header("Attack Damage")]
    public int attackDamage1 = 10;
    public int attackDamage2 = 14;

    private readonly HashSet<Collider2D> _swingHitCache = new HashSet<Collider2D>();
    [SerializeField] private bool debugAttack = false;

    // Animator 파라미터 캐시
    private static readonly int HashSpeedX = Animator.StringToHash("speedX");
    private static readonly int HashSpeedY = Animator.StringToHash("speedY");
    private static readonly int HashGround = Animator.StringToHash("grounded");
    private static readonly int HashHit = Animator.StringToHash("Hit");
    private bool _hasSpeedX, _hasSpeedY, _hasGround;
    private float _lastSpeedX, _lastSpeedY; private bool _lastGround;

    // Runtime
    public bool Grounded { get; private set; }
    public float XInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }

    // ===== Parry 설정 =====
    [Header("Parry")]
    public float parryGuardTime = 0.25f;   // 방어 창 길이(고정)
    public float parryRecover = 0.12f;   // 실패 경직
    public float parryCooldown = 0.30f;   // 재사용 대기
    public float parrySuccessIFrame = 0.12f; // 성공 직후 무적(다단히트 방지)

    [HideInInspector] public bool parryActive;       // 지금 방어 창 열려 있음
    [HideInInspector] public bool parrySuccess;      // 이번 창에서 성공했는가
    [HideInInspector] public float parryEndTime;      // 창 종료 시각
    private float _parryReadyTime;                    // 쿨다운 완료 시각
    private float _parrySuccessUntil;                 // 성공 무적 종료 시각

    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashTime = 0.18f;
    public float dashCooldown = 0.35f;
    public bool dashIFrame = true;

    // 런타임
    [HideInInspector] public PlayerDash_LSH dash;
    [HideInInspector] public bool isDashing;
    [HideInInspector] public float _dashReadyTime;

    [Header("Air Jump")]
    public int maxAirJumps = 1;    // 공중에서 가능한 추가 점프 수(= 더블점프면 1)
    public float coyoteTime = 0.10f; // 코요테타임(지면 이탈 후 짧게 점프 허용)
    public float jumpBuffer = 0.10f; // 점프 버퍼(조금 먼저 눌러도 허용)

    private int _airJumpsRemaining;
    private float _lastGroundedTime;
    private float _lastJumpPressedTime;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        fsm = new PlayerStateMachine_LSH();
        idle = new PlayerIdle_LSH(this, fsm);
        run = new PlayerRun_LSH(this, fsm);
        jump = new PlayerJump_LSH(this, fsm);
        fall = new PlayerFall_LSH(this, fsm);
        attack = new PlayerAttack_LSH(this, fsm);
        attackCombo = new PlayerAttackCombo_LSH(this, fsm);
        parry = new PlayerParry_LSH(this, fsm);   // ⬅ 추가
        dash = new PlayerDash_LSH(this, fsm);
        _baseScaleX = Mathf.Abs(transform.localScale.x);
        CacheAnimatorParams();

        currentHealth = maxHealth;
        currentLight = Mathf.Clamp(currentLight, 0, maxLight);
    }

    void OnEnable()
    {
        moveAction = moveActionRef ? moveActionRef.action : null;
        jumpAction = jumpActionRef ? jumpActionRef.action : null;
        attackAction = attackActionRef ? attackActionRef.action : null;
        parryAction = parryActionRef ? parryActionRef.action : null;
        lanternAction = lanternActionRef ? lanternActionRef.action : null;
        dashAction = dashActionRef ? dashActionRef.action : null;

        moveAction?.Enable();
        jumpAction?.Enable();
        attackAction?.Enable();
        parryAction?.Enable();
        lanternAction?.Enable();
        dashAction?.Enable();
    }

    void OnDisable()
    {
        lanternAction?.Disable();
        parryAction?.Disable();
        attackAction?.Disable();
        jumpAction?.Disable();
        moveAction?.Disable();
        dashAction?.Disable();
    }


    void Start()
    {
        fsm.Initialize(idle);
    }

    void Update()
    {
        // 입력
        XInput = ReadMoveX(moveAction);
        JumpPressed = jumpAction != null && jumpAction.WasPressedThisFrame();

        if (JumpPressed)
            _lastJumpPressedTime = Time.time;

        AttackPressed = attackAction != null && attackAction.WasPressedThisFrame();

        // 패링 입력 → 쿨다운 완료 시에만 진입
        if (parryAction != null && parryAction.WasPressedThisFrame() && Time.time >= _parryReadyTime)
        {
            fsm.ChangeState(parry);
        }

        if (lanternAction != null && lanternAction.WasPressedThisFrame())
        {
            lanternOn = !lanternOn;
            Debug.Log($"[Lantern] {(lanternOn ? "ON" : "OFF")}");
        }

        if (dashAction != null && dashAction.WasPressedThisFrame() && Time.time >= _dashReadyTime)
        {
            fsm.ChangeState(dash);
        }
        // 지면 체크

        Grounded = CheckGroundedPrecise();
        bool wasGrounded = _lastGround != Grounded;
        if (Grounded)
        {
            _lastGroundedTime = Time.time;
            _airJumpsRemaining = maxAirJumps; // 착지 시 에어점프 갱신
        }

        // FSM
        fsm.PlayerKeyInput();
        fsm.UpdateState();

        // Animator 파라미터
        if (animator)
        {
            float sx = Mathf.Abs(rb.linearVelocity.x);
            float sy = rb.linearVelocity.y;

            if (_hasSpeedX && !Mathf.Approximately(_lastSpeedX, sx))
            { animator.SetFloat(HashSpeedX, sx); _lastSpeedX = sx; }

            if (_hasSpeedY && !Mathf.Approximately(_lastSpeedY, sy))
            { animator.SetFloat(HashSpeedY, sy); _lastSpeedY = sy; }

            if (_hasGround && _lastGround != Grounded)
            { animator.SetBool(HashGround, Grounded); _lastGround = Grounded; }
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
            if (s.x < 0f) { s.x = _baseScaleX; transform.localScale = s; }
        }
        else if (x < -0.01f)
        {
            var s = transform.localScale;
            if (s.x > 0f) { s.x = -_baseScaleX; transform.localScale = s; }
        }
    }

    // === Animator Trigger (1프레임 뒤 자동 Reset) ===
    public void TriggerAttack() { SetAndAutoReset("Attack"); }
    public void TriggerAttack2() { SetAndAutoReset("Attack2"); }
    public void TriggerHit() { SetAndAutoReset("Hit"); }
    public void TriggerDie() { SetAndAutoReset("Die"); }

    private void SetAndAutoReset(string trig)
    {
        if (!animator) return;
        animator.ResetTrigger(trig);
        animator.SetTrigger(trig);
        StartCoroutine(ResetTriggerNextFrame(trig));
    }

    public IEnumerator ResetTriggerNextFrame(string trig)
    {
        yield return null; // 한 프레임 대기(소비 시간 확보)
        animator?.ResetTrigger(trig);
    }

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

    // ====== 공격 판정 (FSM에서 호출) ======
    public void AttackSwingBegin() => _swingHitCache.Clear();

    public void DoDamage_Public(int which)
    {
        if (!attackPoint) { Debug.LogWarning("[PlayerController_LSH] attackPoint 미지정"); return; }
        if (which == 1) DoDamage((Vector2)attackPoint.position, attackRange1, attackDamage1);
        else DoDamage((Vector2)attackPoint.position, attackRange2, attackDamage2);
    }

    private void DoDamage(Vector2 center, float radius, int damage)
    {
        var hits = Physics2D.OverlapCircleAll(center, radius, enemyLayers);
        if (debugAttack) Debug.Log($"[Attack] center:{center}, radius:{radius}, hits:{hits.Length}");

        foreach (var col in hits)
        {
            if (_swingHitCache.Contains(col)) continue;
            _swingHitCache.Add(col);

            var d1 = col.GetComponentInParent<IDamageable_LSH>();
            if (d1 != null) { d1.TakeDamage(damage, transform.position); continue; }

            var e1 = col.GetComponentInParent<Enemy_LSH>();
            if (e1 != null) { e1.TakeDamage(damage); continue; }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!attackPoint) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange1);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange2);
    }

    // ====== 데미지 입구 (패링/무적으로 차단) ======
    public void TakeDamage(int damage, Vector3 hitFrom)
    {
        // 패링 무적(있다면) 우선
        if (parryActive || Time.time < _parrySuccessUntil) return;

        // ⬇대시 무적(인스펙터에서 켜면 적용)
        if (dashIFrame && isDashing) return;

        currentHealth -= damage;
        Debug.Log($"[Player] Damaged! -{damage}, HP:{currentHealth}");

        if (currentHealth > 0) TriggerHit();
        else { TriggerDie(); enabled = false; }
    }

    // ====== IParryReceiver 구현 (패링 성공 시 처리) ======
    public bool TryParry(object attackSource, Vector3 hitPoint)
    {
        if (!parryActive) return false;

        parrySuccess = true;
        parryActive = false; // 창 닫기
        _parrySuccessUntil = Time.time + parrySuccessIFrame; // 성공 후 무적
        AddLight(parryLightGain);

        // 성공 연출
        animator?.ResetTrigger("Parry");
        animator?.SetTrigger("Parry");
        StartCoroutine(ResetTriggerNextFrame("Parry"));

        return true; // 이 공격 무효화
    }

    // 패링 종료 시 쿨다운 시작 (Parry 상태 Exit에서 호출)
    public void SetParryCooldown()
    {
        _parryReadyTime = Time.time + parryCooldown;
    }

    public bool CanAirJump() => _airJumpsRemaining > 0;

    public void DoAirJump()
    {
        // 잔량 소모하고 수직 속도 리셋 후 점프력 적용
        _airJumpsRemaining = Mathf.Max(0, _airJumpsRemaining - 1);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        // 점프 애니 트리거가 있으면 사용
        animator?.ResetTrigger("Jump");
        animator?.SetTrigger("Jump");
        StartCoroutine(ResetTriggerNextFrame("Jump"));
    }
}