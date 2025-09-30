using UnityEngine;
public class PlayerStateMachine_LSH
{
    public IPlayerState_LSH currentState { get; private set; }
    public void ChangeState(IPlayerState_LSH newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
    public void Update() => currentState?.Update(); // 상태 업데이트
    public void FixedUpdate() => currentState?.FixedUpdate(); // 물리 업데이트
}
