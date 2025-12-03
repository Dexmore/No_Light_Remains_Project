using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
public class LobbyControl : MonoBehaviour
{
    [SerializeField] Transform lobbyUI;
    GameObject titlePanel;
    GameObject storyPanel;
    GameObject bossPanel;
    GameObject settingPanel;
    GameObject exitPanel;
    PopupControl popupControl;
    void Awake()
    {
        titlePanel = lobbyUI.Find("Title_Panel").gameObject;
        storyPanel = lobbyUI.Find("Story_Panel").gameObject;
        bossPanel = lobbyUI.Find("Boss_Panel").gameObject;
        settingPanel = lobbyUI.Find("Setting_Panel").gameObject;
        exitPanel = lobbyUI.Find("Exit_Panel").gameObject;
    }
    void Start()
    {
        titlePanel.SetActive(true);
        storyPanel.SetActive(false);
        bossPanel.SetActive(false);
        settingPanel.SetActive(false);
        exitPanel.SetActive(false);
        GameManager.I.TryGetComponent(out popupControl);
        DBManager.I.LoadLocal();
        StartAfter();
    }
    async void StartAfter()
    {
        await Task.Delay(3500);
        if (!DBManager.I.IsSteam())
        {
            // 스팀 로그인에 실패하였습니다.
            popupControl.OpenPop(0);
            DBManager.I.GetComponent<LoginUI>().canvasGroup.enabled = false;
        }
    }
    #region Story Panel
    public void StoryPanelOpen()
    {
        AudioManager.I.PlaySFX("UIClick");
        titlePanel.SetActive(false);
        storyPanel.SetActive(true);
        bossPanel.SetActive(false);
        settingPanel.SetActive(false);
        exitPanel.SetActive(false);
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
    public async void StartGame_StoryMode()
    {
        AudioManager.I.PlaySFX("UIClick");
        DisableAllButton();
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
    public async void StartGame_StoryModeNoData()
    {
        AudioManager.I.PlaySFX("UIClick");
        DisableAllButton();
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
        newData.itemDatas = new List<CharacterData.ItemData>();
        newData.gearDatas = new List<CharacterData.GearData>();
        newData.lanternDatas = new List<CharacterData.LanternData>();
        DBManager.I.currData = newData;
        DBManager.I.currSlot = select;
        // 신규캐릭터 시작 아이템
        DBManager.I.AddItem("Useful Sword", 1);
        DBManager.I.AddItem("Helmet", 1);
        DBManager.I.AddItem("Leather Armor", 1);
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
    #endregion
    public void BossPanelOpen()
    {
        AudioManager.I.PlaySFX("UIClick");
        titlePanel.SetActive(false);
        storyPanel.SetActive(false);
        bossPanel.SetActive(true);
        settingPanel.SetActive(false);
        exitPanel.SetActive(false);
    }
    public void SettingPanelOpen()
    {
        AudioManager.I.PlaySFX("UIClick");
        titlePanel.SetActive(false);
        storyPanel.SetActive(false);
        bossPanel.SetActive(false);
        settingPanel.SetActive(true);
        exitPanel.SetActive(false);
    }
    public void ExitPanelOpen()
    {
        AudioManager.I.PlaySFX("UIClick");
        titlePanel.SetActive(false);
        storyPanel.SetActive(false);
        bossPanel.SetActive(false);
        settingPanel.SetActive(false);
        exitPanel.SetActive(true);
    }
    void DisableAllButton()
    {

    }



}
