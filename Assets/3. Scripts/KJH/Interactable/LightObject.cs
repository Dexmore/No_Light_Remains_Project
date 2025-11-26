using UnityEngine;
public class LightObject : Interactable
{
    public override Type type => Type.LightObject;

    public override bool isReady { get; set; }
    public float promptFill;
    void Start()
    {
        isReady = true;
        promptFill = 0f;
    }













}
