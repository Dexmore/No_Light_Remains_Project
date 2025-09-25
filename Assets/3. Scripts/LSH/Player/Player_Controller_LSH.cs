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
        inputActionAsset.FindActionMap("Player").FindAction("Jump").performed += JumpInput;
    }
    void OnDisable()
    {
        inputActionAsset.FindActionMap("Player").FindAction("Jump").performed -= JumpInput;
    }
    [HideInInspector] public bool isJump;
    void JumpInput(InputAction.CallbackContext callback)
    {
        if (!isGround) return;
        if (state != State.Idle && state != State.Run) return;
        if (!isJump)
        {
            isJump = true;
            StartCoroutine(nameof(Jump));
        }
    }
    IEnumerator Jump()
    {
        animator.Play("Player_Jump");
        yield return null;
        rb.AddForce(Vector2.up * jumpForce * 50);
        yield return YieldInstructionCache.WaitForSeconds(0.1f);
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
}
