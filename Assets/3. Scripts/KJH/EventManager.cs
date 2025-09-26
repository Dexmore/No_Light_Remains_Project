using System.Collections;
using UnityEngine;
using UnityEngine.Events;
public class EventManager : SingletonBehaviour<EventManager>
{
    public bool isDebugAttack;
    protected override bool IsDontDestroy() => false;
    #region Attack Event
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
    void OnEnable()
    {
        if (isDebugAttack)
            onAttack += DebugAttack;
    }
    void OnDisable()
    {
        if (isDebugAttack)
            onAttack -= DebugAttack;
    }
    void DebugAttack(AttackData data)
    {
        Debug.Log($"{data.from.name}--Attack-->{data.target.name}..... damage : {data.damage:F1}");
    }
    #endregion
    #region Light Event
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
    #endregion

    IEnumerator Start()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.5f);
        if (GameManager.I.transform.childCount > 0)
            if (GameManager.I.isFade)
                GameManager.I.FadeIn(2f);
    }










}
