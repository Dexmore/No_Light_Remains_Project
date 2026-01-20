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

    // 변경된 부분: 객체 참조가 아닌 데이터의 '값(이름)'을 저장하여 비교합니다.
    List<string> gearNamesSnapshot; 
    Particle electricPa;

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

        // Enter 시점의 장착된 기어 이름들을 저장 (값 복사)
        var rawDatas = DBManager.I.currData.gearDatas;
        if (rawDatas != null)
        {
            gearNamesSnapshot = rawDatas.Where(x => x.isEquipped)
                                        .Select(x => x.Name)
                                        .OrderBy(name => name)
                                        .ToList();
        }
        else
        {
            gearNamesSnapshot = new List<string>();
        }
    }

    public void Exit()
    {
        ctx.inventoryUI.Close();
        AudioManager.I.PlaySFX("Pocket1");
        GameManager.I.isOpenInventory = false;

        // --- Gear 기어 효과 처리 (기존 로직 유지) ---
        bool outValue = false;
        HUDBinder hUDBinder = ctx.hUDBinder;
        if (DBManager.I.HasGear("008_ExpansionGear", out outValue))
        {
            if (outValue)
            {
                int level = DBManager.I.GetGearLevel("008_ExpansionGear");
                if (level == 0)
                {
                    DBManager.I.currData.maxPotionCount = 4;
                    if (DBManager.I.currData.currPotionCount <= 3)
                    {
                        if (!GameManager.I.hasGivenExpansionBonus1)
                        {
                            GameManager.I.hasGivenExpansionBonus1 = true;
                            DBManager.I.currData.currPotionCount++;
                        }
                    }
                }
                else if (level == 1)
                {
                    DBManager.I.currData.maxPotionCount = 5;
                    if (DBManager.I.currData.currPotionCount <= 4)
                    {
                        if (!GameManager.I.hasGivenExpansionBonus2)
                        {
                            GameManager.I.hasGivenExpansionBonus2 = true;
                            DBManager.I.currData.currPotionCount += 2;
                            if (DBManager.I.currData.currPotionCount > 5) DBManager.I.currData.currPotionCount = 5;
                        }
                    }
                }
                hUDBinder?.Refresh(1f);
            }
            else
            {
                DBManager.I.currData.maxPotionCount = 3;
                if (DBManager.I.currData.currPotionCount >= 4) DBManager.I.currData.currPotionCount = 3;
                hUDBinder?.Refresh(1f);
            }
        }
        else
        {
            DBManager.I.currData.maxPotionCount = 3;
            if (DBManager.I.currData.currPotionCount >= 4) DBManager.I.currData.currPotionCount = 3;
            hUDBinder?.Refresh(1f);
        }

        bool outValue1 = false;
        if (DBManager.I.HasGear("006_SuperNovaGear", out outValue1))
        {
            GameManager.I.isSuperNovaGearEquip = outValue1;
        }
        else
        {
            GameManager.I.isSuperNovaGearEquip = false;
        }

        // --- Gear Changed 체크 로직 수정 ---
        var rawDatas = DBManager.I.currData.gearDatas;
        if (rawDatas != null && gearNamesSnapshot != null)
        {
            // Exit 시점의 장착된 기어 이름 리스트 생성
            var currentGearNames = rawDatas.Where(x => x.isEquipped)
                                           .Select(x => x.Name)
                                           .OrderBy(name => name)
                                           .ToList();

            // 두 리스트의 내용물이 같은지 비교 (순서와 값 확인)
            bool isEqual = gearNamesSnapshot.SequenceEqual(currentGearNames);

            if (!isEqual)
            {
                if (currentGearNames.Count > 0)
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
            return;
        }

        inventoryPressed = inventoryAction.IsPressed();
        if (!inventoryPressed && flagInt == 0)
        {
            flagInt = 1;
        }
        else if (inventoryPressed && flagInt == 1)
        {
            fsm.ChangeState(ctx.idle);
            return;
        }
    }

    public void UpdatePhysics() { }
}