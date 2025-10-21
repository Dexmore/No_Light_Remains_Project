using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerFall_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerFall_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
    private InputAction moveAction;
    Vector2 moveActionValue;
    public void Enter()
    {
        if (moveAction == null)
            moveAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Move");

        ctx.animator.Play("Player_Fall");
    }
    public void Exit()
    {

    }
    public void UpdateState()
    {
        moveActionValue = moveAction.ReadValue<Vector2>();
        moveActionValue.y = 0f;
        if (ctx.Grounded)
        {
            if (Mathf.Abs(moveActionValue.x) > 0.01f)
                fsm.ChangeState(ctx.run);
            else
                fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {
        // 유승훈 기획님 요청으로 Fall상태에서 더 빨리 낙하했으면 좋겠다고 해서 추가
        ctx.rb.AddForceY(-13f);

        // 아래는 낙하중에 동시에 이동 처리

        // 1. 캐릭터 좌우 바라보는 방향 변경
        if (moveActionValue.x > 0 && ctx.childTR.right.x < 0)
            ctx.childTR.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (moveActionValue.x < 0 && ctx.childTR.right.x > 0)
            ctx.childTR.localRotation = Quaternion.Euler(0f, 180f, 0f);
        // 2. 공중에서 벽으로 전진하면 벽에 붙어있는 버그방지
        bool isWallClose = false;
        if (ctx.collisions.Count > 0)
            foreach (var element in ctx.collisions)
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
        // 3. AddForce방식으로 캐릭터 이동
        float dot = Vector2.Dot(ctx.rb.linearVelocity, moveActionValue);
        //float speedInAir = ctx.Grounded ? ctx.moveSpeed : ctx.moveSpeed * ctx.airMoveMultiplier;
        if (!isWallClose)
            if (dot < ctx.moveSpeed)
            {
                // 공중이므로 기존 이동보다 ctx.airMoveMultiplier 만큼 감속
                float multiplier = ctx.airMoveMultiplier * 0.5f * ((ctx.moveSpeed - dot) + 1f);
                ctx.rb.AddForce(multiplier * moveActionValue * (ctx.moveSpeed + 4.905f) / 1.25f);
            }


    }
}
