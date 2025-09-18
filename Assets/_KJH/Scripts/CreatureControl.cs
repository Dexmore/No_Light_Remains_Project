using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using State = CreatureData.State;
using Condition = CreatureData.Condition;
public class CreatureControl : MonoBehaviour
{
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
    public CreatureData data;
    [HideInInspector] public float width;
    [HideInInspector] public float height;
    Astar2DXYPathFinder astar;
    void Awake()
    {
        SettingFSM();
        TryGetComponent(out astar);
    }
    void Init()
    {
        if (astar)
        {
            astar.characterHeight = height;
            astar.characterWidth = width;
            astar.tileUnit = Mathf.Clamp(width * 0.33f, 0.5f, 3f);
        }
        // 게임 시작시 상태를 Idle 또는 Wander로
        if (Random.value < 0.5f)
            ChangeState(State.Idle);
        else
            ChangeState(State.Wander);
        // 게임 시작시 Default에 해당하는 동작들이 choisables에 등록
        condition = Condition.Peaceful;
    }
    #region FSM
    [ReadOnlyInspector] public State state;
    [HideInInspector] public State prevState;
    [HideInInspector] public Dictionary<State, CreatureAbility> dictionary = new Dictionary<State, CreatureAbility>();
    void SettingFSM()
    {
        CreatureAbility[] abilities = GetComponents<CreatureAbility>();
        for (int j = 0; j < System.Enum.GetValues(typeof(State)).Length; j++)
        {
            bool temp = false;
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].mapping == (State)j)
                {
                    dictionary.Add((State)j, abilities[i]);
                    abilities[i].enabled = false;
                    temp = true;
                    break;
                }
            }
        }
    }
    public void ChangeState(State newState)
    {
        ChangeState_ut(newState, cts.Token).Forget();
    }
    async UniTask ChangeState_ut(State newState, CancellationToken token)
    {
        // 이전 state 스크립트는 Disable 처리
        if (dictionary.Count == 0) return;
        dictionary[state].UnInit();
        dictionary[state].enabled = false;
        await UniTask.Yield(token);
        prevState = state;
        state = newState;
        // 변경할 state 스크립트는 Enable 처리
        dictionary[state].enabled = true;
        dictionary[state].cts?.Cancel();
        dictionary[state].cts = new CancellationTokenSource();
        dictionary[state].Init(dictionary[state].cts.Token).Forget();
        //Debug.Log($"{transform.name},{GetInstanceID()}] --> {state} 시작");
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
        if (!dictionary.ContainsKey(state)) return;
        string tName = dictionary[state].GetType().ToString();
        int find = cannotList.FindIndex(o => o.abilityName == tName && o.reason == reason);
        if (find != -1) return;
        CanNot element = new CanNot(tName, reason);
        cannotList.Add(element);
    }
    public void RemoveCanNot(System.Type type, string reason)
    {
        string tName = type.Name;
        int find = cannotList.FindIndex(o => o.abilityName == tName && o.reason == reason);
        if (find == -1) return;
        cannotList.RemoveAt(find);
    }
    public void RemoveCanNot(State state, string reason)
    {
        if (!dictionary.ContainsKey(state)) return;
        RemoveCanNot(dictionary[state].GetType(), reason);
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
        if (!dictionary.ContainsKey(state)) return false;
        string tName = dictionary[state].GetType().ToString();
        int find = cannotList.ToList().FindIndex(o => o.abilityName == tName);
        if (find != -1) return false;
        return true;
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
        if (!dictionary.ContainsKey(state)) return;
        string tName = dictionary[state].GetType().ToString();
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
                coolTime.type = dictionary[state].GetType();
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
        if (!dictionary.ContainsKey(state)) return;
        string tName = dictionary[state].GetType().ToString();
        int find = coolTimeList.FindIndex(x => x.abilityName == tName);
        if (find == -1) return;
        coolTimeList[find].isPause = true;
    }
    public void RemoveCoolTime(State state)
    {
        if (!dictionary.ContainsKey(state)) return;
        string tName = dictionary[state].GetType().ToString();
        int find = coolTimeList.FindIndex(x => x.abilityName == tName);
        if (find == -1) return;
        coolTimeList[find].isPause = false;
    }
    public bool IsCoolTime(State state)
    {
        if (!dictionary.ContainsKey(state)) return false;
        string tName = dictionary[state].GetType().ToString();
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

    // // Condition가 새로 바뀌면 condition에 매핑된 Condition가 해당되는 상태스크립트들은 전부 '랜덤 쿨타임'이 돌아가는거에서 시작 합니다.
    // // 이걸 하는 이유는 몹이 플레이어를 발견하자마자 고득점을 받기좋은 필살기를 쓴다던다 하는것들 막기 위함임.
    // // 즉 플레이어를 발견하자마자 몹 내부에서는 100초짜리 쿨타임을 가진 동작이라면 이미 0~100 초 사이의 랜덤한 쿨타임이 돌아가고 있으므로
    // // 전투 시작시에는 일단 기본적인 평타 위주로 쓸 가능성이 더 높음. 한창 싸우다가 몹이 100초 간격으로 필살기급의 공격을 쓰도록 유도
    // public void SetRandomCoolTime(Condition condition)
    // {
    //     foreach (var element in dictionary)
    //     {
    //         if ((element.Value.mapping2 & condition) != 0)
    //         {
    //             if (element.Value.coolTime > 1f)
    //             {
    //                 if (!IsCoolTime(element.Key))
    //                 {
    //                     SetCoolTime(element.Key, Random.Range(0f, element.Value.coolTime));
    //                 }
    //                 else
    //                 {
    //                     RemoveCoolTime(element.Key);
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             if (element.Value.coolTime > 1f)
    //                 if (IsCoolTime(element.Key))
    //                     PauseCoolTime(element.Key);
    //         }
    //     }
    // }
    #endregion
    #region Delay
    #endregion



















}