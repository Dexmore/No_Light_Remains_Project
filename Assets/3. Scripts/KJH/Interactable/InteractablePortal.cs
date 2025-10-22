using UnityEngine;

public class InteractablePortal : Interactable
{
    public override Type type => Type.Portal;
    public string goSceneName;
    public Vector3 goPosition;
    public bool isReady;
    

}
