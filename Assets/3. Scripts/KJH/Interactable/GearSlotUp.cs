using UnityEngine;
public class GearSlotUp : DropItem
{
    public int targetCount;
    protected override void OnEnable()
    {
        base.OnEnable();
        if (targetCount > 0)
        {
            if (DBManager.I.currData.maxGearCost != targetCount - 1) gameObject.SetActive(false);
        }
        else
        {
            if (DBManager.I.currData.maxGearCost == 6) gameObject.SetActive(false);
        }
    }
    public override void Run()
    {
        base.Run();
        AudioManager.I.PlaySFX("Up8Bit");
        if (targetCount > 0)
        {
            DBManager.I.currData.maxGearCost = targetCount;
        }
        else
        {
            int count = DBManager.I.currData.maxGearCost;
            count = Mathf.Clamp(count + 1, 3, 6);
            DBManager.I.currData.maxGearCost = count;
        }
        ParticleManager.I.PlayText("Gear Slot Up!", transform.position + 1.2f * Vector3.up, ParticleManager.TextType.PlayerNotice, 2.3f);
    }




}
