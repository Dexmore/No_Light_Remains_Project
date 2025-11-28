using UnityEngine;

public class PlayerDie : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerDie(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    LightSystem lightSystem;
    public void Enter()
    {
        ctx.Dead = true;
        lightSystem = ctx.GetComponentInChildren<LightSystem>(true);
        ctx.animator.Play("Player_Die");
        GameObject light0 = lightSystem.transform.GetChild(0).gameObject;
        GameObject light1 = lightSystem.transform.GetChild(1).gameObject;
        light0.SetActive(false);
        light1.SetActive(false);
        DBManager.I.isLanternOn = false;
    }
    public void Exit()
    {

    }
    public void UpdateState()
    {

    }
    public void UpdatePhysics()
    {

    }
}
