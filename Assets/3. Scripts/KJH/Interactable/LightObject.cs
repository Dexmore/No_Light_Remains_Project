using UnityEngine;
public class LightObject : Interactable
{
    public override Type type => Type.LightObject;

    public override bool isReady { get; set; }
    void Start()
    {
        isReady = true;
    }













}
