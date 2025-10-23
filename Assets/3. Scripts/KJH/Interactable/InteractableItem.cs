using UnityEngine;
public class InteractableItem : Interactable
{
    public override Type type => Type.Item;
    public bool isAutoRoot = false;
    

}
