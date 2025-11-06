using UnityEngine;
public class DropItem : Interactable
{
    public override Type type => Type.DropItem;
    public bool isAuto = true;
    public ItemData itemData;
    public int money;
    public void Get()
    {
        
    }

}
