public interface IPlayerState_LSH
{
    void Enter(); // 상태가 들어올때
    void Exit(); // 상태에서 벗어날 때
    void Update(); // 플레이어의 상태를 업데이트
    void FixedUpdate(); // 플레이어의 물리적인 연산 업데이트
}
