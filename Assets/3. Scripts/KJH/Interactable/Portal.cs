using System.Threading.Tasks;
using UnityEngine;
public class Portal : Interactable
{
    public override Type type => Type.Portal;
    public override bool isAuto => true;
    public override bool isReady { get; set; }
    public bool loadingPage = false;
    [SerializeField] string sceneName;
    [SerializeField] Vector2 targetPosition;
    [SerializeField] Direction direction;
    [SerializeField] string sfxName;
    [System.Serializable]
    public enum Direction
    {
        right,
        left,
    }
    bool isRun = false;
    void Start()
    {
        isReady = true;
        isRun = false;
    }
    public async override void Run()
    {
        if (!isReady) return;
        if (isRun) return;
        isRun = true;
        if (sfxName != "" && sceneName != null)
        {
            Debug.Log(sfxName);
            AudioManager.I.PlaySFX(sfxName);
            await Task.Delay(500);
        }
        if (direction == Direction.right)
            GameManager.I.SetScene(targetPosition, false);
        else
            GameManager.I.SetScene(targetPosition, true);
        if (sceneName == "EndingCredit")
        {
            //Debug.Log("aaa");
            DBManager.I.currData.sceneName = "Stage5";
            DBManager.I.savedData.sceneName = "Stage5";
            DBManager.I.currData.lastPos = new Vector2(-18, 2.05f);
            DBManager.I.savedData.lastPos = new Vector2(-18, 2.05f);
            DBManager.I.Save();
            // transform.SetParent(null);
            // DontDestroyOnLoad(gameObject);
            await Task.Delay(100);
            GameManager.I.LoadSceneAsync(sceneName, loadingPage);
            // await Task.Delay(700);
            // while(!GameManager.I.isSceneWaiting)
            // {
            //     await Task.Delay(100);
            // }
            // await Task.Delay(100);
            return;
        }

        GameManager.I.LoadSceneAsync(sceneName, loadingPage);
    }


}
