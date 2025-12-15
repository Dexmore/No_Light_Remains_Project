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
        if(transform.Find("Chafe") != null)
            chafe = transform.Find("Chafe").gameObject;
        else
            chafe = transform.GetChild(0).Find("Chafe").gameObject;
        chafe?.SetActive(false);
        Activate(token).Forget();
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
        Destroy(gameObject);
    }
    public override void Exit()
    {
        base.Exit();
        chafe?.SetActive(true);
    }






}
