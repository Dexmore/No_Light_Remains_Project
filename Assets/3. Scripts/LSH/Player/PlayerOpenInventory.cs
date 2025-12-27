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
        Debug.Log("착용중인 모든 기어 검사후 멤버변수에 저장");
    }
    public void Exit()
    {
        ctx.inventoryUI.Close();
        AudioManager.I.PlaySFX("Pocket1");
        GameManager.I.isOpenInventory = false;
        Debug.Log("착용중인 모든 기어 검사후 인벤토리 열었을때 멤버변수에 저장했던 착용상태와 비교");
        Debug.Log("바뀐 기어들이 있다면 기어 체인지드 텍스트와 파워업 전기 지지직 연출 시작");
    }
    public void UpdateState()
    {
        escPressed = escAction.IsPressed();
        if(escPressed)
        {
            fsm.ChangeState(ctx.idle);
        }

        inventoryPressed = inventoryAction.IsPressed();
        if(!inventoryPressed && flagInt == 0)
        {
            flagInt = 1;
        }
        else if(inventoryPressed && flagInt == 1)
        {
            fsm.ChangeState(ctx.idle);
        }


    }
    public void UpdatePhysics()
    {

    }
    
}
