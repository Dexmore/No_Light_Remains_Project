using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerFall : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerFall(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private InputAction moveAction;
    Vector2 moveActionValue;
    public void Enter()
    {
        if (moveAction == null)
            moveAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Move");
        ctx.animator.Play("Player_Fall");
        startTime = Time.time;
        //;
    }
    float startTime;
    public void Exit()
    {

    }
    public void UpdateState()
    {
        moveActionValue = moveAction.ReadValue<Vector2>();
        moveActionValue.y = 0f;
        if(Time.time - startTime > 0.1f && ctx.fallThroughPlatform)
        {
            ctx.fallThroughPlatform = false;
        }
        if (ctx.Grounded)
        {
            if (Mathf.Abs(moveActionValue.x) > 0.01f)
                fsm.ChangeState(ctx.run);
            else
                fsm.ChangeState(ctx.idle);

            SFX sfx;
            float vol = Time.time - startTime;
            if (vol > 0.2f)
            {
                vol = Mathf.Clamp01(vol - 0.3f) * 0.4f;
                sfx = AudioManager.I.PlaySFX("Land");
                if (sfx != null)
                    if (sfx.aus != null)
                        sfx.aus.volume = vol * sfx.aus.volume;
            }
        }
    }
    public void UpdatePhysics()
    {
        ctx.rb.AddForceY(-16f);
        if (Time.time - startTime > 0.4f)
        {
            float _time = Time.time - startTime - 0.4f;
            _time = Mathf.Clamp(_time, 0f, 10f);
            ctx.rb.AddForce(Vector2.down * _time * 0.44f, ForceMode2D.Impulse);
        }

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
