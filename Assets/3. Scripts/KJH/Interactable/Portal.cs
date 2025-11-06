using UnityEngine;
public class Portal : Interactable
{
    public override Type type => Type.Portal;
    public bool isAuto = true;
    public bool isReady = true;
    public bool isShowLoadingPage = false;
    [SerializeField] string sceneName;
    [SerializeField] Vector2 targetPosition;
    
    void Start()
    {
        
    }
    public void Run()
    {
        if (!isReady) return;
        GameManager.I.LoadSceneAsync(sceneName, isShowLoadingPage);
    }
    

}
