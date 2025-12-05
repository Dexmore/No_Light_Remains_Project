using UnityEngine;
public class LightObject : Interactable
{
    public override Type type => Type.LightObject;
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
