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
    private InputAction lanternAction;

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
    [HideInInspector] public PlayerOpenInventory_LSH openInventory;


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
        lightSystem = GetComponentInChildren<LightSystem>(true);

        fsm = new PlayerStateMachine_LSH();
        idle = new PlayerIdle_LSH(this, fsm);
        run = new PlayerRun_LSH(this, fsm);
        jump = new PlayerJump_LSH(this, fsm);
        fall = new PlayerFall_LSH(this, fsm);
        attack = new PlayerAttack_LSH(this, fsm);
        attackCombo = new PlayerAttackCombo_LSH(this, fsm);
        parry = new PlayerParry_LSH(this, fsm);
        dash = new PlayerDash_LSH(this, fsm);
        hit = new PlayerHit_LSH(this, fsm);
        die = new PlayerDie_LSH(this, fsm);
        usePotion = new PlayerUsePotion_LSH(this, fsm);
        openInventory = new PlayerOpenInventory_LSH(this, fsm);

    }

    void OnEnable()
    {
        fsm.ChangeState(idle);
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").performed += DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").performed += DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").canceled += DashInputCancel;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").canceled += DashInputCancel;
        lanternAction = inputActionAsset.FindActionMap("Player").FindAction("Lantern");
        lanternAction.performed += LanternInput;
        GameManager.I.onHit += HitHandler;

    }
    void OnDisable()
    {
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").performed -= DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").performed -= DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").canceled -= DashInputCancel;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").canceled -= DashInputCancel;
        lanternAction.performed -= LanternInput;
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
        if (data.attackType == HitData.AttackType.Chafe)
        {
            if (isHit2) return;
            if (isHit1) return;
            isHit1 = true;
            run.isStagger = true;
            StopCoroutine(nameof(HitCoolTime1));
            StartCoroutine(nameof(HitCoolTime1));
            if (Avoided)
            {
                Debug.Log("회피 성공");
                return;
            }
            Vector2 dir = 4.2f * (data.target.position.x - data.attacker.position.x) * Vector2.right;
            dir.y = 2f;
            Vector3 velo = rb.linearVelocity;
            rb.linearVelocity = 0.4f * velo;
            rb.AddForce(dir, ForceMode2D.Impulse);
            currentHealth -= (int)data.damage;
            if (currentHealth <= 0)
                fsm.ChangeState(die);
            return;
        }
        else if (data.attackType == HitData.AttackType.Default)
        {
            if (isHit2) return;
            isHit2 = true;
            StopCoroutine(nameof(HitCoolTime2));
            StartCoroutine(nameof(HitCoolTime2));
            if (Avoided)
            {
                Debug.Log("회피 성공");
                return;
            }
            if (Parred)
            {
                AudioManager.I.PlaySFX("Parry");
                Debug.Log("패링 성공");
                return;
            }
            float multiplier = 1f;
            switch (data.staggerType)
            {
                case HitData.StaggerType.Small:
                    multiplier = 1.1f;
                    break;
                case HitData.StaggerType.Middle:
                    multiplier = 1.25f;
                    break;
                case HitData.StaggerType.Large:
                    multiplier = 1.4f;
                    break;
            }
            Vector2 dir = 3.5f * multiplier * (data.target.position.x - data.attacker.position.x) * Vector2.right;
            dir.y = 2.3f * Mathf.Sqrt(multiplier) + (multiplier - 1f);
            Vector3 velo = rb.linearVelocity;
            rb.linearVelocity = 0.4f * velo;
            rb.AddForce(dir, ForceMode2D.Impulse);
            if (data.staggerType != HitData.StaggerType.None)
            {
                hit.staggerType = data.staggerType;
                fsm.ChangeState(hit);
            }
            currentHealth -= (int)data.damage;
            if (currentHealth <= 0)
                fsm.ChangeState(die);
            ParticleManager.I.PlayParticle("Hit2", data.hitPoint, Quaternion.identity, null);
            AudioManager.I.PlaySFX("Hit8Bit", data.hitPoint, null);
            return;
        }
    }
    bool isHit1;
    bool isHit2;
    IEnumerator HitCoolTime1()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.2f);
        run.isStagger = false;
        yield return YieldInstructionCache.WaitForSeconds(1.6f - 0.2f);
        isHit1 = false;
    }
    IEnumerator HitCoolTime2()
    {
        yield return YieldInstructionCache.WaitForSeconds(2f);
        isHit2 = false;
    }
    #region Use Lantern
    LightSystem lightSystem;
    void LanternInput(InputAction.CallbackContext callback)
    {
        GameObject light0 = lightSystem.transform.GetChild(0).gameObject;
        GameObject light1 = lightSystem.transform.GetChild(1).gameObject;
        if (light0.activeSelf)
        {
            light0.SetActive(false);
            light1.SetActive(false);
        }
        else
        {
            light0.SetActive(true);
            light1.SetActive(true);
        }
    }

    #endregion
    #region Use Potion

    #endregion
    #region Open Inventory

    #endregion




















































}
