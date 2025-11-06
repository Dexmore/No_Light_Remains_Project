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
        public int money;
        public Vector2Int countRange;
        [Range(0f, 1f)] public float probability;
    }
    [SerializeField] DropTable[] dropTables;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        chafe = transform.Find("Chafe").gameObject;
        chafe?.SetActive(false);
        Activate(token).Forget();
    }
    public async UniTask Activate(CancellationToken token)
    {
        control.isDie = true;
        await UniTask.Yield(cts.Token);
        anim.Play("Die");
        await UniTask.Delay((int)(1000f * duration), cancellationToken: token);
        // 아이템 드롭
        foreach (var element in dropTables)
        {
            if (Random.value > element.probability) continue;
            int count = Random.Range(element.countRange.x, element.countRange.y);
            for (int k = 0; k < count; k++)
            {
                DropItem dropItem = Instantiate(element.dropItem);
                dropItem.money = element.money;
                dropItem.transform.position = transform.position;
                Rigidbody2D rigidbody2D = dropItem.GetComponentInChildren<Rigidbody2D>();
                if (rigidbody2D != null)
                {
                    rigidbody2D.AddForce(10f * Vector2.up, ForceMode2D.Impulse);
                }
            }
            await UniTask.Delay(10, cancellationToken: cts.Token);
        }
        await UniTask.Yield(cts.Token);
        Destroy(gameObject);
    }
    public override void Exit()
    {
        base.Exit();
        chafe?.SetActive(true);
    }






}
