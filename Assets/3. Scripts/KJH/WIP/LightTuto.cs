using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class LightTuto : MonoBehaviour
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
    public async void ForceRun()
    {
        transform.Find("Canvas").gameObject.SetActive(true);
        playerControl.fsm.ChangeState(playerControl.stop);
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
        playerControl.fsm.ChangeState(playerControl.idle);
        lineRenderer.enabled = false;
        transform.Find("Canvas").gameObject.SetActive(false);
    }

}
