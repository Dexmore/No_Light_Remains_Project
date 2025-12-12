using System.Threading.Tasks;
using UnityEngine;
public class DropItem : Interactable
{
    public override Type type => Type.DropItem;
    public override bool isReady { get; set; }
    public bool isAuto = true;
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
    void OnEnable()
    {
        isReady = false;
        isRun = false;
        sfxTime = Time.time;
        Wait2();
    }
    public void Get()
    {
        if (!isReady) return;
        if (isRun) return;
        isRun = true;
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
            UIParticle upa = ParticleManager.I.PlayUIParticle("AttGold", transform.position, Quaternion.identity);
            AttractParticle ap = upa.GetComponent<AttractParticle>();
            if (_mainCamera == null) _mainCamera = Camera.main;
            Vector3 pos = ParticleManager.I.vfxCamera.ViewportToWorldPoint(new Vector3(0.92f, 0.88f, 0f));
                ap.targetVector = pos;
        }
        AudioManager.I.PlaySFX("GetItem");
        DBManager.I.currData.gold += gold;
        if (itemData != null)
        {
            DBManager.I.AddItem(itemData.name);
        }
        else if (gearData != null)
        {
            DBManager.I.AddGear(gearData.name);
        }
        else if (lanternData != null)
        {
            DBManager.I.AddLantern(lanternData.name);
        }
        else if (recordData != null)
        {
            DBManager.I.AddRecord(recordData.name);
        }
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
