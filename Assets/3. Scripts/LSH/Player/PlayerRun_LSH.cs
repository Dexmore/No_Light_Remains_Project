using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerRun_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerRun_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    public void Enter()
    {
        inputAction_Move = ctx.inputActionAsset.FindActionMap("Player").FindAction("Move");
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Move").canceled += MoveInputCancel;
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed += Input_Attack;
        ctx.state = PlayerController_LSH.State.Run;
        isAnimation = false;
    }
    bool isAnimation;
    public void Update()
    {
        direction = inputAction_Move.ReadValue<Vector2>();
    }
    public void FixedUpdate()
    {
        float dot = Vector2.Dot(ctx.rb.linearVelocity, direction);
        // 캐릭터 좌우 방향 설정
        if (direction.x > 0 && ctx.model.right.x < 0)
        {
            ctx.model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (direction.x < 0 && ctx.model.right.x > 0)
        {
            ctx.model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
        // 벽 향해서 전진하는 버그 막기
        bool stopWall = false;
        if (ctx.collisions.Count > 0)
        {
            foreach (var element in ctx.collisions)
            {
                if (Mathf.Abs(element.Value.y - ctx.transform.position.y) >= 0.09f * ctx.height)
                {
                    if (element.Value.x - ctx.transform.position.x > 0.25f * ctx.width && direction.x > 0)
                    {
                        stopWall = true;
                        break;
                    }
                    else if (element.Value.x - ctx.transform.position.x < -0.25f * ctx.width && direction.x < 0)
                    {
                        stopWall = true;
                        break;
                    }
                }
            }
        }
        // AddForce방식으로 캐릭터 이동
        if (!stopWall)
            if (dot < ctx.moveSpeed)
            {
                float multiplier = (ctx.moveSpeed - dot) + 1f;
                ctx.rb.AddForce(multiplier * direction * (ctx.moveSpeed + 4.905f) / 1.25f);
                // 애니매이션처리
                if (ctx.isGround)
                {
                    if (!isAnimation)
                        if (ctx.isGround)
                        {
                            isAnimation = true;
                            ctx.animator.Play("Player_Run");
                        }
                    if (!ctx.animator.GetCurrentAnimatorStateInfo(0).IsName("Player_Run"))
                    {
                        isAnimation = true;
                        ctx.animator.Play("Player_Run");
                    }
                }
                else if (isAnimation)
                {
                    isAnimation = false;
                    if (ctx.isJump)
                    {
                        if (!ctx.animator.GetCurrentAnimatorStateInfo(0).IsName("Player_Jump"))
                        {
                            ctx.animator.Play("Player_Jump");
                        }
                    }
                    else
                    {
                        if (!ctx.animator.GetCurrentAnimatorStateInfo(0).IsName("Player_Fall"))
                        {
                            ctx.animator.Play("Player_Fall");
                        }
                    }
                }
            }
    }
    public void Exit()
    {
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Move").canceled -= MoveInputCancel;
        ctx.inputActionAsset.FindActionMap("Player").FindAction("Attack").performed -= Input_Attack;
    }
    InputAction inputAction_Move;
    Vector2 direction = Vector2.zero;
    void MoveInputCancel(InputAction.CallbackContext callback)
    {
        fsm.ChangeState(ctx.idle);
    }
    void Input_Attack(InputAction.CallbackContext callback)
    {
        if (ctx.isGround)
        {
            fsm.ChangeState(ctx.attack);
        }
    }

}
