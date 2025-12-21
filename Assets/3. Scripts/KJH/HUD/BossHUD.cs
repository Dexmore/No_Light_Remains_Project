using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BossHUD : MonoBehaviour
{
    [ReadOnlyInspector] public MonsterControl target;
    Transform canvas;
    TMP_Text textName;
    SlicedLiquidBar slicedLiquidBar;
    Image barImage;
    bool isFirst = true;
    void Awake()
    {
        canvas = transform.GetChild(0);
        transform.Find("Canvas/Wrap/Text(BossName)").TryGetComponent(out textName);
        slicedLiquidBar = GetComponentInChildren<SlicedLiquidBar>(true);
        barImage = slicedLiquidBar.GetComponent<Image>();
        currColor = phase1Color;
        barImage.color = currColor;
        isFirst = true;
        canvas.Find("Opening").gameObject.SetActive(false);
        canvas.Find("Wrap").gameObject.SetActive(false);
        canvas.gameObject.SetActive(false);
    }
    void OnEnable()
    {
        GameManager.I.onHit += HitHandler;
    }
    void OnDisable()
    {
        GameManager.I.onHit -= HitHandler;
    }
    void OnDestroy()
    {
        currColor = phase1Color;
        barImage.color = currColor;
    }
    public void SetTarget(MonsterControl target)
    {
        if (target == null)
        {
            canvas.gameObject.SetActive(false);
        }
        if (this.target != target)
        {
            this.target = target;
            if (isFirst)
            {
                isFirst = false;
                StartCoroutine(nameof(Opening));
            }
            //
            float ratio = target.currHealth / target.maxHealth;
            slicedLiquidBar.Value = ratio;
            textName.text = target.data.Name;
            if (slicedLiquidBar.Value > 0.7f && currColor != phase1Color)
            {
                currColor = phase1Color;
                barImage.color = currColor;
            }
            else if (slicedLiquidBar.Value > 0.4f && slicedLiquidBar.Value <= 0.7f && currColor != phase2Color)
            {
                currColor = phase2Color;
                barImage.color = currColor;
            }
            else if (slicedLiquidBar.Value > 0.23f && slicedLiquidBar.Value <= 0.4f && currColor != phase3Color)
            {
                currColor = phase3Color;
                barImage.color = currColor;
            }
            else if (slicedLiquidBar.Value <= 0.23f && currColor != moribundColor)
            {
                currColor = moribundColor;
                barImage.color = currColor;
            }
        }
    }
    Color currColor;
    Color phase1Color = new Color(0.27f, 0.64f, 0.03f, 1f);
    Color phase1MatColor = new Color(0.2f, 0.45f, 0.08f, 1f);
    Color phase2Color = new Color(0.68f, 0.7f, 0.01f, 1f);
    Color phase2MatColor = new Color(0.68f, 0.55f, 0.06f, 1f);
    Color phase3Color = new Color(0.7f, 0.24f, 0.05f, 1f);
    Color phase3MatColor = new Color(0.78f, 0.35f, 0.04f, 1f);
    Color moribundColor = new Color(1f, 0f, 0f, 1f);
    Color moribundMatColor = new Color(0.78f, 0.13f, 0.09f, 1f);
    Transform hPBarFill;
    IEnumerator Opening()
    {
        yield return YieldInstructionCache.WaitForSeconds(2f);
        canvas.gameObject.SetActive(true);
        canvas.Find("Opening").gameObject.SetActive(true);
        canvas.Find("Wrap").gameObject.SetActive(false);
        yield return YieldInstructionCache.WaitForSeconds(2f);
        canvas.Find("Opening").gameObject.SetActive(false);
        canvas.Find("Wrap").gameObject.SetActive(true);
    }
    IEnumerator Closing()
    {
        yield return YieldInstructionCache.WaitForSeconds(2f);
        yield return YieldInstructionCache.WaitForSeconds(2f);
    }
    void HitHandler(HitData hData)
    {
        if (target == null) return;
        if (target.isDie) return;
        if (hData.target.Root() != target.transform) return;
        float ratio = target.currHealth / target.maxHealth;
        slicedLiquidBar.Value = ratio;
        RectTransform rect = slicedLiquidBar.transform as RectTransform;
        Vector2 particlePos = Vector2.zero;
        Vector2 pivot = MethodCollection.RectTo1920x1080Position(rect);
        float x = pivot.x - 0.5f * rect.sizeDelta.x;
        float y = pivot.y;
        float addX = slicedLiquidBar.xPosRange.x + (slicedLiquidBar.xPosRange.y - slicedLiquidBar.xPosRange.x) * ratio;
        particlePos = new Vector2(x + addX, y);
        UIParticle uIParticle = ParticleManager.I.PlayUIParticle("UIGush", particlePos, Quaternion.identity);
        Color color1 = Color.Lerp(currColor, phase1Color, 0.5f);
        Color color2 = currColor * 0.6f;
        var main = uIParticle.ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color1, color2);
        uIParticle = ParticleManager.I.PlayUIParticle("UIGush2", particlePos, Quaternion.identity);
        main = uIParticle.ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color1, color2);
        if (slicedLiquidBar.Value > 0.7f && currColor != phase1Color)
        {
            currColor = phase1Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value > 0.4f && slicedLiquidBar.Value <= 0.7f && currColor != phase2Color)
        {
            currColor = phase2Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value > 0.23f && slicedLiquidBar.Value <= 0.4f && currColor != phase3Color)
        {
            currColor = phase3Color;
            barImage.color = currColor;
        }
        else if (slicedLiquidBar.Value <= 0.23f && currColor != moribundColor)
        {
            currColor = moribundColor;
            barImage.color = currColor;
        }
    }




}
