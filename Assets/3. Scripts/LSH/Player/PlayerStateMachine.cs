using UnityEngine;
public class PlayerStateMachine
{
    public IPlayerState currentState { get; private set; }
    public void ChangeState(IPlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
        //Debug.Log($"{currentState} 시작");
    }
    public void OnDisable() => currentState?.Exit();
    public void Update() => currentState?.UpdateState(); // 상태 업데이트
    public void FixedUpdate() => currentState?.UpdatePhysics(); // 물리 업데이트
}