using UnityEngine;
public class PlayerStop : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerStop(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    float elapsed = 0f;
    public float duration;
    public void Enter()
    {
        elapsed = 0;
        ctx.animator.Play("Player_Idle");
        // ACH
        int count1 = DBManager.I.itemDatabase.allGears.Count;
        int count2 = DBManager.I.currData.gearDatas.Count;
        if(count1 == count2)
        {
            DBManager.I.SteamAchievement("ACH_GEAR_COLLECT_ALL");
        }
    }
    public void Exit()
    {
        elapsed = 0;
        duration = 0;
    }
    public void UpdateState()
    {
        elapsed += Time.deltaTime;
        if (elapsed < 1f) return;
        if (elapsed > duration && !GameManager.I.isOpenPop && !GameManager.I.isOpenDialog)
        {
            elapsed = 0;
            duration = 0;
            fsm.ChangeState(ctx.idle);
        }
    }
    public void UpdatePhysics()
    {

    }
}
