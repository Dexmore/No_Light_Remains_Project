using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController_LSH : MonoBehaviour
{
    [Header("Player HP")]
    public int maxHealth = 1000;
    public int currentHealth;

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

    // === 공격 판정(OverlapCircle) ===
    [Header("Attack Hit (OverlapCircle)")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange1 = 0.8f;
    [SerializeField] private float attackRange2 = 0.8f;
    [SerializeField] private LayerMask enemyLayers;

    [Header("Attack Damage")]
    public int attackDamage1 = 10;
    public int attackDamage2 = 14;


    [Header("Hit / I-Frame")]
    public float hurtIFrame = 0.35f;
    private float _hurtFreeUntil = -999f;


    private readonly HashSet<Collider2D> _swingHitCache = new HashSet<Collider2D>();
    [SerializeField] private bool debugAttack = false;

    // Animator 파라미터 캐시
    private static readonly int HashSpeedX = Animator.StringToHash("speedX");
    private static readonly int HashSpeedY = Animator.StringToHash("speedY");
    private static readonly int HashGround = Animator.StringToHash("grounded");
    private bool _hasSpeedX, _hasSpeedY, _hasGround;
    private float _lastSpeedX, _lastSpeedY; private bool _lastGround;

    // Runtime
    public bool Grounded { get; private set; }
    public float XInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }

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

        _baseScaleX = Mathf.Abs(transform.localScale.x);

        CacheAnimatorParams();

        currentHealth = maxHealth;
    }

    void OnEnable()
    {
        if (moveActionRef == null || moveActionRef.action == null ||
            jumpActionRef == null || jumpActionRef.action == null)
        {
            Debug.LogError("[PlayerController_LSH] InputActionReference가 비었습니다. 인스펙터에서 Move/Jump를 할당하세요.");
            enabled = false;
            return;
        }

        if (attackActionRef == null || attackActionRef.action == null)
        {
            Debug.LogError("[PlayerController_LSH] Attack 액션이 비어 있습니다.");
            enabled = false;
            return;
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
        XInput = ReadMoveX(moveAction);
        JumpPressed = jumpAction.WasPressedThisFrame();
        AttackPressed = attackAction.WasPressedThisFrame();

        // 지면 체크
        Grounded = CheckGroundedPrecise();

        // FSM 흐름
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

    public void TriggerAttack()
    {
        if (!animator) return;
        animator.ResetTrigger("Attack"); // 혹시 이전에 남아있던 거 정리
        animator.SetTrigger("Attack");
        StartCoroutine(ResetTriggerNextFrame("Attack"));
    }

    public void TriggerAttack2()
    {
        if (!animator) return;
        animator.ResetTrigger("Attack2");
        animator.SetTrigger("Attack2");
        StartCoroutine(ResetTriggerNextFrame("Attack2"));
    }

    public void TriggerHit()
    {
        if (!animator) return;
        animator.ResetTrigger("Hit");
        animator.SetTrigger("Hit");
        StartCoroutine(ResetTriggerNextFrame("Hit"));
    }

    public void TriggerDie()
    {
        if (!animator) return;
        animator.ResetTrigger("Die");
        animator.SetTrigger("Die");
        StartCoroutine(ResetTriggerNextFrame("Die"));
    }

    private IEnumerator ResetTriggerNextFrame(string triggerName)
    {
        yield return null; // 한 프레임 기다림
        animator.ResetTrigger(triggerName);
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

    // ====== 공격 판정 유틸 (FSM에서 호출) ======
    public void AttackSwingBegin() => _swingHitCache.Clear();

    public void DoDamage_Public(int which)
    {
        if (!attackPoint) { Debug.LogWarning("[PlayerController_LSH] attackPoint 미지정"); return; }
        if (which == 1) DoDamage(attackPoint.position, attackRange1, attackDamage1);
        else DoDamage(attackPoint.position, attackRange2, attackDamage2);
    }

    public void DebugAttack(string msg) { if (debugAttack) Debug.Log(msg); }

    private void DoDamage(Vector2 center, float radius, int damage)
    {
        var hits = Physics2D.OverlapCircleAll(center, radius, enemyLayers);
        Debug.Log($"[Attack] center:{center}, radius:{radius}, hits:{hits.Length}");

        foreach (var h in hits)
        {
            Debug.Log($"[Attack] Hit object: {h.name}, layer:{LayerMask.LayerToName(h.gameObject.layer)}");
        }

        foreach (var col in hits)
        {
            if (_swingHitCache.Contains(col)) continue;
            _swingHitCache.Add(col);

            // 인터페이스 우선 (부모/자식 모두 탐색)
            var d1 = col.GetComponentInParent<IDamageable_LSH>();
            if (d1 != null) { d1.TakeDamage(damage, transform.position); continue; }
            var d2 = col.GetComponentInChildren<IDamageable_LSH>();
            if (d2 != null) { d2.TakeDamage(damage, transform.position); continue; }

            // Enemy_LSH도 지원 (부모/자식)
            var e1 = col.GetComponentInParent<Enemy_LSH>();
            if (e1 != null) { e1.TakeDamage(damage); continue; }
            var e2 = col.GetComponentInChildren<Enemy_LSH>();
            if (e2 != null) { e2.TakeDamage(damage); continue; }

            if (debugAttack) Debug.Log($"[Attack] 맞췄지만 IDamageable_LSH/Enemy_LSH 없음: {col.name}");
        }
    }
    public void TakeDamage(int damage, Vector3 hitFrom)
    {
        if (Time.time < _hurtFreeUntil) return; // 무적 중이면 무시

        currentHealth -= damage;
        _hurtFreeUntil = Time.time + hurtIFrame;

        Debug.Log($"[Player] Damaged! -{damage}, HP:{currentHealth}");

        if (currentHealth > 0) TriggerHit();
        else { TriggerDie(); enabled = false; }
    }
    private void OnDrawGizmosSelected()
    {
        if (!attackPoint) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange1);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange2);
    }
}
