using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LobbyControl : MonoBehaviour
{
    [SerializeField] Transform lobbyUI;
    GameObject titlePanel;
    GameObject storyPanel;
    GameObject bossPanel;
    GameObject settingPanel;
    GameObject exitPanel;
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
        if (DBManager.I.IsSteam())
        {
            for (int i = 0; i < 3; i++)
            {
                if (i < DBManager.I.allSaveDataSteam.characterDatas.Count)
                {
                    saveSlots.GetChild(i).Find("NoData").gameObject.SetActive(false);
                    Transform dataPanel = saveSlots.GetChild(i).Find("Data_Panel");
                    dataPanel.gameObject.SetActive(true);
                    TMP_Text[] tMP_Texts = dataPanel.GetComponentsInChildren<TMP_Text>();
                    CharacterData characterData = DBManager.I.allSaveDataSteam.characterDatas[i];
                    tMP_Texts[0].text = $"위치 : {characterData.sceneName}";
                    tMP_Texts[1].text = $"재화 : {characterData.money} 원";
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
            // 스팀 로그인에 실패하였습니다.
            // 다시 연결하려면 오른쪽 상단의 재시도 버튼을 눌러주세요.
            // 오프라인 상태에서는. 로컬 컴퓨터에 저장된 캐릭터들만 플레이 할 수 있습니다.
            for (int i = 0; i < 3; i++)
            {
                if (i < DBManager.I.allSaveDataLocal.characterDatas.Count)
                {
                    saveSlots.GetChild(i).Find("NoData").gameObject.SetActive(false);
                    Transform dataPanel = saveSlots.GetChild(i).Find("Data_Panel");
                    dataPanel.gameObject.SetActive(true);
                    TMP_Text[] tMP_Texts = dataPanel.GetComponentsInChildren<TMP_Text>();
                    CharacterData characterData = DBManager.I.allSaveDataLocal.characterDatas[i];
                    tMP_Texts[0].text = $"위치 : {characterData.sceneName}";
                    tMP_Texts[1].text = $"재화 : {characterData.money} 원";
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
        select = index;
        Transform saveSlots = storyPanel.transform.Find("SaveSlots");
        if (saveSlots.GetChild(index).Find("NoData").gameObject.activeSelf)
        {
            StartGame_StoryModeNoData(index);
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
        DBManager.I.currentCharData = DBManager.I.allSaveDataSteam.characterDatas[select];
        DBManager.I.currentSlotIndex = select;
        await Task.Delay(100);
        GameManager.I.LoadSceneAsync(DBManager.I.currentCharData.sceneName, true);
    }
    public async void StartGame_StoryModeNoData(int index)
    {
        AudioManager.I.PlaySFX("UIClick");
        DisableAllButton();
        GameObject button = storyPanel.transform.Find("StartButton").gameObject;
        button.SetActive(false);
        await Task.Delay(1500);
        CharacterData newData = new CharacterData();
        newData.money = 0;
        newData.sceneName = "Stage1";
        newData.lastPosition = Vector2.zero;
        newData.HP = 400;
        newData.MP = 0;
        newData.potionCount = 5;
        newData.itemDatas = new List<CharacterData.ItemData>();
        newData.gearDatas = new List<CharacterData.GearData>();
        newData.lanternDatas = new List<CharacterData.LanternData>();
        DBManager.I.currentCharData = newData;
        DBManager.I.currentSlotIndex = 0;
        if (isSteamSlot)
        {
            DBManager.I.allSaveDataSteam.characterDatas.Add(newData);
        }
        else
        {
            DBManager.I.allSaveDataLocal.characterDatas.Add(newData);
        }
        await Task.Delay(1);
        DBManager.I.Save();
        await Task.Delay(700);
        GameManager.I.LoadSceneAsync("Stage1", true);
    }




    #endregion
    public void BossPanelOpen()
    {
        titlePanel.SetActive(false);
        storyPanel.SetActive(false);
        bossPanel.SetActive(true);
        settingPanel.SetActive(false);
        exitPanel.SetActive(false);
    }
    public void SettingPanelOpen()
    {
        titlePanel.SetActive(false);
        storyPanel.SetActive(false);
        bossPanel.SetActive(false);
        settingPanel.SetActive(true);
        exitPanel.SetActive(false);
    }
    public void ExitPanelOpen()
    {
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
