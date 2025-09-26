using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController_LSH : MonoBehaviour
{
    public float height = 1.5f;
    public float width = 0.7f;
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
        fsm.Update();
        isGround = false;
        if (collisions.Count > 0)
            foreach (var element in collisions)
                if (Mathf.Abs(element.Value.y - transform.position.y) < 0.09f * height)
                {
                    isGround = true;
                    break;
                }
    }
    void FixedUpdate()
    {
        fsm.FixedUpdate();
    }
    void OnEnable()
    {
        inputActionAsset.FindActionMap("Player").FindAction("Jump").performed += Input_Jump;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").performed += Input_LeftDash;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").canceled += InputCancel_LeftDash;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").performed += Input_RightDash;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").canceled += InputCancel_RightDash;
        inputActionAsset.FindActionMap("Player").FindAction("Parry").performed += Input_Parry;
        inputActionAsset.FindActionMap("Player").FindAction("Parry").canceled += InputCancel_Parry;
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
        Debug.Log($"jump, {state}");
        rb.AddForce(Vector2.up * jumpForce * 50);
        yield return YieldInstructionCache.WaitForSeconds(0.05f);
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
