using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterControl : MonoBehaviour
{
    public float height = 1.5f;
    public float width = 0.7f;
    public float jumpForce = 9f;
    [ReadOnlyInspector] public bool isDie;
    [ReadOnlyInspector] public bool isGround;
    public LayerMask groundLayer;
    public float currHP;
    [Range(0f, 1f)] public float aggressive = 0.2f;

    [Header("SO")]
    public MonsterDataSO data;
    [ReadOnlyInspector] public string ID;
    [ReadOnlyInspector] public string Name;
    [ReadOnlyInspector] public MonsterType Type;
    [ReadOnlyInspector] public float MoveSpeed;
    [ReadOnlyInspector] public float Attack;
    [ReadOnlyInspector] public float maxHP;

    [Header("Pattern")]
    public Pattern[] patterns;
    Astar2DXYPathFinder astar;
    [HideInInspector] public AttackRange attackRange;
    void Awake()
    {
        SettingFSM();
        TryGetComponent(out astar);
        attackRange = GetComponentInChildren<AttackRange>(true);
    }
    void Init()
    {
        ID = data.ID;
        Name = data.Name;
        Type = data.Type;
        MoveSpeed = data.MoveSpeed;
        Attack = data.Attack;
        maxHP = data.HP;
        currHP = maxHP;
        if (astar)
        {
            astar.height = height;
            astar.width = width;
            astar.unit = Mathf.Clamp(width * 0.33f, 0.5f, 3f);
            if (jumpForce > 0)
            {
                astar.canJump = true;
                astar.jumpForce = jumpForce;
            }
            else
            {
                astar.canJump = false;
                astar.jumpForce = 0f;
            }
        }
        // 게임 시작시 스테이트를 Idle로
        ChangeState(State.Idle);
        // 게임 시작시 컨디션을 Peaceful로
        condition = Condition.Peaceful;
        GameManager.I.onHit += HitHandler;
    }
    #region UniTask Setting
    [HideInInspector] public CancellationTokenSource cts;
    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
        Init();
    }
    void OnDisable() => UniTaskCancel();
    void OnDestroy() => UniTaskCancel();
    void UniTaskCancel()
    {
        try
        {
            cts?.Cancel();
            cts?.Dispose();
        }
        catch (System.Exception e)
        {

            Debug.Log(e.Message);
        }
        cts = null;
    }
    #endregion
    #region FSM
    [ReadOnlyInspector] public State state;
    [HideInInspector] public State prevState;
    [HideInInspector] public Dictionary<State, MonsterState> stateDictionary = new Dictionary<State, MonsterState>();
    void SettingFSM()
    {
        MonsterState[] abilities = GetComponents<MonsterState>();
        for (int j = 0; j < System.Enum.GetValues(typeof(State)).Length; j++)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].mapping == (State)j)
                {
                    stateDictionary.Add((State)j, abilities[i]);
                    abilities[i].enabled = false;
                    break;
                }
            }
        }
    }
    public void ChangeState(State newState)
    {
        if (newState == State.Hit || newState == State.Die)
        {
            ChangeState_ut(newState, cts.Token).Forget();
            return;
        }
        if (stateDictionary[newState].coolTime == 0)
        {
            float _coolTime = 0;
            bool isFind = false;
            for (int i = 0; i < patterns.Length; i++)
            {
                for (int j = 0; j < patterns.Length; j++)
                {

                    Frequency frequency = patterns[i].frequencies[j];
                    if (frequency.state == newState)
                    {
                        isFind = true;
                        _coolTime = frequency.coolTime;
                        break;
                    }
                }
                if (isFind) break;
            }
            stateDictionary[newState].coolTime = _coolTime;
        }
        ChangeState_ut(newState, cts.Token).Forget();
    }
    public void ChangeNextState()
    {
        State curr = state;
        float totalWeightSum = 0;
        bool isPeaceful = HasCondition(Condition.Peaceful);
        for (int i = 0; i < patterns.Length; i++)
        {
            // condition 해당 없으면 skip 
            if (!HasCondition(patterns[i].condition)) continue;
            if (isPeaceful)
            {
                if (patterns[i].condition != Condition.Peaceful) continue;
            }
            for (int j = 0; j < patterns[i].frequencies.Length; j++)
            {
                State state = patterns[i].frequencies[j].state;
                // state가 dictionary에 없으면 skip 
                if (!stateDictionary.ContainsKey(state)) continue;
                // state가 쿨타임 중이면 skip 
                if (IsCoolTime(state)) continue;
                // state가 그외에도 할 수 없는 상태라면 skip 
                if (!IsCan(state)) continue;
                // 현재와 중복된 state라면 skip
                if (curr == state) continue;
                totalWeightSum += patterns[i].frequencies[j].weight;
            }
        }
        float randomWeightSum = Random.Range(0, totalWeightSum);
        int find1 = -1, find2 = -1;
        float partialWeightSum = 0;
        for (int i = 0; i < patterns.Length; i++)
        {
            // condition 해당 없으면 skip 
            if (!HasCondition(patterns[i].condition)) continue;
            if (isPeaceful)
            {
                if (patterns[i].condition != Condition.Peaceful) continue;
            }
            for (int j = 0; j < patterns[i].frequencies.Length; j++)
            {
                State state = patterns[i].frequencies[j].state;
                // state가 dictionary에 없으면 skip 
                if (!stateDictionary.ContainsKey(state)) continue;
                // state가 쿨타임 중이면 skip
                if (IsCoolTime(state)) continue;
                // state가 그외에도 할 수 없는 상태라면 skip 
                if (!IsCan(state)) continue;
                // 현재와 중복된 state라면 skip
                if (curr == state) continue;
                partialWeightSum += patterns[i].frequencies[j].weight;
                if (randomWeightSum <= partialWeightSum)
                {
                    find1 = i;
                    find2 = j;
                    break;
                }
            }
        }
        //Debug.Log($"{totalWeightSum},{randomWeightSum},{partialWeightSum},{find1},{find2}");
        if (find1 == -1 && find2 == -1)
        {
            ChangeState(State.Idle);
            return;
        }
        Frequency frequency = patterns[find1].frequencies[find2];
        State nextState = frequency.state;
        stateDictionary[nextState].coolTime = frequency.coolTime;
        ChangeState(nextState);
    }
    async UniTask ChangeState_ut(State newState, CancellationToken token)
    {
        // 이전 state 스크립트는 Disable 처리
        if (stateDictionary.Count == 0) return;
        stateDictionary[state].Exit();
        stateDictionary[state].enabled = false;
        await UniTask.Yield(token);
        prevState = state;
        state = newState;
        // 변경할 state 스크립트는 Enable 처리
        stateDictionary[state].enabled = true;
        stateDictionary[state].cts?.Cancel();
        stateDictionary[state].cts = new CancellationTokenSource();
        stateDictionary[state].Enter(stateDictionary[state].cts.Token).Forget();
        //Debug.Log($"{transform.name},{GetInstanceID()}] --> {state} 시작");
    }
    [System.Serializable]
    public enum State
    {
        Idle,
        Wander,
        Rest,
        Jump,
        Roar,
        Pursuit,
        RunAway,
        Reposition,
        Hit,
        KnockDown,
        Die,
        HandAttack,
        BiteAttack,
        JumpAttack,
        RangeAttack,
        RushAttack,
        ComboAttack,
    }
    [System.Serializable]
    public struct Frequency
    {
        public State state;
        public float weight;
        public float coolTime;
    }
    #endregion
    #region Condition
    [ReadOnlyInspector] public Condition condition;
    public bool HasCondition(Condition c)
    {
        return (condition & c) != 0;
    }
    public void AddCondition(Condition c)
    {
        if (HasCondition(c))
        {
            // 이미 켜져있는 이펙트 입니다.
            return;
        }
        condition = condition | c;
    }
    public void RemoveCondition(Condition c)
    {
        if (!HasCondition(c))
        {
            // 이미 꺼져있는 이펙트인데 리무브 시도할 경우 그냥 리턴
            return;
        }
        condition = condition & ~(c);
    }
    public int ConditionCount()
    {
        int count = 0;
        uint copy = (uint)condition;
        while (copy > 0)
        {
            copy &= (copy - 1);
            count++;
        }
        return count;
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
        public Frequency[] frequencies;
    }
    #endregion
    #region CoolTime
    [System.Serializable]
    public class CoolTime
    {
        public string abilityName;
        public System.Type type;
        public CancellationTokenSource cts;
        public float startTime;
        public float duration;
        public bool isPause;
    }
    [SerializeField] List<CoolTime> coolTimeList = new List<CoolTime>();
    public void SetCoolTime(State state, float time)
    {
        if (!stateDictionary.ContainsKey(state)) return;
        string tName = stateDictionary[state].GetType().ToString();
        int find = coolTimeList.FindIndex(x => x.abilityName == tName);
        // 매개변수 time이 <= 0 인 경우는 쿨타임이 만약 돌아가고 있는게 있다면 강제적으로 초기화하는 용도입니다. (주로 테스트 용)
        if (time <= 0)
        {
            if (find == -1)
                return;
            try
            {
                coolTimeList[find].cts?.Cancel();
                coolTimeList[find].cts?.Dispose();
            }
            catch
            {

            }
            RemoveCanNot(state, "CoolTime");
            coolTimeList.RemoveAt(find);
        }
        // 매개변수 time이 > 0 인 경우가 실제 대부분 사용하는 케이스 입니다.
        else if (time > 0)
        {
            if (find == -1)
            {
                //coolTimeList 에 없으므로 새로 등록
                CoolTime coolTime = new CoolTime();
                coolTime.abilityName = tName;
                coolTime.type = stateDictionary[state].GetType();
                coolTime.cts = new CancellationTokenSource();
                coolTime.startTime = Time.time;
                coolTime.duration = time;
                coolTimeList.Add(coolTime);
                AddCanNot(state, "CoolTime");
                SetCoolTimeUT(coolTime).Forget();
                return;
            }
            // 기존에 있는게 있다면 새로 덮어쓰기
            try
            {
                coolTimeList[find].cts?.Cancel();
                coolTimeList[find].cts?.Dispose();
            }
            catch
            {

            }
            coolTimeList[find].cts = new CancellationTokenSource();
            coolTimeList[find].startTime = Time.time;
            coolTimeList[find].duration = time;
            SetCoolTimeUT(coolTimeList[find]).Forget();
        }
    }
    public void PauseCoolTime(State state)
    {
        if (!stateDictionary.ContainsKey(state)) return;
        string tName = stateDictionary[state].GetType().ToString();
        int find = coolTimeList.FindIndex(x => x.abilityName == tName);
        if (find == -1) return;
        coolTimeList[find].isPause = true;
    }
    public void RemoveCoolTime(State state)
    {
        if (!stateDictionary.ContainsKey(state)) return;
        string tName = stateDictionary[state].GetType().ToString();
        int find = coolTimeList.FindIndex(x => x.abilityName == tName);
        if (find == -1) return;
        coolTimeList[find].isPause = false;
    }
    public bool IsCoolTime(State state)
    {
        if (!stateDictionary.ContainsKey(state)) return false;
        string tName = stateDictionary[state].GetType().ToString();
        int find = coolTimeList.FindIndex(x => x.abilityName == tName);
        if (find == -1) return false;
        return true;
    }
    public void SetCoolTime(System.Type type, float time)
    {
        string tName = type.Name;
        int find = coolTimeList.FindIndex(x => x.abilityName == tName);
        // 매개변수 time이 <= 0 인 경우는 쿨타임이 만약 돌아가고 있는게 있다면 강제적으로 초기화하는 용도입니다. (주로 테스트 용)
        if (time <= 0)
        {
            if (find == -1)
                return;
            try
            {
                coolTimeList[find].cts?.Cancel();
                coolTimeList[find].cts?.Dispose();
            }
            catch
            {

            }
            RemoveCanNot(type, "CoolTime");
            coolTimeList.RemoveAt(find);
        }
        // 매개변수 time이 > 0 인 경우가 실제 대부분 사용하는 케이스 입니다.
        else if (time > 0)
        {
            if (find == -1)
            {
                //coolTimeList 에 없으므로 새로 등록
                CoolTime coolTime = new CoolTime();
                coolTime.abilityName = tName;
                coolTime.type = type;
                coolTime.cts = new CancellationTokenSource();
                coolTime.startTime = Time.time;
                coolTime.duration = time;
                coolTimeList.Add(coolTime);
                AddCanNot(type, "CoolTime");
                SetCoolTimeUT(coolTime).Forget();
                return;
            }
            // 기존에 있는게 있다면 새로 덮어쓰기
            try
            {
                coolTimeList[find].cts?.Cancel();
                coolTimeList[find].cts?.Dispose();
            }
            catch
            {

            }
            coolTimeList[find].cts = new CancellationTokenSource();
            coolTimeList[find].startTime = Time.time;
            coolTimeList[find].duration = time;
            SetCoolTimeUT(coolTimeList[find]).Forget();
        }
    }
    async UniTask SetCoolTimeUT(CoolTime coolTime)
    {
        CancellationTokenSource ctsComp = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, coolTime.cts.Token);
        float startTime = Time.time;
        while (!ctsComp.IsCancellationRequested && Time.time - startTime < coolTime.duration)
        {
            await UniTask.Yield(ctsComp.Token);
            await UniTask.WaitUntil(() => !coolTime.isPause, cancellationToken: ctsComp.Token);
        }
        RemoveCanNot(coolTime.type, "CoolTime");
        coolTimeList.Remove(coolTime);
    }
    #endregion
    #region CanNot
    [System.Serializable]
    public struct CanNot
    {
        public string abilityName;
        public string reason;
        public CanNot(string abilityName, string reason)
        {
            this.abilityName = abilityName;
            this.reason = reason;
        }
    }
    [SerializeField] List<CanNot> cannotList = new List<CanNot>();
    public void AddCanNot(System.Type type, string reason)
    {
        string tName = type.Name;
        int find = cannotList.FindIndex(o => o.abilityName == tName && o.reason == reason);
        if (find != -1) return;
        CanNot element = new CanNot(tName, reason);
        cannotList.Add(element);
    }
    public void AddCanNot(State state, string reason)
    {
        if (!stateDictionary.ContainsKey(state)) return;
        string tName = stateDictionary[state].GetType().ToString();
        int find = cannotList.FindIndex(o => o.abilityName == tName && o.reason == reason);
        if (find != -1) return;
        CanNot element = new CanNot(tName, reason);
        cannotList.Add(element);
    }
    public void RemoveCanNot(System.Type type, string reason)
    {
        string tName = type.Name;
        for (int i = 0; i < 5; i++)
        {
            int find = cannotList.FindIndex(o => o.abilityName == tName && o.reason == reason);
            if (find != -1)
                cannotList.RemoveAt(find);
        }
    }
    public void RemoveCanNot(State state, string reason)
    {
        if (!stateDictionary.ContainsKey(state)) return;
        RemoveCanNot(stateDictionary[state].GetType(), reason);
    }
    public bool IsCan(System.Type type)
    {
        string tName = type.Name;
        int find = cannotList.ToList().FindIndex(o => o.abilityName == tName);
        if (find != -1) return false;
        return true;
    }
    public bool IsCan(State state)
    {
        if (!stateDictionary.ContainsKey(state)) return false;
        string tName = stateDictionary[state].GetType().ToString();
        int find = cannotList.ToList().FindIndex(o => o.abilityName == tName);
        if (find != -1) return false;
        return true;
    }
    #endregion
    #region Delay
    #endregion
    // === Ground 체크 ===
    [HideInInspector] public Dictionary<Collider2D, Vector2> collisions = new Dictionary<Collider2D, Vector2>();
    void OnCollisionStay2D(Collision2D collision)
    {
        if ((collision.collider.gameObject.layer & groundLayer) != 0)
            if (!collisions.ContainsKey(collision.collider))
                collisions.Add(collision.collider, collision.contacts[0].point);
            else
                collisions[collision.collider] = collision.contacts[0].point;
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if ((collision.collider.gameObject.layer & groundLayer) != 0)
            if (collisions.ContainsKey(collision.collider))
                collisions.Remove(collision.collider);
    }
    void Update()
    {
        isGround = false;
        if (collisions.Count > 0)
            foreach (var element in collisions)
                if (Mathf.Abs(element.Value.y - transform.position.y) < 0.09f * height)
                {
                    isGround = true;
                    break;
                }
    }
    #region Hit
    void HitHandler(HitData data)
    {
        if (isDie) return;
        if (data.target.Root() != transform) return;
        currHP -= data.damage;
        if (HasCondition(Condition.Peaceful))
            RemoveCondition(Condition.Peaceful);
        if (currHP <= 0)
            ChangeState(State.Die);
    }
    #endregion
    


}
