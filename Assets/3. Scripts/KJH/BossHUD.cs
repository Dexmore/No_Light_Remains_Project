using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BossHUD : MonoBehaviour
{
    [ReadOnlyInspector][SerializeField] MonsterControl _target;
    Transform canvas;
    TMP_Text textName;
    SlicedLiquidBar slicedLiquidBar;
    Image barImage;
    void Awake()
    {
        canvas = transform.GetChild(0);
        transform.Find("Canvas/Bottom/Text(BossName)").TryGetComponent(out textName);
        slicedLiquidBar = GetComponentInChildren<SlicedLiquidBar>(true);
        barImage = slicedLiquidBar.GetComponent<Image>();
        currColor = phase1Color;
        barImage.color = currColor;
        barImage.material.SetColor("_LiquidColor", phase1MatColor);
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
        barImage.material.SetColor("_LiquidColor", phase1MatColor);
    }
    public void SetTarget(MonsterControl target)
    {
        if (target == null)
        {
            canvas.gameObject.SetActive(false);
        }
        if (_target != target)
        {
            _target = target;
            canvas.gameObject.SetActive(true);
            textName.text = target.data.Name;
            if (slicedLiquidBar.Value > 0.7f && currColor != phase1Color)
            {
                currColor = phase1Color;
                barImage.color = currColor;
                barImage.material.SetColor("_LiquidColor", phase1MatColor);
            }
            else if (slicedLiquidBar.Value > 0.4f && slicedLiquidBar.Value <= 0.7f && currColor != phase2Color)
            {
                currColor = phase2Color;
                barImage.color = currColor;
                barImage.material.SetColor("_LiquidColor", phase2MatColor);
            }
            else if (slicedLiquidBar.Value > 0.12f && slicedLiquidBar.Value <= 0.4f && currColor != phase3Color)
            {
                currColor = phase3Color;
                barImage.color = currColor;
                barImage.material.SetColor("_LiquidColor", phase3MatColor);
            }
            else if (slicedLiquidBar.Value <= 0.12f && currColor != moribundColor)
            {
                currColor = moribundColor;
                barImage.color = currColor;
                barImage.material.SetColor("_LiquidColor", moribundMatColor);
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
    Color moribundColor = new Color(0.69f, 0.13f, 0.05f, 1f);
    Color moribundMatColor = new Color(0.78f, 0.13f, 0.09f, 1f);
    Transform hPBarFill;
    void HitHandler(HitData hData)
    {
        if (_target == null) return;
        if (_target.isDie) return;
        if (hData.target.Root() != _target.transform) return;
        float ratio = _target.currHP / _target.data.HP;
        slicedLiquidBar.Value = ratio;
        RectTransform rect = slicedLiquidBar.transform as RectTransform;
        Vector2 particlePos = Vector2.zero;
        Vector2 pivot = MethodCollection.Absolute1920x1080Position(rect);
        float x = pivot.x - 0.5f * rect.sizeDelta.x;
        float y = pivot.y;
        float addX = slicedLiquidBar.xPosRange.x + (slicedLiquidBar.xPosRange.y - slicedLiquidBar.xPosRange.x) * ratio;
        particlePos = new Vector2(x + addX, y);
        UIParticle uIParticle = ParticleManager.I.PlayUIParticle("Gush", particlePos, Quaternion.identity);
        if (slicedLiquidBar.Value > 0.7f && currColor != phase1Color)
        {
            currColor = phase1Color;
            barImage.color = currColor;
            barImage.material.SetColor("_LiquidColor", phase1MatColor);
        }
        else if (slicedLiquidBar.Value > 0.4f && slicedLiquidBar.Value <= 0.7f && currColor != phase2Color)
        {
            currColor = phase2Color;
            barImage.color = currColor;
            barImage.material.SetColor("_LiquidColor", phase2MatColor);
        }
        else if (slicedLiquidBar.Value > 0.12f && slicedLiquidBar.Value <= 0.4f && currColor != phase3Color)
        {
            currColor = phase3Color;
            barImage.color = currColor;
            barImage.material.SetColor("_LiquidColor", phase3MatColor);
        }
        else if (slicedLiquidBar.Value <= 0.12f && currColor != moribundColor)
        {
            currColor = moribundColor;
            barImage.color = currColor;
            barImage.material.SetColor("_LiquidColor", moribundMatColor);
        }
    }




}
