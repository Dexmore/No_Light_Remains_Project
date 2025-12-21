using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
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
        if (DBManager.I.currData.sceneDatas != null)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            int find1 = DBManager.I.currData.sceneDatas.FindIndex(x => x.sceneName == sceneName);
            if (find1 != -1)
            {
                string strimedName = transform.name.Split("(")[0];
                if (int.TryParse(transform.name.Split("(")[1].Split(")")[0], out int result))
                {
                    int find2 = DBManager.I.currData.sceneDatas[find1].monsterPositionDatas.FindIndex(x => x.Name == strimedName && x.index == result);
                    if (find2 != -1)
                    {
                        var monsterList = DBManager.I.currData.sceneDatas[find1].monsterPositionDatas;
                        var mData = monsterList[find2];
                        System.DateTime now = System.DateTime.Now;
                        string datePart = now.ToString("yyyy.MM.dd");
                        int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
                        mData.lastDeathTime = $"{datePart}-{secondsOfDay}";
                        mData.lastHealth = 0;
                        monsterList[find2] = mData;
                    }
                }
            }
        }
    }
    public async UniTask Activate(CancellationToken token)
    {
        await UniTask.Yield(token);
        anim.Play("Die");
        await UniTask.Delay((int)(1000f * duration), cancellationToken: token);
        // 아이템 드롭
        foreach (var element in dropTables)
        {
            if (Random.value > element.probability) continue;
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
            await UniTask.Delay(10, cancellationToken: token);
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
