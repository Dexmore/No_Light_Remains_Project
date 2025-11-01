using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerRun_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerRun_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    Vector2 moveActionValue;
    bool jumpPressed;
    bool attackPressed;
    public void Enter()
    {
        if (moveAction == null)
            moveAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Move");
        if (jumpAction == null)
            jumpAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Jump");
        if (attackAction == null)
            attackAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack");
        ctx.animator.Play("Player_Run");
    }
    public void Exit()
    {

    }
    public void UpdateState()
    {
        moveActionValue = moveAction.ReadValue<Vector2>();
        moveActionValue.y = 0f;
        if (moveActionValue.x == 0)
            fsm.ChangeState(ctx.idle);

        jumpPressed = jumpAction.IsPressed();
        if (jumpPressed && ctx.Grounded)
            fsm.ChangeState(ctx.jump);

        attackPressed = attackAction.IsPressed();
        if (attackPressed && ctx.Grounded)
            fsm.ChangeState(ctx.attack);

        if (!ctx.Grounded && ctx.rb.linearVelocity.y < -0.1f)
            fsm.ChangeState(ctx.fall);
    }
    public void UpdatePhysics()
    {
        if (isStagger) return;
        // 1. 캐릭터 좌우 바라보는 방향 변경
        if (moveActionValue.x > 0 && ctx.childTR.right.x < 0)
            ctx.childTR.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (moveActionValue.x < 0 && ctx.childTR.right.x > 0)
            ctx.childTR.localRotation = Quaternion.Euler(0f, 180f, 0f);
        // 2. 공중에서 벽으로 전진하면 벽에 붙어있는 버그방지
        bool isWallClose = false;
        if (ctx.collisions.Count > 0)
        {
            foreach (var element in ctx.collisions)
            {
                if (Mathf.Abs(element.Value.y - ctx.transform.position.y) >= 0.09f * ctx.height)
                {
                    if (element.Value.x - ctx.transform.position.x > 0.25f * ctx.width && moveActionValue.x > 0)
                    {
                        isWallClose = true;
                        break;
                    }
                    else if (element.Value.x - ctx.transform.position.x < -0.25f * ctx.width && moveActionValue.x < 0)
                    {
                        isWallClose = true;
                        break;
                    }
                }
            }
        }
        // 3. AddForce방식으로 캐릭터 이동
        float dot = Vector2.Dot(ctx.rb.linearVelocity, moveActionValue);
        //float speedInAir = ctx.Grounded ? ctx.moveSpeed : ctx.moveSpeed * ctx.airMoveMultiplier;
        if (!isWallClose)
            if (dot < ctx.moveSpeed)
            {
                float multiplier = (ctx.moveSpeed - dot) + 1f;
                ctx.rb.AddForce(multiplier * moveActionValue * (ctx.moveSpeed + 4.905f) / 1.25f);
            }
    }
    [HideInInspector] public bool isStagger = false;
}
