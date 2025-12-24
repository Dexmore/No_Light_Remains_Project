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
    public override void Run()
    {
        if (!isReady) return;
        if (isRun) return;
        isRun = true;
        if (direction == Direction.right)
            GameManager.I.SetScene(targetPosition, false);
        else
            GameManager.I.SetScene(targetPosition, true);
        GameManager.I.LoadSceneAsync(sceneName, loadingPage);
    }


}
