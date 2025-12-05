using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class LobbyStoryPanel : MonoBehaviour
{
    GameObject storyPanel;
    PopupControl popupControl;
    void Awake()
    {
        storyPanel = gameObject;
    }

    void OnEnable()
    {
        RefreshSlots();
    }
    bool isSteamSlot;
    public void RefreshSlots()
    {
        Transform saveSlots = storyPanel.transform.Find("SaveSlots");
        if (DBManager.I.IsSteamInit())
        {
            for (int i = 0; i < 3; i++)
            {
                if (i < DBManager.I.allSaveDatasInSteam.characterDatas.Count)
                {
                    saveSlots.GetChild(i).Find("NoData").gameObject.SetActive(false);
                    Transform dataPanel = saveSlots.GetChild(i).Find("Data_Panel");
                    dataPanel.gameObject.SetActive(true);
                    TMP_Text[] tMP_Texts = dataPanel.GetComponentsInChildren<TMP_Text>();
                    CharacterData characterData = DBManager.I.allSaveDatasInSteam.characterDatas[i];
                    tMP_Texts[0].text = $"위치 : {characterData.sceneName}";
                    tMP_Texts[1].text = $"재화 : {characterData.gold} 원";
                    tMP_Texts[2].text = $"기어 : {characterData.gearDatas.Count} 개";
                }
                else
                {
                    saveSlots.GetChild(i).Find("Data_Panel").gameObject.SetActive(false);
                    saveSlots.GetChild(i).Find("NoData").gameObject.SetActive(true);
                }
            }
            isSteamSlot = true;
        }
        else
        {
            DBManager.I.currSlot = 3;
            for (int i = 0; i < 3; i++)
            {
                if (i < DBManager.I.allSaveDatasInLocal.characterDatas.Count)
                {
                    saveSlots.GetChild(i).Find("NoData").gameObject.SetActive(false);
                    Transform dataPanel = saveSlots.GetChild(i).Find("Data_Panel");
                    dataPanel.gameObject.SetActive(true);
                    TMP_Text[] tMP_Texts = dataPanel.GetComponentsInChildren<TMP_Text>();
                    CharacterData characterData = DBManager.I.allSaveDatasInLocal.characterDatas[i];
                    tMP_Texts[0].text = $"위치 : {characterData.sceneName}";
                    tMP_Texts[1].text = $"재화 : {characterData.gold} 원";
                    tMP_Texts[2].text = $"기어 : {characterData.gearDatas.Count} 개";
                }
                else
                {
                    saveSlots.GetChild(i).Find("Data_Panel").gameObject.SetActive(false);
                    saveSlots.GetChild(i).Find("NoData").gameObject.SetActive(true);
                }
            }
            isSteamSlot = false;
        }
    }
    int select = -1;
    public void StoryModeButton(int index)
    {
        int addIndex = 0;
        if (!isSteamSlot) addIndex = 3;
        select = index + addIndex;
        Transform saveSlots = storyPanel.transform.Find("SaveSlots");
        if (saveSlots.GetChild(index).Find("NoData").gameObject.activeSelf)
        {
            StartGame_StoryModeNoData();
            return;
        }
        GameObject button = storyPanel.transform.Find("StartButton").gameObject;
        button.SetActive(true);
        AudioManager.I.PlaySFX("UIClick");
    }
    public async void StartGameButton()
    {
        AudioManager.I.PlaySFX("UIClick");
        DisableAllButtons();
        await Task.Delay(700);
        if (isSteamSlot)
        {
            DBManager.I.currData = DBManager.I.allSaveDatasInSteam.characterDatas[select];
            DBManager.I.currSlot = select;
        }
        else
        {
            DBManager.I.currData = DBManager.I.allSaveDatasInLocal.characterDatas[select - 3];
            DBManager.I.currSlot = select;
        }
        await Task.Delay(100);
        GameManager.I.LoadSceneAsync(DBManager.I.currData.sceneName, true);
    }
    public async void RemoveCharacterButton()
    {
        AudioManager.I.PlaySFX("UIClick");

    }

    async void StartGame_StoryModeNoData()
    {
        AudioManager.I.PlaySFX("UIClick");
        DisableAllButtons();
        GameObject button = storyPanel.transform.Find("StartButton").gameObject;
        button.SetActive(false);
        await Task.Delay(1500);
        CharacterData newData = new CharacterData();
        newData.gold = 0;
        newData.sceneName = "Stage1";
        newData.lastPos = Vector2.zero;
        newData.maxHealth = 400;
        newData.maxBattery = 100;
        newData.currHealth = 400;
        newData.currBattery = 100;
        newData.potionCount = 5;
        newData.maxGearCost = 5;
        newData.itemDatas = new List<CharacterData.ItemData>();
        newData.gearDatas = new List<CharacterData.GearData>();
        newData.lanternDatas = new List<CharacterData.LanternData>();
        DBManager.I.currData = newData;
        DBManager.I.currSlot = select;
        // 신규캐릭터 시작 아이템
        DBManager.I.AddLantern("BasicLantern");
        DBManager.I.AddItem("UsefulSword");
        DBManager.I.AddItem("Helmet");
        DBManager.I.AddItem("LeatherArmor");

        if (isSteamSlot)
        {
            DBManager.I.allSaveDatasInSteam.characterDatas.Add(newData);
        }
        else
        {
            DBManager.I.allSaveDatasInLocal.characterDatas.Add(newData);
        }
        await Task.Delay(5);
        DBManager.I.Save();
        await Task.Delay(700);
        GameManager.I.LoadSceneAsync("Stage1", true);
    }
    Button[] buttons;
    void DisableAllButtons()
    {
        buttons = transform.root.GetComponentsInChildren<Button>();
        foreach (var btn in buttons)
            btn.enabled = false;
    }



}
