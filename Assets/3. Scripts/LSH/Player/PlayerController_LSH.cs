using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController_LSH : MonoBehaviour
{

    [Header("Player HP")]
    public int maxHealth = 1000;
    public int currentHealth;

    // [Header("Light Resource")]
    // public int maxLight = 100;
    // public int currentLight = 50;
    // public int parryLightGain = 15;

    [Header("Move")]
    public float moveSpeed = 6f;
    public float airMoveMultiplier = 0.85f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Input (use bound actions)")]
    public InputActionAsset inputActionAsset;
    private InputAction parryAction; // ⬅ 패링
    private InputAction lanternAction; // ⬅ 랜턴
    private InputAction interactAction;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Transform childTR;
    [HideInInspector] public AttackRange attackRange;
    [HideInInspector] public PlayerStateMachine_LSH fsm;
    // States
    [HideInInspector] public PlayerIdle_LSH idle;
    [HideInInspector] public PlayerRun_LSH run;
    [HideInInspector] public PlayerJump_LSH jump;
    [HideInInspector] public PlayerFall_LSH fall;
    [HideInInspector] public PlayerAttack_LSH attack;
    [HideInInspector] public PlayerAttackCombo_LSH attackCombo;
    [HideInInspector] public PlayerDash_LSH dash;
    [HideInInspector] public PlayerParry_LSH parry;
    [HideInInspector] public PlayerHit_LSH hit;
    [HideInInspector] public PlayerDie_LSH die;
    [HideInInspector] public PlayerJumpAttack_LSH jumpAttack;
    [HideInInspector] public PlayerUsePotion_LSH usePotion;
    [HideInInspector] public PlayerOpenGear_LSH openGear;
    

    // === Ground 체크 ===
    [Header("Ground Sensor (정교 판정)")]
    [SerializeField] private LayerMask groundLayer;
    CapsuleCollider2D capsuleCollider2D;
    [HideInInspector] public float height;
    [HideInInspector] public float width;
    private readonly ContactPoint2D[] _contactPts = new ContactPoint2D[8];
    [HideInInspector] public Dictionary<Collider2D, Vector2> contactPts = new Dictionary<Collider2D, Vector2>();
    [HideInInspector] public Dictionary<Collider2D, Vector2> collisions = new Dictionary<Collider2D, Vector2>();

    // Runtime
    [ReadOnlyInspector] public bool Grounded { get; private set; }
    [ReadOnlyInspector] public bool Parred { get; set; }
    [ReadOnlyInspector] public bool Avoided { get; set; }

    void Awake()
    {
        TryGetComponent(out rb);
        childTR = transform.GetChild(0);
        animator = GetComponentInChildren<Animator>(true);
        capsuleCollider2D = GetComponentInChildren<CapsuleCollider2D>(true);
        attackRange = GetComponentInChildren<AttackRange>(true);
        height = capsuleCollider2D.size.y;
        width = capsuleCollider2D.size.x;

        fsm = new PlayerStateMachine_LSH();
        idle = new PlayerIdle_LSH(this, fsm);
        run = new PlayerRun_LSH(this, fsm);
        jump = new PlayerJump_LSH(this, fsm);
        fall = new PlayerFall_LSH(this, fsm);
        attack = new PlayerAttack_LSH(this, fsm);
        attackCombo = new PlayerAttackCombo_LSH(this, fsm);
        parry = new PlayerParry_LSH(this, fsm);
        dash = new PlayerDash_LSH(this, fsm);

    }

    void OnEnable()
    {
        fsm.ChangeState(idle);
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").performed += DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").performed += DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").canceled += DashInputCancel;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").canceled += DashInputCancel;
        GameManager.I.onHit += HitHandler;

    }
    void OnDisable()
    {
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").performed -= DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").performed -= DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").canceled -= DashInputCancel;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").canceled -= DashInputCancel;
    }

    void Update()
    {
        fsm.Update();
        CheckGroundedPrecise();
    }

    void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

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
    void CheckGroundedPrecise()
    {
        Grounded = false;
        if (collisions.Count > 0)
            foreach (var element in collisions)
                if (Mathf.Abs(element.Value.y - transform.position.y) < 0.1f * capsuleCollider2D.size.y)
                {
                    Grounded = true;
                    break;
                }
    }

    #region Dash
    int leftDashInputCount = 0;
    int rightDashInputCount = 0;
    void DashInput(InputAction.CallbackContext callback)
    {
        if (!Grounded) return;
        if (fsm.currentState == dash) return;
        if (callback.action.name == "LeftDash")
        {
            if (leftDashInputCount == 0)
            {
                leftDashInputCount = 1;
                if (rightDashInputCount != 0) rightDashInputCount = 0;
                StopCoroutine(nameof(DashRelease));
                StartCoroutine(nameof(DashRelease));
            }
            else if (leftDashInputCount == 2)
            {
                if (rightDashInputCount != 0) rightDashInputCount = 0;
                dash.isLeft = true;
                StopCoroutine(nameof(Dash));
                StartCoroutine(nameof(Dash));
            }
        }
        else if (callback.action.name == "RightDash")
        {
            if (rightDashInputCount == 0)
            {
                rightDashInputCount = 1;
                if (leftDashInputCount != 0) leftDashInputCount = 0;
                StopCoroutine(nameof(DashRelease));
                StartCoroutine(nameof(DashRelease));
            }
            else if (rightDashInputCount == 2)
            {
                if (leftDashInputCount != 0) leftDashInputCount = 0;
                dash.isLeft = false;
                StopCoroutine(nameof(Dash));
                StartCoroutine(nameof(Dash));
            }
        }
    }
    bool isDash;
    void DashInputCancel(InputAction.CallbackContext callback)
    {
        if (!Grounded) return;
        if (leftDashInputCount == 1)
            leftDashInputCount = 2;
        if (rightDashInputCount == 1)
            rightDashInputCount = 2;
    }
    IEnumerator DashRelease()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.23f);
        leftDashInputCount = 0;
        rightDashInputCount = 0;
    }
    IEnumerator Dash()
    {
        float time = Time.time;
        while (Time.time - time < 0.55f)
        {
            yield return null;
            if (fsm.currentState == dash) break;
            if (fsm.currentState == idle || fsm.currentState == run)
            {
                fsm.ChangeState(dash);
                break;
            }
        }
        yield return null;
        leftDashInputCount = 0;
        rightDashInputCount = 0;
    }
    #endregion

    void HitHandler(HitData data)
    {
        if (data.target.Root() != transform) return;
        if (fsm.currentState == die) return;
        currentHealth -= (int)data.damage;
        if (currentHealth <= 0)
            fsm.ChangeState(die);
    }





















































}
