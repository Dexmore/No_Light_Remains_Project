using UnityEngine;
public class Portal : Interactable
{
    public override Type type => Type.Portal;
    public bool isAuto = true;
    public override bool isReady { get; set; }
    public bool loadingPage = false;
    [SerializeField] string sceneName;
    [SerializeField] Vector2 targetPosition;
    bool isRun = false;
    void Start()
    {
        isReady = true;
        isRun = false;
    }
    public void Run()
    {
        if (!isReady) return;
        if (isRun) return;
        isRun = true;
        GameManager.I.LoadSceneAsync(sceneName, loadingPage);
    }
    

}
