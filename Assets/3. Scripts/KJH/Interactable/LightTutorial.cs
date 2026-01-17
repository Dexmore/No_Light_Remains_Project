using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class LightTutorial : MonoBehaviour
{
    public SconceLight sconceLight;
    SFX sfxLanternInteraction;
    PlayerControl playerControl;
    //PlayerInteraction playerInteraction;
    LineRenderer lineRenderer;
    UnityEngine.UI.Text keyText;
    void Awake()
    {
        playerControl = FindAnyObjectByType<PlayerControl>();
        //playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        lineRenderer = GetComponent<LineRenderer>();
        keyText = transform.Find("Canvas/Wrap/Key/Text").GetComponent<UnityEngine.UI.Text>();
        transform.Find("Canvas").gameObject.SetActive(false);
        keyText.text = SettingManager.I.GetBindingName("Lantern");
    }
    public void Stop()
    {
        playerControl.fsm.ChangeState(playerControl.stop);
        playerControl.stop.duration = 5f;
    }
    public async void ForceRun()
    {
        transform.Find("Canvas").gameObject.SetActive(true);
        playerControl.fsm.ChangeState(playerControl.stop);
        playerControl.stop.duration = 5f;
        sfxLanternInteraction = AudioManager.I.PlaySFX("ElectricityUsing");
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, playerControl.transform.position + Vector3.up);
        //playerInteraction.target2 = sconceLight;
        //playerInteraction.LanternAnimationStart(playerInteraction.cts.Token).Forget();
        await Task.Delay(900);
        if (sconceLight.isReady)
            sconceLight.Run();
        sfxLanternInteraction?.Despawn();
        sfxLanternInteraction = null;
        lineRenderer.enabled = false;
        transform.Find("Canvas").gameObject.SetActive(false);
        await Task.Delay(500);
        playerControl.fsm.ChangeState(playerControl.idle);
    }

}
