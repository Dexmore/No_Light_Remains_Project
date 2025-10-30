using UnityEngine;
public class InteractablePortal : Interactable
{
    public override Type type => Type.Portal;
    [SerializeField] string goSceneName;
    [SerializeField] Vector3 goPosition;
    public bool isReady;
    [SerializeField] GameObject unreadyPortal;
    [SerializeField] GameObject readyPortal;
    void Start()
    {
        if (isReady)
        {

        }
        else
        {
            
        }
    }
    public void Ready()
    {
        if (isReady) return;
        isReady = true;
    }
    public void UnReady()
    {
        if (!isReady) return;
        isReady = false;
    }
    public void Run()
    {
        if (!isReady) return;
        GameManager.I.LoadSceneAsync(goSceneName, true);
    }
    

}
