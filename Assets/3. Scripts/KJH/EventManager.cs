using UnityEngine;
using UnityEngine.Events;
public class EventManager : SingletonBehaviour<EventManager>
{
    protected override bool IsDontDestroy() => false;
    public struct AttackData
    {
        public Transform from;
        public Transform target;
        public float damage;
        public AttackData(Transform from, Transform target, float damage)
        {
            this.from = from;
            this.target = target;
            this.damage = damage;
        }
    }
    public UnityAction<AttackData> onAttack = (x) => { };
    public struct LightData
    {
        public float amount;
        public Vector2 center;
        public LightData(float amount, Vector2 center)
        {
            this.amount = amount;
            this.center = center;
        }
    }
    public UnityAction<LightData> onLightStay = (x) => { };








    
}
