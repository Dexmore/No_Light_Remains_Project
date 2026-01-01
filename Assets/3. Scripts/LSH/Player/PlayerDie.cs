using UnityEngine;

public class PlayerDie : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerDie(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    PlayerLight PlayerLight;
    public void Enter()
    {
        ctx.Dead = true;
        PlayerLight = ctx.GetComponentInChildren<PlayerLight>(true);
        ctx.animator.Play("Player_Die");
        GameObject light0 = PlayerLight.transform.GetChild(0).gameObject;
        GameObject light1 = PlayerLight.transform.GetChild(1).gameObject;
        GameObject light2 = PlayerLight.transform.GetChild(2).gameObject;
        light0.SetActive(false);
        light1.SetActive(false);
        light2.SetActive(true);
        GameManager.I.isLanternOn = false;
        DBManager.I.currData.death++;
        DBManager.I.savedData.death++;
        
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
