using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
public class MonsterControl : MonoBehaviour
{
    public float height = 1.5f;
    public float width = 0.7f;
    public float jumpLength = 9f;
    [ReadOnlyInspector] public bool isDie;
    [ReadOnlyInspector] public bool isGround;
    [HideInInspector] public bool isStagger;
    public LayerMask groundLayer;
    [ReadOnlyInspector] public float maxHealth;
    public float currHealth;
    [Range(0f, 1f)] public float aggressive = 0.2f;

    [Header("SO")]
    public MonsterDataSO data;
    [ReadOnlyInspector] public string ID;
    [ReadOnlyInspector] public string Name;
    [ReadOnlyInspector] public MonsterType Type;
    [ReadOnlyInspector] public float MoveSpeed;
    [HideInInspector] public float adjustedAttack;

    [Header("Pattern")]
    public Pattern[] patterns;
    Astar2DXYPathFinder astar;
    Rigidbody2D rb;
    [HideInInspector] public AttackRange attackRange;
    BossHUD bossHUD;
    void Awake()
    {
        startPosition = transform.position;
        startPosition.x = Mathf.Floor(startPosition.x * 10f) * 0.1f;
        startPosition.y = Mathf.Floor(startPosition.y * 10f) * 0.1f;
        SettingFSM();
        TryGetComponent(out astar);
        TryGetComponent(out rb);
        attackRange = GetComponentInChildren<AttackRange>(true);
        eye = transform.GetChild(0).Find("Eye");
        InitMatInfo();
        TryGetComponent(out monsterHit);
        if (data.Type == MonsterType.Large || data.Type == MonsterType.Boss)
            bossHUD = FindAnyObjectByType<BossHUD>();
        Transform pcc = null;
        if (pcc = transform.Find("ParryCountCanvas"))
        {
            parryCanvas = pcc.gameObject;
            parryCanvas.SetActive(false);
        }
    }
    void Init()
    {
        ID = data.ID;
        Name = data.Name;
        Type = data.Type;
        MoveSpeed = data.MoveSpeed;
        switch (DBManager.I.currData.difficulty)
        {
            case 0:
                adjustedAttack = data.Attack * 0.9f;
                maxHealth = data.HP;
                break;
            case 1:
                adjustedAttack = data.Attack * 1.15f;
                maxHealth = data.HP * 1.3f;
                break;
            case 2:
                adjustedAttack = data.Attack * 1.3f;
                maxHealth = data.HP * 1.6f;
                break;
        }
        currHealth = maxHealth;
        if (astar)
        {
            astar.height = height;
            astar.width = width;
            astar.unit = Mathf.Clamp(width * 0.33f, 0.5f, 3f);
            if (jumpLength > 0)
            {
                astar.canJump = true;
                astar.jumpLength = jumpLength;
            }
            else
            {
                astar.canJump = false;
                astar.jumpLength = 0f;
            }
        }
        // 게임 시작시 스테이트를 Idle로
        ChangeState(State.Idle);
        // 게임 시작시 컨디션을 Peaceful로
        condition = Condition.Peaceful;
        GameManager.I.onHit += HitHandler;
        GameManager.I.onParry += ParryHandler;
        Sensor(cts.Token).Forget();
    }
    #region UniTask Setting
    [HideInInspector] public CancellationTokenSource cts;
    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
        Init();
    }
    void OnDisable()
    {
        GameManager.I.onHit -= HitHandler;
        GameManager.I.onParry -= ParryHandler;
        MonsterState[] abilities = GetComponents<MonsterState>();
        for (int j = 0; j < System.Enum.GetValues(typeof(State)).Length; j++)
            for (int i = 0; i < abilities.Length; i++)
                abilities[i].Exit();
        UniTaskCancel();
    }
    void OnDestroy() => UniTaskCancel();
    void UniTaskCancel()
    {
        cts?.Cancel();
        try
        {
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
    public void ChangeState(State newState, bool force = false)
    {
        if (newState == State.Hit || newState == State.Die || force)
        {
            ChangeState_ut(newState, cts.Token).Forget();
            return;
        }
        if (stateDictionary[newState].coolTime == 0)
        {
            float _coolTime = 0;
            bool canPhase2 = false;
            for (int i = 0; i < patterns.Length; i++)
            {
                for (int j = 0; j < patterns.Length; j++)
                {

                    Frequency frequency = patterns[i].frequencies[j];
                    if (frequency.state == newState)
                    {
                        canPhase2 = true;
                        _coolTime = frequency.coolTime;
                        break;
                    }
                }
                if (canPhase2) break;
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
        if (totalWeightSum == 0)
        {
            ChangeState(State.Idle);
            return;
        }
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
        Jump,
        Pursuit,
        Reposition,
        RunAway,
        Roar,
        Rest,
        Hit,
        Die,
        NormalAttack,
        BiteAttack,
        RangeAttack,
        ShortAttack,
        MovingAttack,
        ReturnHome,
        SequenceAttack1,
        //
        ShootingAttack1,
        Heal,
        ShootingAttack2,
        SequenceAttack2,
        SequenceAttack3,
        DoubleAttack,
        Revive,
        Explosion,
        /////////////////////



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
        Phase2 = 1 << 3,
        Phase3 = 1 << 4,
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
            coolTimeList[find].cts?.Cancel();
            try
            {
                coolTimeList[find].cts?.Dispose();
            }
            catch
            {

            }
            RemoveCanNot(state, "CoolTime");
            coolTimeList.RemoveAt(find);
        }
        else if (time <= 0.1f)
        {
            return;
        }
        // 매개변수 time이 > 0.1 인 경우가 실제 대부분 사용하는 케이스 입니다.
        else
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
                SetCoolTimeUT(coolTime, coolTime.cts.Token).Forget();
                return;
            }
            // 기존에 있는게 있다면 새로 덮어쓰기
            coolTimeList[find].cts?.Cancel();
            try
            {
                coolTimeList[find].cts?.Dispose();
            }
            catch
            {

            }
            coolTimeList[find].cts = new CancellationTokenSource();
            coolTimeList[find].startTime = Time.time;
            coolTimeList[find].duration = time;
            SetCoolTimeUT(coolTimeList[find], coolTimeList[find].cts.Token).Forget();
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
            coolTimeList[find].cts?.Cancel();
            try
            {
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
                SetCoolTimeUT(coolTime, cts.Token).Forget();
                return;
            }
            // 기존에 있는게 있다면 새로 덮어쓰기
            coolTimeList[find].cts?.Cancel();
            try
            {
                coolTimeList[find].cts?.Dispose();
            }
            catch
            {

            }
            coolTimeList[find].cts = new CancellationTokenSource();
            coolTimeList[find].startTime = Time.time;
            coolTimeList[find].duration = time;
            SetCoolTimeUT(coolTimeList[find], cts.Token).Forget();
        }
    }
    async UniTask SetCoolTimeUT(CoolTime coolTime, CancellationToken token)
    {
        CancellationTokenSource ctsComp = CancellationTokenSource.CreateLinkedTokenSource(token, coolTime.cts.Token);
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
    Ray2D groundRay = new Ray2D();
    RaycastHit2D groundRayHit;
    float groundCheckTime;
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
        if (!isGround)
        {
            if (Time.time - groundCheckTime > Random.Range(0.1f, 0.3f))
            {
                groundCheckTime = Time.time;
                groundRay.origin = (Vector2)transform.position + 0.08f * Vector2.up;
                groundRay.direction = Vector2.down;
                groundRayHit = Physics2D.Raycast(groundRay.origin, groundRay.direction, 0.1f, groundLayer);
                if (groundRayHit)
                {
                    isGround = true;
                }
            }
        }
    }
    #region Sensor
    Collider2D[] nearPlayers = new Collider2D[80];
    public Dictionary<Collider2D, float> visibilites = new Dictionary<Collider2D, float>();
    public Dictionary<Collider2D, float> memories = new Dictionary<Collider2D, float>();
    [ReadOnlyInspector] public float findRadius;
    public float closeRadius;
    [HideInInspector] public Transform eye;
    [HideInInspector] bool isTemporalFight;
    float temporalFightTime;
    async UniTask Sensor(CancellationToken token)
    {
        await UniTask.Yield(token);
        findRadius = 4.1f * ((width + height) * 0.58f + 0.55f);
        if (data.Type == MonsterType.Large || data.Type == MonsterType.Boss)
            findRadius = 30.1f * ((width + height) * 0.58f + 0.55f);

        if (closeRadius == 0) closeRadius = 1.2f * (width * 0.61f + 0.7f);
        int count = 0;
        bool canPhase2 = false;
        bool canPhase3 = false;
        foreach (var element in patterns)
            if ((element.condition & Condition.Phase2) != 0)
            {
                canPhase2 = true;
                break;
            }
        foreach (var element in patterns)
            if ((element.condition & Condition.Phase3) != 0)
            {
                canPhase3 = true;
                break;
            }
        bool isSetTarget = false;
        while (!token.IsCancellationRequested)
        {
            int timeDelta = Random.Range(150, 500);
            await UniTask.Delay(timeDelta, cancellationToken: token);
            //nearPlayers = Physics2D.OverlapCircleAll((Vector2)transform.position, findRadius, LayerMask.GetMask("Player"));
            count = Physics2D.OverlapCircleNonAlloc(transform.position, findRadius, nearPlayers, LayerMask.GetMask("Player"));
            if (count > 0)
            {
                if (bossHUD && !isSetTarget)
                {
                    isSetTarget = true;
                    bossHUD.SetTarget(this);
                }
            }
            // visibilites
            float minDist = 999;
            for (int i = 0; i < count; i++)
            {
                if (!visibilites.ContainsKey(nearPlayers[i]))
                {
                    float visibility = await CheckVisibility(nearPlayers[i], token);
                    visibilites.Add(nearPlayers[i], visibility);
                }
                else
                {
                    float visibility = await CheckVisibility(nearPlayers[i], token);
                    visibilites[nearPlayers[i]] = visibility;
                }
                if (!memories.ContainsKey(nearPlayers[i]))
                {
                    memories.Add(nearPlayers[i], Time.time);
                }
                else
                {
                    memories[nearPlayers[i]] = Time.time;
                }
                await UniTask.Yield(token);
                float dist = Vector2.Distance(nearPlayers[i].transform.position, transform.position);
                if (minDist > dist)
                    minDist = dist;
                if (dist <= closeRadius)
                    if (!HasCondition(Condition.ClosePlayer))
                    {
                        AddCondition(Condition.ClosePlayer);
                        if (data.Type == MonsterType.Large || data.Type == MonsterType.Boss)
                            if (bossHUD.target != this)
                            {
                                bossHUD.SetTarget(this);
                            }
                    }

            }
            if (minDist > closeRadius)
                if (HasCondition(Condition.ClosePlayer))
                    RemoveCondition(Condition.ClosePlayer);
            Dictionary<Collider2D, float> copy = visibilites.ToDictionary(x => x.Key, x => x.Value);
            foreach (var element in visibilites)
            {
                int find = -1;
                for (int i = 0; i < count; i++)
                {
                    if (nearPlayers[i] == element.Key)
                    {
                        find = i;
                        break;
                    }
                }
                if (find == -1)
                {
                    copy.Remove(element.Key);
                }
            }
            visibilites = copy;
            // memories
            copy = memories.ToDictionary(x => x.Key, x => x.Value);
            foreach (var element in memories)
            {
                if (Time.time - element.Value > 15f)
                {
                    copy.Remove(element.Key);
                }
            }
            memories = copy;
            // Change Condition
            if (HasCondition(Condition.FindPlayer))
            {
                if (memories.Count == 0)
                {
                    RemoveCondition(Condition.FindPlayer);
                    AddCondition(Condition.Peaceful);
                }
            }
            else
            {
                if (memories.Count > 0)
                    AddCondition(Condition.FindPlayer);
            }
            // Remove Peaceful
            if (HasCondition(Condition.Peaceful))
            {
                if (memories.Count > 0 && visibilites.Count > 0)
                    if (!isTemporalFight)
                    {
                        float pow = Mathf.Pow(aggressive, 5.1f);
                        if (Random.value < pow)
                        {
                            foreach (var element in visibilites)
                            {
                                if (isTemporalFight) break;
                                if (element.Value > 0.72f)
                                {
                                    isTemporalFight = true;
                                    temporalFightTime = Time.time;
                                    RemoveCondition(Condition.Peaceful);
                                    break;
                                }
                            }
                            foreach (var element in memories)
                            {
                                if (isTemporalFight) break;
                                if (Time.time - element.Value < 3f)
                                {
                                    isTemporalFight = true;
                                    temporalFightTime = Time.time;
                                    RemoveCondition(Condition.Peaceful);
                                    break;
                                }
                            }

                        }
                    }
            }
            // Add Peaceful
            else
            {
                float val = 1f - aggressive;
                val = Mathf.Clamp(val, 0.15f, 0.7f);
                float val2 = 0f;
                if (isTemporalFight) val2 = 1.5f;
                float val3 = 0f;
                if (visibilites.Count == 0) val3 = 3f;
                if (memories.Count == 0) val3 = 5f;
                float pow = Mathf.Pow(val, 8f - val2 - val3);
                if (Random.value < pow)
                {
                    AddCondition(Condition.Peaceful);
                    isTemporalFight = false;
                }
            }
            // Injury
            float ratio = currHealth / maxHealth;
            if (canPhase2)
            {
                if (ratio <= 0.7f && ratio > 0.4f && !HasCondition(Condition.Phase2))
                {
                    AddCondition(Condition.Phase2);
                    RemoveCondition(Condition.Phase3);
                }
            }
            if (canPhase3)
            {
                if (ratio <= 0.4f && !HasCondition(Condition.Phase3))
                {
                    RemoveCondition(Condition.Phase2);
                    AddCondition(Condition.Phase3);
                }
            }
            if (HasCondition(Condition.Phase2) || HasCondition(Condition.Phase3))
            {
                if (ratio > 0.7f)
                {
                    RemoveCondition(Condition.Phase2);
                    RemoveCondition(Condition.Phase3);
                }
            }
        }
    }
    // 2D로 수정된 코드
    async UniTask<float> CheckVisibility(Collider2D target, CancellationToken token)
    {
        float sum = 0f;
        int rayCount = 0;
        Vector2 eyePos = (Vector2)eye.position;
        Vector2 targetCenter = (Vector2)target.bounds.center;
        Vector2 directionToTarget = targetCenter - eyePos;
        float sqrDistance = directionToTarget.sqrMagnitude;
        // 원래 Job System의 로직을 2D Raycast에 맞게 재구현
        if (sqrDistance < 4f * 4f)
            rayCount = 12;
        else if (sqrDistance < 6f * 6f)
            rayCount = 11;
        else if (sqrDistance < 8f * 8f)
            rayCount = 10;
        else if (sqrDistance < 10f * 10f)
            rayCount = 9;
        else if (sqrDistance < 12f * 12f)
            rayCount = 8;
        else if (sqrDistance < 15f * 15f)
            rayCount = 6;
        else if (sqrDistance < 18f * 18f)
            rayCount = 5;
        else if (sqrDistance < 23f * 23f)
            rayCount = 4;
        else if (sqrDistance < 33f * 33f)
            rayCount = 2;
        else
            rayCount = 1;
        // 2D 시야각 계산 (Y축을 기준으로 2D 평면에서 각도 계산)
        float angleToTarget = Vector2.Angle(transform.right, directionToTarget);
        if (angleToTarget > 120f)
        {
            return 0f;
        }
        for (int i = 0; i < rayCount; i++)
        {
            // 방향 벡터 계산 (랜덤성을 부여하여 흩뿌림)
            Vector2 randomDirection = Quaternion.Euler(0, 0, Random.Range(-10f, 10f)) * directionToTarget.normalized;
            float distance = directionToTarget.magnitude;
            RaycastHit2D hit = Physics2D.Raycast(eyePos, randomDirection, distance, groundLayer);
            if (hit.collider == null)
            {
                sum++;
            }
            else if (hit.collider.gameObject.GetInstanceID() == target.gameObject.GetInstanceID())
            {
                sum++;
            }
            await UniTask.Delay(1, cancellationToken: token);
        }
        if (rayCount == 0) return 0f;
        float result = sum / rayCount;
        return result;
    }
    #endregion
    #region Hit
    [HideInInspector] float curHitAmount;
    [HideInInspector] float maxHitAmount;
    int prevHitType;
    float prevHitTime;
    float hitCoolTime;
    MonsterHit monsterHit;
    void HitHandler(HitData hData)
    {
        if (isDie) return;
        if (hData.target.Root() != transform) return;

        // Effect
        if (hData.particleNames != null)
        {
            int rnd = Random.Range(0, hData.particleNames.Length);
            string particleName = hData.particleNames[rnd];
            ParticleManager.I.PlayParticle(particleName, hData.hitPoint, Quaternion.identity, null);
        }
        ParticleManager.I.PlayParticle("RadialLines", hData.hitPoint, Quaternion.identity);
        ParticleManager.I.PlayText(hData.damage.ToString("F1"), hData.hitPoint, ParticleManager.TextType.Damage);
        AudioManager.I.PlaySFX("Hit8bit1", 0.7f * hData.hitPoint + 0.3f * transform.position, null);
        GameManager.I.HitEffect(hData.hitPoint, 0.5f);
        HitChangeColor(Color.white);

        // Stagger
        if (Random.value <= 0.25f)
        {
            float staggerForce = 4.4f;
            float staggerFactor1 = 1f;
            switch (data.Type)
            {
                case MonsterType.Middle:
                    staggerFactor1 = 0.81f;
                    break;
                case MonsterType.Large:
                    staggerFactor1 = 0.65f;
                    break;
                case MonsterType.Boss:
                    staggerFactor1 = 0.43f;
                    break;
            }
            float staggerFactor2 = 1f;
            switch (hData.attackName)
            {
                case "Attack":
                    staggerFactor2 = 0.7f;
                    break;
                case "AttackCombo":
                    staggerFactor2 = 1f;
                    break;
            }
            float temp = hData.damage / data.HP;
            float staggerFactor3 = 0.68f + 0.32f * temp;
            Vector2 velo = rb.linearVelocity;
            rb.linearVelocity = 0.4f * velo;
            Vector2 dir = transform.position - hData.hitPoint;
            dir.y = dir.y * 0.1f + 0.02f;
            if (dir.y < 0) dir.y = 0.02f;
            dir.Normalize();
            rb.AddForce(staggerForce * Random.Range(0.9f, 1.1f) * staggerFactor1 * staggerFactor2 * staggerFactor3 * dir, ForceMode2D.Impulse);
            ctsStagger?.Cancel();
            ctsStagger = new CancellationTokenSource();
            var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsStagger.Token);
            ReleaseStagger(ctsLink.Token).Forget();
        }

        // Hit Small
        bool pass = true;
        if (state == State.Idle) pass = false;
        if (state == State.Pursuit) pass = false;
        if (state == State.Reposition) pass = false;
        if (state == State.NormalAttack) pass = false;
        if (state == State.RangeAttack) pass = false;
        if (state == State.BiteAttack) pass = false;
        if (state == State.MovingAttack) pass = false;
        if (!pass && Random.value <= 0.63f)
        {
            curHitAmount += hData.damage;
            if (maxHitAmount == 0) maxHitAmount = 1.5f * Random.Range(0.11f, 0.26f) * Mathf.Clamp(data.HP, 270, 1100);
            if (hitCoolTime == 0) hitCoolTime = Random.Range(0.2f, 0.6f);
            if (hitCoolTime >= 0.5f) hitCoolTime = Random.Range(0.2f, 1f);
            if (curHitAmount >= maxHitAmount && Time.time - prevHitTime > hitCoolTime)
            {
                if (Random.value < 0.63f)
                {
                    monsterHit.type = 1;
                    monsterHit.prevState = state;
                    ChangeState(State.Hit);
                    curHitAmount = 0;
                    prevHitTime = Time.time;
                    hitCoolTime = Random.Range(0.2f, 0.8f);
                    if (hitCoolTime >= 0.7f) hitCoolTime = Random.Range(0.2f, 1.5f);
                }
                else
                    curHitAmount = Random.Range(0.3f, 0.7f) * maxHitAmount;
            }
        }
        // Set HP
        currHealth -= hData.damage;
        if (HasCondition(Condition.Peaceful))
        {
            RemoveCondition(Condition.Peaceful);
            if (bossHUD != null)
                bossHUD.SetTarget(this);
        }
        if (currHealth <= 0)
        {
            ChangeState(State.Die);
            GameManager.I.onDie.Invoke(hData);
        }
    }
    CancellationTokenSource ctsStagger = new CancellationTokenSource();
    async UniTask ReleaseStagger(CancellationToken token)
    {
        isStagger = true;
        await UniTask.Delay((int)(1000f * Random.Range(0.18f, 0.48f)), cancellationToken: token);
        isStagger = false;
    }
    public int parryCount = 0;
    GameObject parryCanvas;
    CanvasGroup parryCanvasGroup;
    Color parryCountColor = new Color(0.984f, 1f, 0.75f, 1f);
    UnityEngine.UI.Image[] parryCountImages;
    CancellationTokenSource ctsParryHUD;
    Transform parryHUDWrap;
    async UniTask ParryHUD(int index, int total, CancellationToken token)
    {
        await UniTask.Yield(token);
        parryHUDWrap.gameObject.SetActive(true);
        float ratio = ((float)(index + 1)) / total;
        float multiplier = 0.4f * Mathf.Pow(ratio, 1.5f) + 0.3f + 0.02f * index;
        multiplier = Mathf.Clamp01(multiplier);
        float startTime = Time.time;
        //Debug.Log(multiplier);
        parryCanvasGroup.alpha = multiplier;
        while (!token.IsCancellationRequested && Time.time - startTime < 0.8f)
        {
            await UniTask.Yield(token);
            float tick = (Time.time - startTime) / 0.8f;
            parryCanvasGroup.alpha = multiplier * (1f - tick);
        }
        parryCanvasGroup.alpha = 0f;
        if (index + 1 != total) return;
        for (int i = 0; i < parryCountImages.Length; i++)
        {
            parryCountImages[i].color = Color.black;
        }
        parryCanvas.SetActive(false);
    }
    void ParryHandler(HitData hitData)
    {
        if (hitData.attacker != transform) return;
        if (isDie) return;
        if (state == State.Hit) return;
        if (hitData.attackType == HitData.AttackType.Chafe
        || hitData.attackType == HitData.AttackType.Bullet
        || hitData.attackType == HitData.AttackType.Trap)
            return;
        parryCount++;
        if (parryCount > data.ParryCount) parryCount = data.ParryCount;
        if (parryCanvas)
        {
            parryCanvas.SetActive(true);
            if (parryHUDWrap == null)
            {
                parryHUDWrap = parryCanvas.transform.Find("Wrap");
                parryHUDWrap.gameObject.SetActive(true);
                parryCanvasGroup = parryHUDWrap.GetComponent<CanvasGroup>();
                parryCountImages = new UnityEngine.UI.Image[parryHUDWrap.childCount];
                for (int i = 0; i < parryCountImages.Length; i++)
                {
                    parryCountImages[i] = parryHUDWrap.GetChild(i).GetComponent<UnityEngine.UI.Image>();
                    parryCountImages[i].color = Color.black;
                }
            }
            ctsParryHUD?.Cancel();
            ctsParryHUD = new CancellationTokenSource();
            CancellationTokenSource ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsParryHUD.Token);
            parryCountImages[parryCount - 1].color = parryCountColor;
            ParryHUD(parryCount - 1, data.ParryCount, ctsLink.Token).Forget();
        }
        if (state == State.SequenceAttack1) return;
        if (parryCount >= data.ParryCount)
        {
            //Debug.Log($"패링성공({parryCount}/{data.ParryCount}) 자세파괴");
            parryCount = 0;
            monsterHit.type = 2;
            monsterHit.prevState = state;
            ChangeState(State.Hit);
        }
    }
    class MatInfo
    {
        public SpriteRenderer spriteRenderer;
        public Material[] originalMats;
        public Sequence[] sequences;
    }
    List<MatInfo> matInfos = new List<MatInfo>();
    void InitMatInfo()
    {
        matInfos.Clear();
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < srs.Length; i++)
        {
            MatInfo matInfo = new MatInfo();
            matInfo.spriteRenderer = srs[i];
            matInfo.originalMats = srs[i].sharedMaterials;
            matInfo.sequences = new Sequence[srs[i].sharedMaterials.Length];
            matInfos.Add(matInfo);
        }
    }
    void HitChangeColor(Color color)
    {
        foreach (var element in matInfos)
        {
            Material[] newMats = new Material[element.spriteRenderer.materials.Length];
            // element변수의 로컬 복사본을 만듭니다 (클로저 문제방지)
            var currentElement = element;
            for (int i = 0; i < currentElement.originalMats.Length; i++)
            {
                // 루프변수i의 로컬 복사본을 만듭니다 (클로저 문제방지)
                int materialIndex = i;
                if (currentElement.sequences[materialIndex] != null && currentElement.sequences[materialIndex].IsActive())
                    currentElement.sequences[materialIndex].Kill();
                newMats[materialIndex] = Instantiate(GameManager.I.hitTintMat);
                newMats[materialIndex].color = currentElement.originalMats[materialIndex].color;
                newMats[materialIndex].SetColor("_TintColor", new Color(color.r, color.g, color.b, 1f));
                currentElement.sequences[materialIndex] = DOTween.Sequence();
                currentElement.sequences[materialIndex].AppendInterval(0.08f);
                Tween colorTween = newMats[materialIndex].DOVector(
                    new Vector4(color.r, color.g, color.b, 0f),
                    "_TintColor",
                    0.22f
                ).SetEase(Ease.OutBounce);
                currentElement.sequences[materialIndex].Append(colorTween);
                currentElement.sequences[materialIndex].OnComplete(() =>
                {
                    Material[] currentMats = currentElement.spriteRenderer.materials;
                    currentMats[materialIndex] = currentElement.originalMats[materialIndex];
                    currentElement.spriteRenderer.materials = currentMats;
                    // 인스턴스화된 hitTintMat을 제거합니다. (메모리 누수 방지)
                    Destroy(newMats[materialIndex]);
                });
                currentElement.sequences[materialIndex].Play();
            }
            currentElement.spriteRenderer.materials = newMats;
        }
    }
    #endregion

    [Space(50)]
    public Vector2 startPosition;
    [Range(0, 1)] public float homeValue = 1f;



}
