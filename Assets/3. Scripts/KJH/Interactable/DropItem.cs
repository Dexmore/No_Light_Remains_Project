using System.Threading.Tasks;
using UnityEngine;
public class DropItem : Interactable
{
    public override Type type => Type.DropItem;
    
    public bool isAuto = true;
    public ItemData itemData;
    public int money;
    bool isRun = false;
    public override bool isReady { get; protected set;}
    public LayerMask groundLayer;
    void Start()
    {
        isReady = false;
        isRun = false;
    }
    public void Get()
    {
        if (!isReady) return;
        if (isRun) return;
        isRun = true;
        Destroy(gameObject);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundLayer & (1 << collision.gameObject.layer)) == 0) return;
        if (!isReady && !flag)
        {
            flag = true;
            GroundReady();
        }
    }
    bool flag = false;
    async void GroundReady()
    {
        await Task.Delay(Random.Range(1500, 2500));
        isReady = true;
    }


}
