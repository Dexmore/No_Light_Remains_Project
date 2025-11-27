using UnityEngine;

public class DarkObject : Interactable
{
    public override Type type => Type.DarkObject;
    public override bool isReady { get; set; }
    public float promptFill;
    protected virtual void Start()
    {
        isReady = true;
        promptFill = 0f;
    }
    public virtual void Run()
    {
        isReady = false;
        
    }
    
    
}
