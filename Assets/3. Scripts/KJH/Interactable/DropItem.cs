using System.Threading.Tasks;
using UnityEngine;
public class DropItem : Interactable
{
    public override Type type => Type.DropItem;
    
    public bool isAuto = true;
    public ItemData itemData;
    public GearData gearData;
    public LanternFunctionData lanternData;
    public int money;
    bool isRun = false;
    public override bool isReady { get; protected set;}
    public LayerMask groundLayer;
    Rigidbody2D rb;
    PlayerController_LSH player;
    void Awake()
    {
        TryGetComponent(out rb);
        player = FindAnyObjectByType<PlayerController_LSH>();
    }
    void Start()
    {
        isReady = false;
        isRun = false;
    }
    void OnEnable()
    {
        Wait2();
    }
    public void Get()
    {
        if (!isReady) return;
        if (isRun) return;
        isRun = true;
        Rooting();
    }
    async void Rooting()
    {
        float startTime = Time.time;
        float duration = Random.Range(0.6f, 1.1f);
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        transform.GetChild(0).gameObject.SetActive(false);
        while(Time.time - startTime < duration)
        {
            float ratio = (Time.time - startTime)/duration;
            transform.position = Vector2.Lerp((Vector2)transform.position, (Vector2)player.transform.position + 0.6f * Vector2.up , (3.6f + 20f * ratio) * Time.deltaTime);
            if((transform.position - player.transform.position).magnitude < 1.2f) break;
            await Task.Delay((int) (1000f * Time.deltaTime));
        }
        AudioManager.I.PlaySFX("GetItem");
        Destroy(gameObject);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundLayer & (1 << collision.gameObject.layer)) == 0) return;
        if (!isReady && !flag)
        {
            flag = true;
            Wait1();
        }
    }
    bool flag = false;
    async void Wait1()
    {
        await Task.Delay(Random.Range(600, 1800));
        isReady = true;
    }
    async void Wait2()
    {
        await Task.Delay(Random.Range(2300, 4500));
        isReady = true;
    }


}
