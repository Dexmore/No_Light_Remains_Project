using UnityEngine;

public class PlayerDie_LSH : IPlayerState_LSH
{
    private readonly PlayerController_LSH ctx;
    private readonly PlayerStateMachine_LSH fsm;
    public PlayerDie_LSH(PlayerController_LSH ctx, PlayerStateMachine_LSH fsm) { this.ctx = ctx; this.fsm = fsm; }
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
