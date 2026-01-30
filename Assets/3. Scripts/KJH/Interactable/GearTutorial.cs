using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class GearTutorial : MonoBehaviour
{
    public ChestInteractable_LSH chest;
    PlayerControl playerControl;
    void Awake()
    {
        playerControl = FindAnyObjectByType<PlayerControl>();
    }
    public void Stop()
    {
        playerControl.fsm.ChangeState(playerControl.stop);
        playerControl.stop.duration = 5f;
        if (chest.isReady)
            chest.Run();
    }
    public async void After()
    {
        playerControl.fsm.ChangeState(playerControl.stop);
        playerControl.stop.duration = 5f;
        DBManager.I.SteamAchievement("ACH_FIRST_GEAR_GET");
        await Task.Delay(600);
        playerControl.fsm.ChangeState(playerControl.idle);
    }

}
