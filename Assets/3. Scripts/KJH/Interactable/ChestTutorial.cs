using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class ChestTutorial : MonoBehaviour
{
    public ChestInteractable_LSH chest;
    PlayerControl playerControl;
    UnityEngine.UI.Text keyText;
    void Awake()
    {
        playerControl = FindAnyObjectByType<PlayerControl>();
        keyText = transform.Find("Canvas/Wrap/Key/Text").GetComponent<UnityEngine.UI.Text>();
        transform.Find("Canvas").gameObject.SetActive(false);
        keyText.text = SettingManager.I.GetBindingName("Interaction");
    }
    public void Stop()
    {
        transform.Find("Canvas").gameObject.SetActive(true);
        playerControl.fsm.ChangeState(playerControl.stop);
        playerControl.stop.duration = 5f;
    }
    public async void ForceRun()
    {
        playerControl.fsm.ChangeState(playerControl.stop);
        playerControl.stop.duration = 5f;
        if (chest.isReady)
            chest.Run();
        await Task.Delay(200);
        transform.Find("Canvas").gameObject.SetActive(false);
        await Task.Delay(500);
        playerControl.fsm.ChangeState(playerControl.idle);
    }

}
