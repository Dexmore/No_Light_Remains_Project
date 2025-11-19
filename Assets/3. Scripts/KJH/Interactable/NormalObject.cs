using UnityEngine;

public class NormalObject : Interactable
{
    public override Type type => Type.NormalObject;
    public override bool isReady { get; set; }
    void Start()
    {
        isReady = true;
    }



}
