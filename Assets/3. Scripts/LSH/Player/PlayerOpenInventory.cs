using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOpenInventory : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;

    public PlayerOpenInventory(PlayerControl ctx, PlayerStateMachine fsm)
    {
        this.ctx = ctx;
        this.fsm = fsm;
    }

    private InputAction escAction;
    private bool escPressed;
    private InputAction inventoryAction;
    private bool inventoryPressed;
    private int flagInt = 0;

    // Enter 시점의 장착 상태를 저장하기 위한 리스트
    private List<string> gearNamesSnapshot;
    private Particle electricPa;

    public void Enter()
    {
        // 입력 액션 초기화
        if (escAction == null)
            escAction = ctx.inputActionAsset.FindActionMap("UI").FindAction("ESC");
        if (inventoryAction == null)
            inventoryAction = ctx.inputActionAsset.FindActionMap("Player").FindAction("Inventory");

        ctx.inventoryUI.Open();
        flagInt = 0;
        AudioManager.I.PlaySFX("Pocket1");
        GameManager.I.isOpenInventory = true;

        // [핵심] 인벤토리를 여는 시점의 장착 기어 이름들을 저장 (값 복사)
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
        HUDBinder hUDBinder = ctx.hUDBinder;

        // 1. 상태 판별
        bool wasEquipped = gearNamesSnapshot.Contains("008_ExpansionGear");
        bool isEquippedNow = DBManager.I.HasGear("008_ExpansionGear", out bool isEquip) && isEquip;
        int level = isEquippedNow ? DBManager.I.GetGearLevel("008_ExpansionGear") : -1;

        // --- 핵심 로직: 부채 시스템을 이용한 증감 ---

        // [케이스 A] 안 끼고 있다가 새로 장착함: 보너스 지급
        if (!wasEquipped && isEquippedNow)
        {
            int bonus = (level == 1) ? 2 : 1;

            // 만약 갚아야 할 부채가 있다면 보너스에서 먼저 차감
            if (GameManager.I.potionDebt > 0)
            {
                int debtClear = Mathf.Min(bonus, GameManager.I.potionDebt);
                bonus -= debtClear;
                GameManager.I.potionDebt -= debtClear;
            }

            DBManager.I.currData.cpc += bonus;
        }
        // [케이스 B] 끼고 있다가 해제함: 보너스 회수
        else if (wasEquipped && !isEquippedNow)
        {
            int penalty = (DBManager.I.currData.mpc == 5) ? 2 : 1;

            // 현재 포션이 부족해서 다 못 뺏는다면, 그만큼 부채로 남김
            if (DBManager.I.currData.cpc < penalty)
            {
                GameManager.I.potionDebt += (penalty - DBManager.I.currData.cpc);
                DBManager.I.currData.cpc = 0;
            }
            else
            {
                DBManager.I.currData.cpc -= penalty;
            }
        }
        // 2. 수치 갱신
        if (isEquippedNow)
            DBManager.I.currData.mpc = (level == 1) ? 5 : 4;
        else
            DBManager.I.currData.mpc = 3;

        // 최종 한도 보정
        DBManager.I.currData.cpc = Mathf.Clamp(DBManager.I.currData.cpc, 0, DBManager.I.currData.mpc);

        hUDBinder?.Refresh(1f);

        // --- 2. 슈퍼노바 기어 (006_SuperNovaGear) 처리 ---
        if (DBManager.I.HasGear("006_SuperNovaGear", out bool isSuperNova))
        {
            GameManager.I.isSuperNovaGearEquip = isSuperNova;
        }
        else
        {
            GameManager.I.isSuperNovaGearEquip = false;
        }


        // --- 3. Gear Changed 연출 체크 로직 ---
        var rawDatas = DBManager.I.currData.gearDatas;
        if (rawDatas != null && gearNamesSnapshot != null)
        {
            var currentGearNames = rawDatas.Where(x => x.isEquipped)
                                           .Select(x => x.Name)
                                           .OrderBy(name => name)
                                           .ToList();

            // 스냅샷과 현재 상태 비교
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