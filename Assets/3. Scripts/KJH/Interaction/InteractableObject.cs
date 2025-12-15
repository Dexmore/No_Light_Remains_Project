using UnityEngine;

public class InteractableObject : Interactable
{
    public override Type type => Type.NormalObject;
    public override bool isReady { get; set; }
    protected virtual void Start()
    {
        isReady = true;
    }
    public virtual void Run()
    {
        isReady = false;
    }



}
