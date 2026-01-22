using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;
public class MonsterDie : MonsterState
{
    public float duration = 0.4f;
    public override MonsterControl.State mapping => MonsterControl.State.Die;
    GameObject chafe;
    [System.Serializable]
    public struct DropTable
    {
        public DropItem dropItem;
        public int gold;
        public RecordData record;
        public Vector2Int countRange;
        [Range(0f, 1f)] public float probability;
    }
    [SerializeField] DropTable[] dropTables;
    public override async UniTask Enter(CancellationToken token)
    {
        control.isDie = true;
        await UniTask.Yield(token);
        if (transform.Find("Chafe") != null)
            chafe = transform.Find("Chafe").gameObject;
        else
            chafe = transform.GetChild(0).Find("Chafe").gameObject;
        chafe?.SetActive(false);
        Activate(token).Forget();

        // if (DBManager.I.currData.sceneDatas != null)
        // {
        //     if (transform.name.Contains("("))
        //     {
        //         string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        //         int find1 = DBManager.I.currData.sceneDatas.FindIndex(x => x.sceneName == sceneName);
        //         string strimedName = transform.name.Split("(")[0];
        //         if (find1 != -1)
        //         {
        //             if (int.TryParse(transform.name.Split("(")[1].Split(")")[0], out int result))
        //             {
        //                 int find2 = DBManager.I.currData.sceneDatas[find1].monsterPositionDatas.FindIndex(x => x.Name == strimedName && x.index == result);
        //                 if (find2 != -1)
        //                 {
        //                     var monsterList = DBManager.I.currData.sceneDatas[find1].monsterPositionDatas;
        //                     var mData = monsterList[find2];
        //                     System.DateTime now = System.DateTime.Now;
        //                     string datePart = now.ToString("yyyy.MM.dd");
        //                     int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
        //                     mData.lastDeathTime = $"{datePart}-{secondsOfDay}";
        //                     mData.lastHealth = 0;
        //                     monsterList[find2] = mData;
        //                 }
        //             }
        //         }
        //         if (DBManager.I.currData.killCounts != null && DBManager.I.currData.killCounts.Count > 0)
        //         {
        //             find1 = DBManager.I.currData.killCounts.FindIndex(x => x.Name == strimedName);
        //             if (find1 != -1)
        //             {
        //                 var killCount = DBManager.I.currData.killCounts[find1];
        //                 killCount.count++;
        //                 DBManager.I.currData.killCounts[find1] = killCount;
        //             }
        //             else
        //             {
        //                 var killCount = new CharacterData.KillCount();
        //                 killCount.Name = strimedName;
        //                 killCount.count = 1;
        //                 DBManager.I.currData.killCounts.Add(killCount);
        //             }
        //         }
        //     }
        // }


    }
    public async UniTask Activate(CancellationToken token)
    {
        await UniTask.Yield(token);
        anim.Play("Die");
        await UniTask.Delay((int)(1000f * duration), cancellationToken: token);
        string strimedName = transform.name.Split("(")[0];

        int killCount = 0;
        if (DBManager.I.currData.killCounts == null)
        {
            DBManager.I.currData.killCounts = new System.Collections.Generic.List<CharacterData.KillCount>();
            CharacterData.KillCount newData = new CharacterData.KillCount();
            newData.Name = strimedName;
            newData.count = 1;
            DBManager.I.currData.killCounts.Add(newData);
        }
        else
        {
            int find = DBManager.I.currData.killCounts.FindIndex(x => x.Name == strimedName);
            if (find != -1)
            {
                killCount = DBManager.I.currData.killCounts[find].count;
            }
            else
            {
                CharacterData.KillCount newData = new CharacterData.KillCount();
                newData.Name = strimedName;
                newData.count = 1;
                DBManager.I.currData.killCounts.Add(newData);
            }
        }

        HUDBinder hUDBinder = FindFirstObjectByType<HUDBinder>();
        // 아이템 드롭
        foreach (var element in dropTables)
        {
            if (Random.value > element.probability) continue;
            if (element.probability <= 0.1f)
            {
                // 유니크 아이템이 한마리 잡고 바로 나오는 행위방지
                float MinExpectation = (1 / element.probability) * 0.1f;
                if (killCount < MinExpectation) continue;
            }
            if (element.dropItem == null && element.record != null)
            {
                if (DBManager.I.HasRecord(element.record.name))
                {
                    //Debug.Log($"{dropInfo.recordData.name}는 이미 가지고 있습니다. 습득불가");
                    continue;
                }
                DBManager.I.AddRecord(element.record.name);
                hUDBinder.PlayNoticeText(3);
            }
            else
            {
                DropItem dropInfo = element.dropItem;
                if (dropInfo.gearData != null)
                {
                    bool outValue;
                    if (DBManager.I.HasGear(dropInfo.gearData.name, out outValue))
                    {
                        //Debug.Log($"{dropInfo.gearData.name}는 이미 가지고 있습니다. 드롭불가");
                        continue;
                    }
                }
                else if (dropInfo.lanternData != null)
                {
                    bool outValue;
                    if (DBManager.I.HasLantern(dropInfo.lanternData.name, out outValue))
                    {
                        //Debug.Log($"{dropInfo.lanternData.name}는 이미 가지고 있습니다. 드롭불가");
                        continue;
                    }
                }
                else if (dropInfo.recordData != null)
                {
                    bool outValue;
                    if (DBManager.I.HasRecord(dropInfo.recordData.name))
                    {
                        //Debug.Log($"{dropInfo.recordData.name}는 이미 가지고 있습니다. 드롭불가");
                        continue;
                    }
                }
                AudioManager.I.PlaySFX("Tick1");
                int count = Random.Range(element.countRange.x, element.countRange.y + 1);
                for (int k = 0; k < count; k++)
                {
                    DropItem dropItem = Instantiate(element.dropItem);
                    dropItem.gold = element.gold;
                    dropItem.transform.position = transform.position;
                    Rigidbody2D rigidbody2D = dropItem.GetComponentInChildren<Rigidbody2D>();
                    if (rigidbody2D != null)
                    {
                        Vector2 dir = Quaternion.Euler(0f, 0f, Random.Range(5f, 15f)) * Vector2.up;
                        if (Random.value <= 0.5f) dir.x = -dir.x;
                        rigidbody2D.AddForce(Random.Range(4f, 8.5f) * dir, ForceMode2D.Impulse);
                    }
                }
            }

            await UniTask.Delay(10, cancellationToken: token);
        }

        string strimedName1 = transform.name.Split("(")[0];
        //Debug.Log(strimedName1);
        switch (strimedName1)
        {
            case "ContaminatedFlower":
                // int tempCount = DBManager.I.GetKillcount(strimedName1);
                // if (tempCount >= 20)
                // {
                //     DBManager.I.SteamAchievement("ACH_FLOWER_KILL_20");
                // }
                // if (tempCount >= 100)
                // {
                //     DBManager.I.SteamAchievement("ACH_FLOWER_KILL_100");
                // }
                break;

            case "LanternKeeper":
            DBManager.I.SteamAchievement("ACH_BOSS_LANTERN_KILL");
                // if (DBManager.I.currData.difficulty >= 1)
                //     if (DBManager.I.currData.maxPotionCount == DBManager.I.currData.currPotionCount)
                //     {
                //         DBManager.I.SteamAchievement("ACH_BOSS_LANTERN_NO_POTION_HARD");
                //     }
                break;
        }

        await UniTask.Yield(token);
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }
    public override void Exit()
    {
        base.Exit();
        chafe?.SetActive(true);
    }






}
