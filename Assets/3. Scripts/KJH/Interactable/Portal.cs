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
        if (direction == Direction.right)
            GameManager.I.SetScene(targetPosition, false);
        else
            GameManager.I.SetScene(targetPosition, true);
        
        if (sceneName == "EndingCredit")
        {
            DBManager.I.Save();
            DBManager.I.currData.sceneName = "Stage5";
            DBManager.I.savedData.sceneName = "Stage5";
            DBManager.I.currData.lastPos = new Vector2(-18, 2.05f);
            DBManager.I.savedData.lastPos = new Vector2(-18, 2.05f);
            await Task.Delay(50);
        }
        
        GameManager.I.LoadSceneAsync(sceneName, loadingPage);
    }


}
