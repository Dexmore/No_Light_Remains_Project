using UnityEngine;

public enum MonsterType { Small, Middle, Large }

[CreateAssetMenu(fileName = "MonsterData", menuName = "Game/Monster Data")]
public class MonsterDataSO : ScriptableObject
{
    public string ID;
    public string Name;
    public MonsterType Type;
    public float MoveSpeed;
    public float Attack;
    public float HP;
}
