using System.Threading.Tasks;
using UnityEngine;
public class DropItem : Interactable
{
    public override Type type => Type.DropItem;
    public override bool isReady { get; set; }
    public override bool isAuto => _isAuto;
    public bool _isAuto = true;
    public ItemData itemData;
    public GearData gearData;
    public LanternFunctionData lanternData;
    public RecordData recordData;
    public int gold;
    bool isRun = false;

    public LayerMask groundLayer;
    Rigidbody2D rb;
    PlayerControl player;
    void Awake()
    {
        TryGetComponent(out rb);
        player = FindAnyObjectByType<PlayerControl>();
    }
    protected virtual void OnEnable()
    {
        isReady = false;
        isRun = false;
        sfxTime = Time.time;
        Wait2();
    }
    public override void Run()
    {
        if (!isReady) return;
        if (isRun) return;
        isRun = true;
        isReady = false;
        Rooting();
    }
    Camera _mainCamera;
    async void Rooting()
    {
        float startTime = Time.time;
        float duration = Random.Range(0.6f, 1.1f);
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        transform.GetChild(0).gameObject.SetActive(false);
        while (Time.time - startTime < duration)
        {
            float ratio = (Time.time - startTime) / duration;
            transform.position = Vector2.Lerp((Vector2)transform.position, (Vector2)player.transform.position + 0.6f * Vector2.up, (3.6f + 20f * ratio) * Time.deltaTime);
            if ((transform.position - player.transform.position).magnitude < 0.7f) break;
            await Task.Delay((int)(1000f * Time.deltaTime));
        }
        if (gold > 0)
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            UIParticle upa = ParticleManager.I.PlayUIParticle("UIAttGold", MethodCollection.WorldTo1920x1080Position(transform.position, _mainCamera), Quaternion.identity);
            AttractParticle ap = upa.GetComponent<AttractParticle>();
            Vector3 pos = _mainCamera.ViewportToWorldPoint(new Vector3(1.4f, 1f, 0f));
            ap.targetVector = pos;
        }
        AudioManager.I.PlaySFX("GetItem");
        DBManager.I.currData.gold += gold;
        bool outBool = false;
        if (itemData != null)
        {
            DBManager.I.AddItem(itemData.name);
        }
        else if (gearData != null)
        {
            if (DBManager.I.HasGear(gearData.name, out outBool))
            {
                if (outBool)
                {
                    isRun = false;
                    isReady = true;
                    return;
                }
            }
            DBManager.I.AddGear(gearData.name);
        }
        else if (lanternData != null)
        {
            if (DBManager.I.HasGear(lanternData.name, out outBool))
            {
                if (outBool)
                {
                    isRun = false;
                    isReady = true;
                    return;
                }
            }
            DBManager.I.AddLantern(lanternData.name);
        }
        else if (recordData != null)
        {
            if (DBManager.I.HasGear(recordData.name, out outBool))
            {
                if (outBool)
                {
                    isRun = false;
                    isReady = true;
                    return;
                }
            }
            DBManager.I.AddRecord(recordData.name);
        }
        isReady = false;
        await Task.Delay(10);
        Destroy(gameObject);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundLayer & (1 << collision.gameObject.layer)) == 0) return;
        if (Time.time - sfxTime > 0.5f)
        {
            sfxTime = Time.time;
            AudioManager.I.PlaySFX("Tick1");
        }
        if (!isReady && !flag)
        {
            flag = true;
            Wait1();
        }
    }
    bool flag = false;
    float sfxTime;
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
