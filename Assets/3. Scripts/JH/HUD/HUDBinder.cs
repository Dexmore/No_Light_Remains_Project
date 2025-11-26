using UnityEngine;

public class HUDBinder : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;          // Player 오브젝트 지정
    public HealthBar healthBar;         // HUD_RuntimeHooks에 있는 컴포넌트 참조
    public LighthouseBar lighthouseBar; // 등대게이지 
    public CurrencyUI currencyUI;       // 보여지고 있는 UI 

    void Awake()
    {
        if (!player)
        {
            player = FindObjectOfType<PlayerController>();
        }
        if (!healthBar)     healthBar     = FindObjectOfType<HealthBar>();
        if (!lighthouseBar) lighthouseBar = FindObjectOfType<LighthouseBar>();
        if (!currencyUI)    currencyUI    = FindObjectOfType<CurrencyUI>();
    }

    void OnEnable()
    {
        if (!player) return;

        // player.OnHPChanged     += HandleHP;
        // player.OnLightChanged  += HandleLight;
        // player.OnGoldChanged   += HandleGold;
    }
    void OnDisable()
    {
        if (!player) return;

        // player.OnHPChanged     -= HandleHP;
        // player.OnLightChanged  -= HandleLight;
        // player.OnGoldChanged   -= HandleGold;
    }

    void Update()
    {
        HandleHP(player.currentHealth, player.maxHealth);
    }


    void HandleHP(float cur, float max)
        => healthBar?.Set01(max > 0 ? cur / max : 0f);

    void HandleLight(float cur, float max)
        => lighthouseBar?.Set01(max > 0 ? cur / max : 0f);

    void HandleGold(int amount)
        => currencyUI?.SetAmount(amount);
    

}
