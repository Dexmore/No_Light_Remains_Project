using UnityEngine;

public class PlayerStateMachine_LSH : MonoBehaviour
{
    public IPlayerState_LSH currentState { get; private set; }
    public void Initialize(IPlayerState_LSH startState) // 처음 상태 설정
    {
        currentState = startState;
        currentState.Enter();
    }

    public void ChangeState(IPlayerState_LSH newState) // 상태 변경
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void PlayerKeyInput() => currentState?.PlayerKeyInput(); // 플레이어 키 입력

    public void UpdateState() => currentState?.UpdateState(); // 상태 업데이트
    public void UpdatePhysics() => currentState?.UpdatePhysics(); // 물리 업데이트
}
