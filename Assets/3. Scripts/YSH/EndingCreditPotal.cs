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

        //if (sceneName == "EndingCredit")
        //{
            //Debug.Log("aaa");
            DBManager.I.currData.sceneName = "Stage5";
            DBManager.I.savedData.sceneName = "Stage5";
            DBManager.I.currData.lastPos = new Vector2(-18, 2.05f);
            DBManager.I.savedData.lastPos = new Vector2(-18, 2.05f);
            DBManager.I.Save();
            // transform.SetParent(null);
            // DontDestroyOnLoad(gameObject);
            // await Task.Delay(100);
        //}

        await Task.Delay(200);
        GameManager.I.LoadSceneAsync(sceneName);
    }


}
