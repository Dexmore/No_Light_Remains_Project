using UnityEngine;
using UnityEngine.UI;

public class HUDBinder : MonoBehaviour
{
    // Player 오브젝트 지정
    PlayerControl player;          
    SlicedLiquidBar healthBar;
    CanvasGroup currencyCG;
    
    void Awake()
    {
        if (!player) player = FindFirstObjectByType<PlayerControl>();
        healthBar = GetComponentInChildren<SlicedLiquidBar>();
        currencyCG = transform.Find("HUDCanvas/TopRight/Currency").GetComponent<CanvasGroup>();
        currencyCG.alpha = 0f;
    }
    void OnEnable()
    {
        GameManager.I.onHitAfter += HandleHit;
    }
    void OnDisable()
    {
        GameManager.I.onHitAfter -= HandleHit;
    }
    void HandleHit(HitData hitData)
    {
        if (hitData.target.Root() != player.transform) return;
        if (player.fsm.currentState == player.die) return;

        healthBar.Value = Mathf.Clamp01(player.currentHealth / player.maxHealth);

        RectTransform rect = healthBar.transform as RectTransform;
        Vector2 particlePos = Vector2.zero;
        Vector2 pivot = MethodCollection.Absolute1920x1080Position(rect);
        float x = pivot.x - 0.5f * rect.sizeDelta.x;
        float y = pivot.y;
        float addX = healthBar.xPosRange.x + (healthBar.xPosRange.y - healthBar.xPosRange.x) * healthBar.Value;
        particlePos = new Vector2(x + addX, y);
        if (hitData.attackType == HitData.AttackType.Chafe)
        {
            var pa = ParticleManager.I.PlayUIParticle("Gush3", particlePos, Quaternion.identity);
            pa.transform.localScale = 0.5f * Vector3.one;
        }
        else
        {
            var pa = ParticleManager.I.PlayUIParticle("Gush3", particlePos, Quaternion.identity);
            pa.transform.localScale = 0.8f * Vector3.one;
        }
        var pa2 = ParticleManager.I.PlayUIParticle("Gush", particlePos, Quaternion.identity);
        pa2.transform.localScale = 0.7f * Vector3.one;
        var main = pa2.ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.59f,0.159f,0.196f,1f), new Color(0.5f,0.05f,0.05f,1f));


    }


}
