using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LobbyStoryPanel : MonoBehaviour
{
    PopupControl popupControl;
    void Awake()
    {
        GameManager.I.TryGetComponent(out popupControl);
        slots = new Transform[3];
        slots[0] = transform.Find("Wrap/CharacterSlot0");
        slots[1] = transform.Find("Wrap/CharacterSlot1");
        slots[2] = transform.Find("Wrap/CharacterSlot2");
        leftMonitor = transform.Find("Wrap/LeftMonitor").gameObject;
    }
    void OnEnable()
    {
        RefreshSlots();
        DBManager.I.onLogOut += HandlerChangeLogin;
        DBManager.I.onReLogIn += HandlerChangeLogin;
    }
    void OnDisable()
    {
        DBManager.I.onLogOut -= HandlerChangeLogin;
        DBManager.I.onReLogIn -= HandlerChangeLogin;
    }
    async void HandlerChangeLogin()
    {
        if (DBManager.I.IsSteamInit())
        {
            await Task.Delay(2200);
            popupControl.ClosePop(0, false);
            popupControl.OpenPop(2);
        }
        else
        {
            popupControl.ClosePop(2, false);
            popupControl.OpenPop(0);
        }
        RefreshSlots();
    }
    Transform[] slots;
    bool isSteamSlot;
    GameObject leftMonitor;
    Color color1 = new Color(0.9f, 0.9f, 0.9f, 0.9f);
    Color color2 = new Color(0.617f, 0.861f, 1f, 1f);
    void RefreshSlots()
    {
        leftMonitor.SetActive(false);
        if (DBManager.I.IsSteamInit() && DBManager.I.IsSteam())
        {
            isSteamSlot = true;
            for (int i = 0; i < 3; i++)
            {
                if (i < DBManager.I.allSaveDatasInSteam.characterDatas.Count)
                {
                    slots[i].Find("Empty").gameObject.SetActive(false);
                    slots[i].Find("Slot").gameObject.SetActive(true);
                    slots[i].Find("Frame").GetComponent<Image>().color = color1;

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
            for (int i = 0; i < 3; i++)
            {
                if (i < DBManager.I.allSaveDatasInLocal.characterDatas.Count)
                {
                    slots[i].Find("Empty").gameObject.SetActive(false);
                    slots[i].Find("Slot").gameObject.SetActive(true);
                    slots[i].Find("Frame").GetComponent<Image>().color = color1;

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
    int select = -1;
    public async void NewGameButton(int index)
    {
        AudioManager.I.PlaySFX("SciFiConfirm");
        select = -1;
        leftMonitor.SetActive(false);
        slots[0].Find("Slot/SelectButton").gameObject.SetActive(true);
        slots[1].Find("Slot/SelectButton").gameObject.SetActive(true);
        slots[2].Find("Slot/SelectButton").gameObject.SetActive(true);
        ColorRecoverSlot(0);
        ColorRecoverSlot(1);
        ColorRecoverSlot(2);
        ColorChangeSlot(index);
        await Task.Delay(80);
        popupControl.OpenPop(3, false);
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
    void OpenLeftMonitor()
    {
        leftMonitor.SetActive(true);
        RectTransform rtFrame = leftMonitor.transform.Find("Frame").GetComponent<RectTransform>();
        Image imgFrame = rtFrame.GetComponent<Image>();
        DOTween.Kill(rtFrame);
        DOTween.Kill(rtFrame.transform);
        DOTween.Kill(imgFrame);
        rtFrame.transform.localScale = 0.5f * Vector3.one;
        rtFrame.transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        Vector2 size = rtFrame.sizeDelta;
        Vector2 size2 = new Vector2(size.x, 100f);
        rtFrame.sizeDelta = size2;
        rtFrame.DOSizeDelta(size, 0.55f).SetEase(Ease.InBack);
        imgFrame.color = new Color(imgFrame.color.r, imgFrame.color.g, imgFrame.color.b, 0f);
        imgFrame.DOFade(1f, 4f);
    }
    void ColorChangeSlot(int index)
    {
        DOTween.Kill(slots[index].transform);
        slots[index].transform.localScale = 1.5f * Vector3.one;
        slots[index].transform.DOScale(1f, 0.8f).SetEase(Ease.OutBack);
        Image imgFrame = slots[index].Find("Frame").GetComponent<Image>();
        DOTween.Kill(imgFrame);
        imgFrame.color = color1;
        imgFrame.DOColor(color2, 0.5f).SetEase(Ease.OutQuad);
    }
    void ColorRecoverSlot(int index)
    {
        DOTween.Kill(slots[index].transform);
        slots[index].transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        Image imgFrame = slots[index].Find("Frame").GetComponent<Image>();
        DOTween.Kill(imgFrame);
        imgFrame.DOColor(color1, 0.3f).SetEase(Ease.OutQuad);
    }









    // GameObject storyPanel;
    // 
    // void Awake()
    // {
    //     storyPanel = gameObject;
    // }

    // void OnEnable()
    // {
    //     RefreshSlots();
    // }
    // bool isSteamSlot;
    // public void RefreshSlots()
    // {
    //     Transform saveSlots = storyPanel.transform.Find("SaveSlots");
    //     if (DBManager.I.IsSteamInit())
    //     {
    //         for (int i = 0; i < 3; i++)
    //         {
    //             if (i < DBManager.I.allSaveDatasInSteam.characterDatas.Count)
    //             {
    //                 saveSlots.GetChild(i).Find("NoData").gameObject.SetActive(false);
    //                 Transform dataPanel = saveSlots.GetChild(i).Find("Data_Panel");
    //                 dataPanel.gameObject.SetActive(true);
    //                 TMP_Text[] tMP_Texts = dataPanel.GetComponentsInChildren<TMP_Text>();
    //                 CharacterData characterData = DBManager.I.allSaveDatasInSteam.characterDatas[i];
    //                 tMP_Texts[0].text = $"위치 : {characterData.sceneName}";
    //                 tMP_Texts[1].text = $"재화 : {characterData.gold} 원";
    //                 tMP_Texts[2].text = $"기어 : {characterData.gearDatas.Count} 개";
    //             }
    //             else
    //             {
    //                 saveSlots.GetChild(i).Find("Data_Panel").gameObject.SetActive(false);
    //                 saveSlots.GetChild(i).Find("NoData").gameObject.SetActive(true);
    //             }
    //         }
    //         isSteamSlot = true;
    //     }
    //     else
    //     {
    //         DBManager.I.currSlot = 3;
    //         for (int i = 0; i < 3; i++)
    //         {
    //             if (i < DBManager.I.allSaveDatasInLocal.characterDatas.Count)
    //             {
    //                 saveSlots.GetChild(i).Find("NoData").gameObject.SetActive(false);
    //                 Transform dataPanel = saveSlots.GetChild(i).Find("Data_Panel");
    //                 dataPanel.gameObject.SetActive(true);
    //                 TMP_Text[] tMP_Texts = dataPanel.GetComponentsInChildren<TMP_Text>();
    //                 CharacterData characterData = DBManager.I.allSaveDatasInLocal.characterDatas[i];
    //                 tMP_Texts[0].text = $"위치 : {characterData.sceneName}";
    //                 tMP_Texts[1].text = $"재화 : {characterData.gold} 원";
    //                 tMP_Texts[2].text = $"기어 : {characterData.gearDatas.Count} 개";
    //             }
    //             else
    //             {
    //                 saveSlots.GetChild(i).Find("Data_Panel").gameObject.SetActive(false);
    //                 saveSlots.GetChild(i).Find("NoData").gameObject.SetActive(true);
    //             }
    //         }
    //         isSteamSlot = false;
    //     }
    // }
    // int select = -1;
    // public void StoryModeButton(int index)
    // {
    //     int addIndex = 0;
    //     if (!isSteamSlot) addIndex = 3;
    //     select = index + addIndex;
    //     Transform saveSlots = storyPanel.transform.Find("SaveSlots");
    //     if (saveSlots.GetChild(index).Find("NoData").gameObject.activeSelf)
    //     {
    //         StartGame_StoryModeNoData();
    //         return;
    //     }
    //     GameObject button = storyPanel.transform.Find("StartButton").gameObject;
    //     button.SetActive(true);
    //     AudioManager.I.PlaySFX("UIClick");
    // }
    // public async void StartGameButton()
    // {
    //     AudioManager.I.PlaySFX("UIClick");
    //     DisableAllButtons();
    //     await Task.Delay(700);
    //     if (isSteamSlot)
    //     {
    //         DBManager.I.currData = DBManager.I.allSaveDatasInSteam.characterDatas[select];
    //         DBManager.I.currSlot = select;
    //     }
    //     else
    //     {
    //         DBManager.I.currData = DBManager.I.allSaveDatasInLocal.characterDatas[select - 3];
    //         DBManager.I.currSlot = select;
    //     }
    //     await Task.Delay(100);
    //     GameManager.I.LoadSceneAsync(DBManager.I.currData.sceneName, true);
    // }
    // public async void RemoveCharacterButton()
    // {
    //     AudioManager.I.PlaySFX("UIClick");

    // }

    // async void StartGame_StoryModeNoData()
    // {
    //     AudioManager.I.PlaySFX("UIClick");
    //     DisableAllButtons();
    //     GameObject button = storyPanel.transform.Find("StartButton").gameObject;
    //     button.SetActive(false);
    //     await Task.Delay(1500);
    //     CharacterData newData = new CharacterData();
    //     newData.gold = 0;
    //     newData.sceneName = "Stage1";
    //     newData.lastPos = Vector2.zero;
    //     newData.maxHealth = 400;
    //     newData.maxBattery = 100;
    //     newData.currHealth = 400;
    //     newData.currBattery = 100;
    //     newData.potionCount = 5;
    //     newData.maxGearCost = 5;
    //     newData.itemDatas = new List<CharacterData.ItemData>();
    //     newData.gearDatas = new List<CharacterData.GearData>();
    //     newData.lanternDatas = new List<CharacterData.LanternData>();
    //     newData.recordDatas = new List<CharacterData.RecordData>();
    //     DBManager.I.currData = newData;
    //     DBManager.I.currSlot = select;
    //     // 신규캐릭터 시작 아이템
    //     DBManager.I.AddLantern("BasicLantern");
    //     DBManager.I.AddItem("UsefulSword");
    //     DBManager.I.AddItem("Helmet");
    //     DBManager.I.AddItem("LeatherArmor");

    //     if (isSteamSlot)
    //     {
    //         DBManager.I.allSaveDatasInSteam.characterDatas.Add(newData);
    //     }
    //     else
    //     {
    //         DBManager.I.allSaveDatasInLocal.characterDatas.Add(newData);
    //     }
    //     await Task.Delay(5);
    //     DBManager.I.Save();
    //     await Task.Delay(700);
    //     GameManager.I.LoadSceneAsync("Stage1", true);
    // }
    // Button[] buttons;
    // void DisableAllButtons()
    // {
    //     buttons = transform.root.GetComponentsInChildren<Button>();
    //     foreach (var btn in buttons)
    //         btn.enabled = false;
    // }



}
