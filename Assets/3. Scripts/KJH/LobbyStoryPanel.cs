using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;
public class LobbyStoryPanel : MonoBehaviour
{
    PopupUI PopupUI;
    void Awake()
    {
        GameManager.I.TryGetComponent(out PopupUI);
        slots = new Transform[3];
        slots[0] = transform.Find("Wrap/CharacterSlot0");
        slots[1] = transform.Find("Wrap/CharacterSlot1");
        slots[2] = transform.Find("Wrap/CharacterSlot2");
        leftMonitor = transform.Find("Wrap/LeftMonitor").gameObject;
    }
    void OnEnable()
    {
        RefreshSlots();
        DBManager.I.onLogOut += LoginChangeHandler;
        DBManager.I.onReLogIn += LoginChangeHandler;
        Opening();
        select = -1;
        isDisable = false;
    }
    void OnDisable()
    {
        DBManager.I.onLogOut -= LoginChangeHandler;
        DBManager.I.onReLogIn -= LoginChangeHandler;
        isDisable = true;
    }
    async void LoginChangeHandler()
    {
        if (DBManager.I.IsSteamInit())
        {
            await Task.Delay(2000);
            PopupUI.ClosePop(0, false);
            PopupUI.OpenPop(2);
        }
        else
        {
            PopupUI.ClosePop(2, false);
            PopupUI.OpenPop(0);
        }
        RefreshSlots();
    }
    async void Opening()
    {
        transform.Find("Wrap").gameObject.SetActive(false);
        transform.Find("Opening").gameObject.SetActive(true);
        RectTransform rt = transform.Find("Opening/BG1").GetComponent<RectTransform>();
        DOTween.Kill(rt);
        rt.anchoredPosition = new Vector2(0, -100);
        rt.DOAnchorPos(new Vector2(0, 800), 1.9f).SetEase(Ease.InSine);
        Image image = rt.GetComponent<Image>();
        DOTween.Kill(image);
        image.color = new Color(1f, 1f, 1f, 1f);
        image.DOFade(0f, 2f).SetLink(gameObject);
        DBManager.I.currData = new CharacterData();
        await Task.Delay(1500);
        transform.Find("Wrap").gameObject.SetActive(true);
        CanvasGroup canvasGroup = transform.Find("Wrap").GetComponent<CanvasGroup>();
        DOTween.Kill(canvasGroup);
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 1.9f).SetEase(Ease.InSine).SetLink(gameObject);
        rt = transform.Find("Wrap/BG2").GetComponent<RectTransform>();
        DOTween.Kill(rt);
        rt.anchoredPosition = new Vector2(0, -130);
        rt.DOAnchorPos(new Vector2(0, 40), 1.8f);
        SometimesGlitchTextLoop();
        await Task.Delay(1000);
    }
    Transform[] slots;
    bool isSteamSlot;
    GameObject leftMonitor;
    Color color1 = new Color(1f, 1f, 1f, 0.85f);
    Color color2 = new Color(0.617f, 0.861f, 1f, 1f);
    void RefreshSlots()
    {
        select = -1;
        leftMonitor.SetActive(false);
        slots[0].Find("Slot/SelectButton").gameObject.SetActive(true);
        slots[1].Find("Slot/SelectButton").gameObject.SetActive(true);
        slots[2].Find("Slot/SelectButton").gameObject.SetActive(true);
        if (DBManager.I.IsSteamInit() && DBManager.I.IsSteam())
        {
            isSteamSlot = true;
            if (DBManager.I.allSaveDatasInSteam.characterDatas == null
            || DBManager.I.allSaveDatasInSteam.characterDatas.Count == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    slots[i].Find("Empty").gameObject.SetActive(true);
                    slots[i].Find("Slot").gameObject.SetActive(false);
                    slots[i].Find("Frame").GetComponent<Image>().color = color1;
                }
                return;
            }
            for (int i = 0; i < 3; i++)
            {
                if (i < DBManager.I.allSaveDatasInSteam.characterDatas.Count
                && DBManager.I.allSaveDatasInSteam.characterDatas[i].maxHealth > 0)
                {
                    slots[i].Find("Empty").gameObject.SetActive(false);
                    slots[i].Find("Slot").gameObject.SetActive(true);
                    slots[i].Find("Frame").GetComponent<Image>().color = color1;
                    // 텍스트 내용 갱신
                    Transform wrap = slots[i].Find("Slot");
                    CharacterData data = DBManager.I.allSaveDatasInSteam.characterDatas[i];
                    wrap.Find("NameText(1)").GetComponent<TMP_Text>().text = $"Player{i + 1}";
                    wrap.Find("DeathText(1)").GetComponent<TMP_Text>().text = $"{data.death}";
                    wrap.Find("GearText(1)").GetComponent<TMP_Text>().text = $"{data.gearDatas.Count}";
                    wrap.Find("LastText(1)").GetComponent<TMP_Text>().text = $"{data.lastTime.Split("-")[0]}";
                    wrap.Find("LastText(1)").GetComponent<TMP_Text>().text = $"";
                    wrap.Find("GoldText(1)").GetComponent<TMP_Text>().text = $"{data.gold}";
                }
                else
                {
                    slots[i].Find("Empty").gameObject.SetActive(true);
                    slots[i].Find("Slot").gameObject.SetActive(false);
                    slots[i].Find("Frame").GetComponent<Image>().color = color1;
                }
            }
        }
        else
        {
            isSteamSlot = false;
            if (DBManager.I.allSaveDatasInLocal.characterDatas == null
            || DBManager.I.allSaveDatasInLocal.characterDatas.Count == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    slots[i].Find("Empty").gameObject.SetActive(true);
                    slots[i].Find("Slot").gameObject.SetActive(false);
                    slots[i].Find("Frame").GetComponent<Image>().color = color1;
                }
                return;
            }
            for (int i = 0; i < 3; i++)
            {
                if (i < DBManager.I.allSaveDatasInLocal.characterDatas.Count
                && DBManager.I.allSaveDatasInLocal.characterDatas[i].maxHealth > 0)
                {
                    slots[i].Find("Empty").gameObject.SetActive(false);
                    slots[i].Find("Slot").gameObject.SetActive(true);
                    slots[i].Find("Frame").GetComponent<Image>().color = color1;
                    // 텍스트 내용 갱신
                    Transform wrap = slots[i].Find("Slot");
                    CharacterData data = DBManager.I.allSaveDatasInLocal.characterDatas[i];
                    wrap.Find("NameText(1)").GetComponent<TMP_Text>().text = $"Offline Player{i + 1}";
                    wrap.Find("DeathText(1)").GetComponent<TMP_Text>().text = $"{data.death}";
                    wrap.Find("GearText(1)").GetComponent<TMP_Text>().text = $"{data.gearDatas.Count}";
                    wrap.Find("LastText(1)").GetComponent<TMP_Text>().text = $"{data.lastTime.Split("-")[0]}";
                    wrap.Find("GoldText(1)").GetComponent<TMP_Text>().text = $"{data.gold}";
                }
                else
                {
                    slots[i].Find("Empty").gameObject.SetActive(true);
                    slots[i].Find("Slot").gameObject.SetActive(false);
                    slots[i].Find("Frame").GetComponent<Image>().color = color1;
                }
            }

        }
    }
    [SerializeField] int select = -1;
    public async void NewGameButton(int index)
    {
        AudioManager.I.PlaySFX("SciFiConfirm");
        select = index;
        leftMonitor.SetActive(false);
        slots[0].Find("Slot/SelectButton").gameObject.SetActive(true);
        slots[1].Find("Slot/SelectButton").gameObject.SetActive(true);
        slots[2].Find("Slot/SelectButton").gameObject.SetActive(true);
        ColorRecoverSlot(0);
        ColorRecoverSlot(1);
        ColorRecoverSlot(2);
        ColorChangeSlot(index);
        await Task.Delay(50);
        PopupUI.OpenPop(3, false);
    }
    public void SelectButton(int index)
    {
        if (select == index) return;
        slots[0].Find("Slot/SelectButton").gameObject.SetActive(true);
        slots[1].Find("Slot/SelectButton").gameObject.SetActive(true);
        slots[2].Find("Slot/SelectButton").gameObject.SetActive(true);
        ColorRecoverSlot(0);
        ColorRecoverSlot(1);
        ColorRecoverSlot(2);
        AudioManager.I.PlaySFX("SciFiConfirm");
        select = index;
        slots[index].Find("Slot/SelectButton").gameObject.SetActive(false);
        OpenLeftMonitor();
        ColorChangeSlot(index);
    }
    [SerializeField] Sprite noImage;
    [SerializeField] Sprite stage0;
    void OpenLeftMonitor()
    {
        leftMonitor.SetActive(true);
        RectTransform rtFrame = leftMonitor.transform.Find("Frame").GetComponent<RectTransform>();
        Image imgFrame = rtFrame.GetComponent<Image>();
        DOTween.Kill(rtFrame);
        DOTween.Kill(rtFrame.transform);
        DOTween.Kill(imgFrame);
        rtFrame.transform.localScale = 0.5f * Vector3.one;
        rtFrame.transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad).SetLink(gameObject);
        Vector2 size = rtFrame.sizeDelta;
        Vector2 size2 = new Vector2(size.x, 100f);
        rtFrame.sizeDelta = size2;
        rtFrame.DOSizeDelta(size, 0.55f).SetEase(Ease.InBack).SetLink(gameObject);
        imgFrame.color = new Color(imgFrame.color.r, imgFrame.color.g, imgFrame.color.b, 0f);
        imgFrame.DOFade(1f, 4f).SetLink(gameObject);
        CanvasGroup canvasGroup = leftMonitor.transform.Find("Wrap").GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        DOTween.Kill(canvasGroup);
        canvasGroup.DOFade(1f, 3.5f).SetEase(Ease.InSine).SetLink(gameObject);
        // 텍스트 내용 갱신
        Transform wrap = leftMonitor.transform.Find("Wrap");
        CharacterData data;
        if (isSteamSlot)
            data = DBManager.I.allSaveDatasInSteam.characterDatas[select];
        else
            data = DBManager.I.allSaveDatasInLocal.characterDatas[select];
        // 썸네일  
        Image thumbnail = wrap.Find("ThumbnailFrame/Thumbnail").GetComponent<Image>();
        int _slotIndex = (isSteamSlot) ? select : select + 3;
#if UNITY_STANDALONE_WIN
        string fileLocation = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "My Games", "REKINDLE");
#else
        string fileLocation = Path.Combine(Application.persistentDataPath, "REKINDLE_SaveData");
#endif
        fileLocation = Path.Combine(fileLocation, $"{_slotIndex}.png");
        if (File.Exists(fileLocation))
        {
            byte[] fileData = File.ReadAllBytes(fileLocation);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                // 2. Texture2D를 기반으로 Sprite 생성
                Rect rect = new Rect(0, 0, tex.width, tex.height);
                thumbnail.sprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
            }
            else
                thumbnail.sprite = noImage;
        }
        else
        {
            if (data.sceneName == "Stage0" || data.sceneName == "Cinematic")
                thumbnail.sprite = stage0;
            else
                thumbnail.sprite = noImage;
        }
        
        wrap.Find("LocationText(1)").GetComponent<TMP_Text>().text = $"{data.sceneName}";
        string diffText = "";
        switch (data.difficulty)
        {
            case 0:
                switch (SettingManager.I.setting.locale)
                {
                    case 0:
                        diffText = "Easy";
                        break;
                    case 1:
                        diffText = "쉬움";
                        break;
                }
                break;
            case 1:
                switch (SettingManager.I.setting.locale)
                {
                    case 0:
                        diffText = "Normal";
                        break;
                    case 1:
                        diffText = "보통";
                        break;
                }
                break;
            case 2:
                switch (SettingManager.I.setting.locale)
                {
                    case 0:
                        diffText = "Hard";
                        break;
                    case 1:
                        diffText = "어려움";
                        break;
                }
                break;
        }
        wrap.Find("DifficultyText(1)").GetComponent<TMP_Text>().text = $"{diffText}";

    }
    void ColorChangeSlot(int index)
    {
        DOTween.Kill(slots[index].transform);
        slots[index].transform.localScale = 1.5f * Vector3.one;
        slots[index].transform.DOScale(1f, 0.8f).SetEase(Ease.OutBack).SetLink(gameObject);
        Image imgFrame = slots[index].Find("Frame").GetComponent<Image>();
        DOTween.Kill(imgFrame);
        imgFrame.color = color1;
        imgFrame.DOColor(color2, 0.2f).SetLink(gameObject);
    }
    void ColorRecoverSlot(int index)
    {
        DOTween.Kill(slots[index].transform);
        slots[index].transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad).SetLink(gameObject);
        Image imgFrame = slots[index].Find("Frame").GetComponent<Image>();
        DOTween.Kill(imgFrame);
        imgFrame.DOColor(color1, 0.2f).SetLink(gameObject);
    }
    /////////////////
    Button[] buttons;
    void DisableAllButtons()
    {
        buttons = transform.root.GetComponentsInChildren<Button>();
        foreach (var btn in buttons)
            btn.enabled = false;
    }
    public async void StartButton()
    {
        DBManager.I.CloseLoginUI();
        AudioManager.I.PlaySFX("SciFiConfirm");
        DisableAllButtons();
        await Task.Delay(200);
        if (isSteamSlot)
        {
            DBManager.I.currData = DBManager.I.allSaveDatasInSteam.characterDatas[select];
            DBManager.I.currSlot = select;
        }
        else
        {
            DBManager.I.currData = DBManager.I.allSaveDatasInLocal.characterDatas[select];
            DBManager.I.currSlot = select + 3;
        }
        await Task.Delay(200);
        GameManager.I.SetSceneFromDB();
        GameManager.I.LoadSceneAsync(DBManager.I.currData.sceneName, true);
    }
    [ReadOnlyInspector] public int diff;
    public async void StartNewGameButton()
    {
        DBManager.I.CloseLoginUI();
        AudioManager.I.PlaySFX("SciFiConfirm");
        DisableAllButtons();
        PopupUI.ClosePop(3, false);
        await Task.Delay(200);
        CharacterData newData = new CharacterData();
        newData.gold = 0;
        newData.death = 0;
        newData.difficulty = diff;
        Debug.Log(diff);
        newData.sceneName = "Cinematic";
        newData.lastPos = Vector2.zero;
        newData.maxHealth = 400;
        newData.maxBattery = 100;
        newData.currHealth = 400;
        newData.currBattery = 100;
        newData.maxPotionCount = 3;
        newData.currPotionCount = 3;
        newData.maxGearCost = 3;

        System.DateTime now = System.DateTime.Now;
        string datePart = now.ToString("yyyy.MM.dd");
        int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
        newData.lastTime = $"{datePart}-{secondsOfDay}";

        newData.itemDatas = new List<CharacterData.ItemData>();
        newData.gearDatas = new List<CharacterData.GearData>();
        newData.lanternDatas = new List<CharacterData.LanternData>();
        newData.recordDatas = new List<CharacterData.RecordData>();
        newData.sceneDatas = new List<CharacterData.SceneData>();
        newData.progressDatas = new List<CharacterData.ProgressData>();
        newData.killCounts = new List<CharacterData.KillCount>();

        DBManager.I.currData = newData;
        // 신규캐릭터 시작 아이템
        DBManager.I.AddLantern("BasicLantern");
        int find = DBManager.I.currData.lanternDatas.FindIndex(x => x.Name == "BasicLantern");
        CharacterData.LanternData lanternData = DBManager.I.currData.lanternDatas[find];
        lanternData.isEquipped = true;
        lanternData.isNew = false;
        DBManager.I.currData.lanternDatas[find] = lanternData;


        if (isSteamSlot)
        {
            DBManager.I.currSlot = select;

            if (DBManager.I.allSaveDatasInSteam.characterDatas == null)
            {
                DBManager.I.allSaveDatasInSteam.characterDatas = new List<CharacterData>();
            }

            int addEmptyCount = select - DBManager.I.allSaveDatasInSteam.characterDatas.Count;
            if (addEmptyCount > 0)
            {
                CharacterData emptyData = new CharacterData();
                emptyData.maxHealth = 0;
                emptyData.itemDatas = new List<CharacterData.ItemData>();
                emptyData.gearDatas = new List<CharacterData.GearData>();
                emptyData.lanternDatas = new List<CharacterData.LanternData>();
                emptyData.recordDatas = new List<CharacterData.RecordData>();
                for (int i = 0; i < addEmptyCount; i++)
                    DBManager.I.allSaveDatasInSteam.characterDatas.Add(emptyData);
            }
            DBManager.I.allSaveDatasInSteam.characterDatas.Add(newData);
        }
        else
        {
            DBManager.I.currSlot = select + 3;

            if (DBManager.I.allSaveDatasInLocal.characterDatas == null)
            {
                DBManager.I.allSaveDatasInLocal.characterDatas = new List<CharacterData>();
            }

            int addEmptyCount = select - DBManager.I.allSaveDatasInLocal.characterDatas.Count;
            if (addEmptyCount > 0)
            {
                CharacterData emptyData = new CharacterData();
                emptyData.maxHealth = 0;
                emptyData.itemDatas = new List<CharacterData.ItemData>();
                emptyData.gearDatas = new List<CharacterData.GearData>();
                emptyData.lanternDatas = new List<CharacterData.LanternData>();
                emptyData.recordDatas = new List<CharacterData.RecordData>();
                for (int i = 0; i < addEmptyCount; i++)
                    DBManager.I.allSaveDatasInLocal.characterDatas.Add(emptyData);
            }



            DBManager.I.allSaveDatasInLocal.characterDatas.Add(newData);
        }
        DBManager.I.Save();
        await Task.Delay(200);
        GameManager.I.LoadSceneAsync(DBManager.I.currData.sceneName, false);
    }
    public async void DeleteButton()
    {
        AudioManager.I.PlaySFX("UIClick");
        PopupUI.OpenPop(4);
    }
    public async void Pop4DeleteButton()
    {
        AudioManager.I.PlaySFX("SciFiConfirm");
        PopupUI.ClosePop(4, false);
        if (isSteamSlot)
        {
            SaveData copy = new SaveData();
            copy.characterDatas = DBManager.I.allSaveDatasInSteam.characterDatas.ToList();
            SaveData newSaveData = new SaveData();
            newSaveData.characterDatas = new List<CharacterData>();
            for (int i = 0; i < copy.characterDatas.Count; i++)
            {
                newSaveData.characterDatas.Add(copy.characterDatas[i]);
            }
            await Task.Delay(50);
            newSaveData.characterDatas[select] = new CharacterData();
            await Task.Delay(50);
            DBManager.I.allSaveDatasInSteam = new SaveData();
            DBManager.I.allSaveDatasInSteam.characterDatas = newSaveData.characterDatas.ToList();
            await Task.Delay(50);
            for (int i = 0; i < DBManager.I.allSaveDatasInSteam.characterDatas.Count; i++)
            {
                Debug.Log(DBManager.I.allSaveDatasInSteam.characterDatas[i].maxHealth);
            }
            await Task.Delay(50);
            DBManager.I.SaveSteam();
        }
        else
        {
            SaveData copy = new SaveData();
            copy.characterDatas = DBManager.I.allSaveDatasInLocal.characterDatas.ToList();
            SaveData newSaveData = new SaveData();
            newSaveData.characterDatas = new List<CharacterData>();
            for (int i = 0; i < copy.characterDatas.Count; i++)
            {
                newSaveData.characterDatas.Add(copy.characterDatas[i]);
            }
            await Task.Delay(50);
            newSaveData.characterDatas[select] = new CharacterData();
            await Task.Delay(50);
            DBManager.I.allSaveDatasInLocal = new SaveData();
            DBManager.I.allSaveDatasInLocal.characterDatas = newSaveData.characterDatas.ToList();
            await Task.Delay(50);
            for (int i = 0; i < DBManager.I.allSaveDatasInLocal.characterDatas.Count; i++)
            {
                Debug.Log(DBManager.I.allSaveDatasInLocal.characterDatas[i].maxHealth);
            }
            await Task.Delay(50);
            DBManager.I.SaveLocal();
        }
        await Task.Delay(50);
        RefreshSlots();
    }
    TMP_Text[] texts1;
    Text[] texts2;
    bool isDisable;
    async void SometimesGlitchTextLoop()
    {
        await Task.Delay(1700);
        texts1 = transform.GetComponentsInChildren<TMP_Text>();
        texts2 = transform.GetComponentsInChildren<Text>();
        while (true)
        {
            await Task.Delay(50);
            if (Random.value < 0.3f)
            {
                int rnd = Random.Range(0, texts1.Length + texts2.Length);
                if (rnd >= texts1.Length)
                {
                    if (texts2.Length <= rnd - texts1.Length) continue;
                    Text text2 = texts2[rnd - texts1.Length];
                    if (text2 == null) continue;
                    if (!text2.gameObject.activeInHierarchy) continue;
                    if (text2.transform.name == "EmptyText") continue;
                    GameManager.I.GlitchText(text2, 0.16f);
                    if (Random.value < 0.73f)
                        AudioManager.I.PlaySFX("Glitch1");
                }
                else
                {
                    if (texts1.Length <= rnd) continue;
                    TMP_Text text1 = texts1[rnd];
                    if (text1 == null) continue;
                    if (!text1.gameObject.activeInHierarchy) continue;
                    if (text1.transform.name == "EmptyText") continue;
                    GameManager.I.GlitchText(text1, 0.16f);
                    if (Random.value < 0.73f)
                        AudioManager.I.PlaySFX("Glitch1");
                }
            }
            await Task.Delay(Random.Range(200, 800));
            if (isDisable) return;
        }
    }







}
