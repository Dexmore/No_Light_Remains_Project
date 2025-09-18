using UnityEngine;
[CreateAssetMenu(menuName = "MyScriptableObject/CreatureData")]
public class CreatureData : ScriptableObject
{
    public float maxHealth;
    public float moveSpeed;
    public Pattern[] patterns;
    [System.Serializable]
    public enum State
    {
        // 아래는 평화로운 상황에서 주로 하는 행동..
        Idle,
        Wander,
        Rest,
        Jump,

        // 플레이어 발견시 주로 하는 행동들..
        Roar,
        Pursuit,
        RunAway,

        // 플레이어랑 가까우면 할수있는 행동들..
        RePosition,


        // 아래는 AI가 직접 고를 수 없다
        Hit,
        KnockDown,
        Dead,

        // 전투 패턴들..
        HandAttack1,
        BiteAttack1,
        JumpAttack1,
        LoneRangeAttack1,
        RushAttack1,
        SequenceAttack1,

    }
    [System.Serializable]
    [System.Flags]
    public enum Condition
    {
        None,
        Peaceful = 1 << 0,
        FindPlayer = 1 << 1,
        ClosePlayer = 1 << 2,
        Injury1 = 1 << 4,
        Injury2 = 1 << 5,
        
    }
    [System.Serializable]
    public struct Pattern
    {
        public Condition condition;
        public StateData[] stateDatas;
    }
    [System.Serializable]
    public struct StateData
    {
        public State state;
        public float weight;
        public float coolTime;
    }
    
}
