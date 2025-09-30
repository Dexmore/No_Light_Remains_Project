#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class CreateHudLayout
{
    [MenuItem("GameObject/Create HUD (SideScroller)/Minimal HUD (v2)", priority = 0)]
    public static void CreateMinimalHudV2()
    {
        // Canvas 
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // EventSystem 
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }

        // Root 
        var hudPanel = CreateUI("HUD_Panel", canvasGO.transform, out RectTransform hudRT);
        hudRT.anchorMin = Vector2.zero; hudRT.anchorMax = Vector2.one;
        hudRT.offsetMin = Vector2.zero; hudRT.offsetMax = Vector2.zero;

        var topBar = CreateUI("TopBar", hudPanel.transform, out RectTransform topRT);
        topRT.anchorMin = new Vector2(0, 1);
        topRT.anchorMax = new Vector2(1, 1);
        topRT.pivot = new Vector2(0.5f, 1);
        topRT.sizeDelta = new Vector2(0, 140);
        topRT.anchoredPosition = Vector2.zero;

        // 레이아웃: [아이콘] [포션+게이지] [기어(동그라미 3개)]
        // Left: 등대 아이콘
        var leftCol = CreateUI("LeftColumn", topBar.transform, out RectTransform leftRT);
        leftRT.anchorMin = new Vector2(0, 1);
        leftRT.anchorMax = new Vector2(0, 1);
        leftRT.pivot = new Vector2(0, 1);
        leftRT.sizeDelta = new Vector2(96, 96);
        leftRT.anchoredPosition = new Vector2(24, -22);

        var lighthouseSlot = CreateImage("Lighthouse_IconSlot", leftCol.transform, out RectTransform lrt);
        lrt.sizeDelta = new Vector2(64, 64);
        lrt.anchorMin = new Vector2(0, 1);
        lrt.anchorMax = new Vector2(0, 1);
        lrt.pivot = new Vector2(0, 1);
        lrt.anchoredPosition = Vector2.zero;
        lighthouseSlot.color = new Color(1,1,1,0.25f); // 임시 표시

        // Center: 포션 4칸(작은 네모) + 게이지 스택
        var centerCol = CreateUI("CenterColumn", topBar.transform, out RectTransform centerRT);
        centerRT.anchorMin = new Vector2(0, 1);
        centerRT.anchorMax = new Vector2(0, 1);
        centerRT.pivot = new Vector2(0, 1);
        centerRT.anchoredPosition = new Vector2(120, -20);
        centerRT.sizeDelta = new Vector2(950, 100);

        // 포션 4칸 (작은 사각형)
        var potionRow = CreateUI("Potion_Row", centerCol.transform, out RectTransform potionRT);
        potionRT.anchorMin = new Vector2(0, 1);
        potionRT.anchorMax = new Vector2(0, 1);
        potionRT.pivot = new Vector2(0, 1);
        potionRT.sizeDelta = new Vector2(240, 28);
        potionRT.anchoredPosition = Vector2.zero;
        var potionGrid = potionRow.gameObject.AddComponent<GridLayoutGroup>();
        potionGrid.cellSize = new Vector2(26, 26);
        potionGrid.spacing = new Vector2(6, 0);
        potionGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        potionGrid.constraintCount = 4;
        for (int i = 0; i < 4; i++)
        {
            var slot = CreateImage($"Potion_{i+1}", potionRow.transform, out _);
            slot.color = new Color(1,1,1,0.25f); // 임시
        }

        // 게이지 스택 (HP 위, 등대 아래)
        var gauges = CreateUI("Gauges", centerCol.transform, out RectTransform gaugesRT);
        gaugesRT.anchorMin = new Vector2(0, 1);
        gaugesRT.anchorMax = new Vector2(0, 1);
        gaugesRT.pivot = new Vector2(0, 1);
        gaugesRT.sizeDelta = new Vector2(740, 58);
        gaugesRT.anchoredPosition = new Vector2(0, -36);

        var hp = CreateUI("HP_Bar", gauges.transform, out RectTransform hpRT);
        hpRT.anchorMin = new Vector2(0, 1);
        hpRT.anchorMax = new Vector2(0, 1);
        hpRT.pivot = new Vector2(0, 1);
        hpRT.sizeDelta = new Vector2(740, 24);
        hpRT.anchoredPosition = Vector2.zero;
        var hpBG = CreateImage("BG", hp.transform, out RectTransform _hpBg); // 틀 스프라이트 지정
        _hpBg.anchorMin = Vector2.zero; _hpBg.anchorMax = Vector2.one; _hpBg.offsetMin = Vector2.zero; _hpBg.offsetMax = Vector2.zero;
        var hpFill = CreateImage("Fill", hp.transform, out RectTransform _hpFill);
        var hpFillImg = hpFill.GetComponent<Image>();
        hpFillImg.type = Image.Type.Filled;
        hpFillImg.fillMethod = Image.FillMethod.Horizontal;
        hpFillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        hpFillImg.fillAmount = 1f;

        var light = CreateUI("Lighthouse_Gauge", gauges.transform, out RectTransform lgRT);
        lgRT.anchorMin = new Vector2(0, 1);
        lgRT.anchorMax = new Vector2(0, 1);
        lgRT.pivot = new Vector2(0, 1);
        lgRT.sizeDelta = new Vector2(520, 18);
        lgRT.anchoredPosition = new Vector2(0, -30);
        var lgBG = CreateImage("BG", light.transform, out RectTransform _lgBg);
        _lgBg.anchorMin = Vector2.zero; _lgBg.anchorMax = Vector2.one; _lgBg.offsetMin = Vector2.zero; _lgBg.offsetMax = Vector2.zero;
        var lgFill = CreateImage("Fill", light.transform, out RectTransform _lgFill);
        var lgFillImg = lgFill.GetComponent<Image>();
        lgFillImg.type = Image.Type.Filled;
        lgFillImg.fillMethod = Image.FillMethod.Horizontal;
        lgFillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        lgFillImg.fillAmount = 0.5f;

        // Right: 착용 기어 (동그라미 3칸)
        var gearRow = CreateUI("Gear_Row", topBar.transform, out RectTransform gearRT);
        gearRT.anchorMin = new Vector2(1, 1);
        gearRT.anchorMax = new Vector2(1, 1);
        gearRT.pivot = new Vector2(1, 1);
        gearRT.anchoredPosition = new Vector2(-24, -34);
        gearRT.sizeDelta = new Vector2(220, 56);
        var gearHLG = gearRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        gearHLG.spacing = 10; gearHLG.childAlignment = TextAnchor.MiddleRight;
        for (int i = 0; i < 3; i++)
        {
            var circle = CreateImage($"Gear_{i+1}_Circle", gearRow.transform, out RectTransform crt);
            crt.sizeDelta = new Vector2(48, 48);
            circle.color = new Color(1,1,1,0.25f); // 임시(동그라미 스프라이트 지정 예정)
            // 원형 아이콘을 마스킹하고 싶다면 아래 주석 해제 후, 자식에 Icon 추가해서 사용:
            // circle.type = Image.Type.Simple;
            // circle.preserveAspect = true;
            // circle.gameObject.AddComponent<Mask>(); // 원형 스프라이트를 Mask로 사용
        }

        // Currency (아이콘 + 숫자) - 좌측 하단 줄
        var currency = CreateUI("Currency_Display", topBar.transform, out RectTransform curRT);
        curRT.anchorMin = new Vector2(0, 1);
        curRT.anchorMax = new Vector2(0, 1);
        curRT.pivot = new Vector2(0, 1);
        curRT.anchoredPosition = new Vector2(24, -110);
        curRT.sizeDelta = new Vector2(240, 26);

        var coinIcon = CreateImage("Icon", currency.transform, out RectTransform coinRT);
        coinRT.sizeDelta = new Vector2(20, 20);
        coinRT.anchorMin = new Vector2(0, 0.5f); coinRT.anchorMax = new Vector2(0, 0.5f);
        coinRT.pivot = new Vector2(0, 0.5f);

        var coinTextGO = new GameObject("Text", typeof(TextMeshProUGUI));
        coinTextGO.transform.SetParent(currency.transform, false);
        var coinTMP = coinTextGO.GetComponent<TextMeshProUGUI>();
        coinTMP.text = "0";
        coinTMP.fontSize = 24;
        var coinTextRT = coinTextGO.GetComponent<RectTransform>();
        coinTextRT.anchorMin = new Vector2(0, 0); coinTextRT.anchorMax = new Vector2(1, 1);
        coinTextRT.offsetMin = new Vector2(28, 0); coinTextRT.offsetMax = Vector2.zero;

        // Runtime hooks
        var hooks = CreateUI("HUD_RuntimeHooks", hudPanel.transform, out _);
        hooks.AddComponent<HealthBar>();
        hooks.AddComponent<LighthouseBar>();
        hooks.AddComponent<CurrencyUI>();

        Selection.activeObject = canvasGO;
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Minimal HUD v2");
        Debug.Log("✅ Minimal HUD v2 created. 스프라이트를 각 Image에 할당하세요.");
    }

    // Helpers 
    private static GameObject CreateUI(string name, Transform parent, out RectTransform rt)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        rt = go.GetComponent<RectTransform>();
        return go;
    }

    private static Image CreateImage(string name, Transform parent, out RectTransform rt)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        rt = go.GetComponent<RectTransform>();
        return go.GetComponent<Image>();
    }
}
#endif
