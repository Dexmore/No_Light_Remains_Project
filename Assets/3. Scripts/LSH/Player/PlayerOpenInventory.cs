using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerOpenInventory : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerOpenInventory(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private InputAction escAction;
    bool escPressed;
    private InputAction inventoryAction;
    bool inventoryPressed;
    int flagInt = 0;
    public void Enter()
    {

        if (escAction == null)
            escAction = ctx.inputActionAsset.FindActionMap("UI").FindAction("ESC");
        if (inventoryAction == null)
            inventoryAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Inventory");
        ctx.inventoryUI.Open();
        flagInt = 0;
        AudioManager.I.PlaySFX("Pocket1");
        GameManager.I.isOpenInventory = true;
        // Debug.Log("착용중인 모든 기어 멤버변수에 저장");
        gearDatas1 = null;
        gearDatas2 = null;
        var rawDatas = DBManager.I.currData.gearDatas;
        if (rawDatas != null)
        {
            // 이름순으로 정렬된 '장착 상태' 스냅샷 생성
            gearDatas1 = rawDatas.OrderBy(x => x.Name).Where(x => x.isEquipped).ToList();
        }


    }
    List<CharacterData.GearData> gearDatas1;
    List<CharacterData.GearData> gearDatas2;
    Particle electricPa;
    public void Exit()
    {
        ctx.inventoryUI.Close();
        AudioManager.I.PlaySFX("Pocket1");
        GameManager.I.isOpenInventory = false;


        //Gear 기어 (확장의 기어) 008_ExpansionGear
        bool outValue = false;
        if (DBManager.I.HasGear("008_ExpansionGear", out outValue))
        {
            if (outValue)
            {
                DBManager.I.currData.maxPotionCount = 4;
                if (DBManager.I.currData.currPotionCount <= 3)
                {
                    DBManager.I.currData.currPotionCount++;
                }
                ctx.hUDBinder.Refresh(1f);
            }
            else
            {
                DBManager.I.currData.maxPotionCount = 3;
                if (DBManager.I.currData.currPotionCount >= 4)
                {
                    DBManager.I.currData.currPotionCount = 3;
                }
                ctx.hUDBinder.Refresh(1f);
            }
        }
        else
        {
            DBManager.I.currData.maxPotionCount = 3;
            if (DBManager.I.currData.currPotionCount >= 4)
            {
                DBManager.I.currData.currPotionCount = 3;
            }
            ctx.hUDBinder.Refresh(1f);
        }

        //Gear 기어 (초신성 기어) 006_SuperNovaGear
        bool outValue1 = false;
        if (DBManager.I.HasGear("006_SuperNovaGear", out outValue1))
        {
            if (outValue1)
            {
                GameManager.I.isSuperNovaGearEquip = true;
            }
            else
            {
                GameManager.I.isSuperNovaGearEquip = false;
            }
        }
        else
        {
            GameManager.I.isSuperNovaGearEquip = false;
        }



        //Gear Changed

        var rawDatas = DBManager.I.currData.gearDatas;
        if (rawDatas != null)
        {
            // 이름순으로 정렬된 '장착 상태' 스냅샷 생성
            gearDatas2 = rawDatas.OrderBy(x => x.Name).Where(x => x.isEquipped).ToList();
        }
        bool isEqual = false;
        if (gearDatas1 == null && gearDatas2 == null)
        {
            isEqual = true;
        }
        else if (gearDatas1 == null || gearDatas2 == null)
        {
            isEqual = false;
        }
        else if (gearDatas1.Count == 0 && gearDatas2.Count == 0)
        {
            isEqual = true;
        }
        else if (gearDatas1.Count == 0 || gearDatas2.Count == 0)
        {
            isEqual = false;
        }
        else if (gearDatas1.Count != gearDatas2.Count)
        {
            isEqual = false;
        }
        else
        {
            isEqual = true;
            gearDatas2.OrderBy(x => x.Name);
            for (int k = 0; k < gearDatas1.Count; k++)
            {
                if (gearDatas1[k].isEquipped != gearDatas2[k].isEquipped)
                {
                    isEqual = false;
                    break;
                }
            }
        }
        if (!isEqual && gearDatas2.Count != 0)
        {
            if (gearDatas2.Count != 0)
            {
                AudioManager.I.PlaySFX("Up8Bit");
                Effect();
                ParticleManager.I.PlayText("Gear Changed!", ctx.transform.position + 1.2f * Vector3.up, ParticleManager.TextType.PlayerNotice, 2.3f);
            }
            else
            {
                ParticleManager.I.PlayText("Empty Gear", ctx.transform.position + 1.2f * Vector3.up, ParticleManager.TextType.PlayerNotice, 2f);
            }
        }
    }
    async void Effect()
    {
        await Task.Delay(1);
        electricPa = ParticleManager.I.PlayParticle("Electricity2", ctx.transform.position + new Vector3(0.1f, 1f, 0), Quaternion.identity);
        await Task.Delay(200);
        AudioManager.I.PlaySFX("Electricity");
        await Task.Delay(300);
        electricPa?.Despawn();
        electricPa = null;
    }
    public void UpdateState()
    {
        escPressed = escAction.IsPressed();
        if (escPressed)
        {
            fsm.ChangeState(ctx.idle);
        }

        inventoryPressed = inventoryAction.IsPressed();
        if (!inventoryPressed && flagInt == 0)
        {
            flagInt = 1;
        }
        else if (inventoryPressed && flagInt == 1)
        {
            fsm.ChangeState(ctx.idle);
        }


    }
    public void UpdatePhysics()
    {

    }
    







}
