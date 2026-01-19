using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
public class EndingCreditPotal : Interactable
{
    public override Type type => Type.Normal;
    public override bool isReady { get; set; } = true;
    public override bool isAuto => false;
    public string sceneName;
    void Awake()
    {
        isReady = true;
    }
    public override async void Run()
    {
        if (GameManager.I.isOpenDialog || GameManager.I.isOpenPop || GameManager.I.isOpenInventory) return;

        isReady = false;
        transform.SetParent(null);
        //AudioManager.I.PlaySFX("DoorOpen2");
        await Task.Delay(200);
        GameManager.I.LoadSceneAsync(sceneName);
    }


}
