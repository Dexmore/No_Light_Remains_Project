using UnityEngine;
public class GearSlot : DropItem
{
    public int targetCount;
    protected override void OnEnable()
    {
        base.OnEnable();
        if (DBManager.I.currData.maxGearCost != targetCount - 1) gameObject.SetActive(false);
    }
    public override void Run()
    {
        base.Run();
        DBManager.I.currData.maxGearCost = targetCount;
        DBManager.I.savedData.maxGearCost = targetCount;
        ParticleManager.I.PlayText("Gear Socket Up", transform.position + 1.2f * Vector3.up, ParticleManager.TextType.PlayerNotice, 1f);
    }
    



}
