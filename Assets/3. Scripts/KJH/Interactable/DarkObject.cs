using UnityEngine;

public class DarkObject : Interactable
{
    public override Type type => Type.DarkObject;
    public override bool isReady { get; set; }
    void Start()
    {
        isReady = true;
    }
    
    
}
