using UnityEngine;
public class PlayerOpenESCMenu : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerOpenESCMenu(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    float startTime = 0f;
    PopupControl popupControl;
    public void Enter()
    {
        startTime = Time.time;
        if (popupControl == null)
            popupControl = GameManager.I.transform.GetComponent<PopupControl>();
        if (popupControl != null)
        {
            popupControl.OpenPop(1);
        }
    }
    public void Exit()
    {

    }
    public void UpdateState()
    {
        if (Time.time - startTime > 1f && !GameManager.I.isOpenPop)
        {
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }

}
