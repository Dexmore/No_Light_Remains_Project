using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEditor;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;

public class PopupUI : MonoBehaviour
{
    [SerializeField] private InputActionReference cancelAction;
    GameObject canvasGo;
    Transform[] allPopups;
    List<bool> isOpens = new List<bool>();
    [ReadOnlyInspector][SerializeField] int openPopCount;
    PlayerControl playerControl;
    void Awake()
    {
        canvasGo = transform.Find("PopupCanvas").gameObject;
        canvasGo.SetActive(false);
        allPopups = new Transform[canvasGo.transform.childCount - 1];
        isOpens.Clear();
        for (int i = 0; i < allPopups.Length; i++)
        {
            allPopups[i] = transform.Find("PopupCanvas").GetChild(i + 1);
            allPopups[i].gameObject.SetActive(false);
            isOpens.Add(false);
        }
        openPopCount = 0;
    }
    void OnEnable()
    {
        cancelAction.action.performed += InputESC;
        GameManager.I.onHitAfter += HandleHit;
        if (playerControl == null)
            playerControl = FindAnyObjectByType<PlayerControl>();
    }
    void OnDisable()
    {
        cancelAction.action.performed -= InputESC;
        GameManager.I.onHitAfter -= HandleHit;
    }
    void HandleHit(HitData hitData)
    {
        if (hitData.target.Root().name != "Player") return;
        if (allPopups[1].gameObject.activeSelf)
        {
            ClosePop(1);
        }
    }
    float coolTime = 0;
    void InputESC(InputAction.CallbackContext callbackContext)
    {
        if (Time.time - coolTime < 1.2f) return;
        coolTime = Time.time;
        for (int i = allPopups.Length - 1; i >= 0; i--)
        {
            if (allPopups[i].gameObject.activeSelf)
            {
                ClosePop(i);
                return;
            }
        }
    }
    public void OpenPop(int index)
    {
        OpenPop(index, true);
    }
    public void OpenPop(int index, bool sfx = true)
    {
        if (allPopups[index].gameObject.activeSelf) return;
        if (playerControl)
            if (playerControl.fsm.currentState != playerControl.stop)
                playerControl.fsm.ChangeState(playerControl.stop);
        canvasGo.SetActive(true);
        allPopups[index].gameObject.SetActive(true);
        if (sfx)
            AudioManager.I.PlaySFX("OpenPopup");
        DOTween.Kill(allPopups[index].transform);
        allPopups[index].transform.localScale = 0.7f * Vector3.one;
        allPopups[index].transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce).SetLink(gameObject);
        isOpens[index] = true;
        openPopCount++;
        GameManager.I.isOpenPop = true;
        if (index == 1)
        {
            Pop1Init();
        }
        if (index == 3)
        {
            TMP_Text tMP_Text = allPopups[index].transform.Find("Difficulty/Text").GetComponent<TMP_Text>();
            pop3Diff = 0;
            switch (SettingManager.I.setting.locale)
            {
                case 0:
                    tMP_Text.text = "Easy";
                    break;
                case 1:
                    tMP_Text.text = "쉬움";
                    break;
            }
            if (lobbyStoryPanel == null) lobbyStoryPanel = FindAnyObjectByType<LobbyStoryPanel>();
            lobbyStoryPanel.diff = 0;
        }
        if (tween == null)
            tween = DOVirtual.DelayedCall(1f, () => { SometimesGlitchTextLoop(); });
    }
    Tween tween;
    public void ClosePop(int index)
    {
        ClosePop(index, true);
    }
    public void ClosePop(int index, bool sfx = true)
    {
        if (!allPopups[index].gameObject.activeSelf) return;
        DOTween.Kill(allPopups[index].transform);
        if (sfx)
            AudioManager.I.PlaySFX("UIClick");
        allPopups[index].gameObject.SetActive(false);
        isOpens[index] = false;
        int find = isOpens.FindIndex(x => x == true);
        if (find == -1)
        {
            canvasGo.SetActive(false);
            DOVirtual.DelayedCall(0.4f, () => GameManager.I.isOpenPop = false).Play();
        }
        openPopCount--;
        if (index == 0)
        {
            DBManager.I.GetComponent<LoginUI>().canvasGroup.enabled = true;
        }
        if (index == 1)
        {
            Pop1UnInit();
        }
        if (openPopCount == 0)
        {
            tween?.Kill();
            tween = null;
        }
    }
    async void SometimesGlitchTextLoop()
    {
        Transform parent = canvasGo.transform;
        await Task.Delay(600);
        TMP_Text[] texts1 = parent.GetComponentsInChildren<TMP_Text>();
        Text[] texts2 = parent.GetComponentsInChildren<Text>();
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
                    GameManager.I.GlitchPartialText(text2, 6, 0.16f);
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
                    GameManager.I.GlitchPartialText(text1, 6, 0.16f);
                    if (Random.value < 0.73f)
                        AudioManager.I.PlaySFX("Glitch1");
                }
            }
            await Task.Delay(Random.Range(200, 800));
            if (!canvasGo.activeInHierarchy) return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public async void GoMainMenu()
    {
        AudioManager.I.PlaySFX("UIClick");
        await Task.Delay(100);
        ClosePop(1, false);
        await Task.Delay(500);
        GameManager.I.LoadSceneAsync("Lobby", false);
    }
    public async void QuitGame()
    {
        AudioManager.I.PlaySFX("UIClick");
        await Task.Delay(100);
        ClosePop(1, false);
        await Task.Delay(500);
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
    int pop3Diff = 0;
    LobbyStoryPanel lobbyStoryPanel;
    public void Pop3Left()
    {
        if (lobbyStoryPanel == null) lobbyStoryPanel = FindAnyObjectByType<LobbyStoryPanel>();
        AudioManager.I.PlaySFX("UIClick");
        TMP_Text tMP_Text = allPopups[3].transform.Find("Difficulty/Text").GetComponent<TMP_Text>();
        pop3Diff--;
        if (pop3Diff < 0) pop3Diff = 0;
        lobbyStoryPanel.diff = pop3Diff;
        switch (pop3Diff)
        {
            case 0:
                switch (SettingManager.I.setting.locale)
                {
                    case 0:
                        tMP_Text.text = "Easy";
                        break;
                    case 1:
                        tMP_Text.text = "쉬움";
                        break;
                }
                break;
            case 1:
                switch (SettingManager.I.setting.locale)
                {
                    case 0:
                        tMP_Text.text = "Normal";
                        break;
                    case 1:
                        tMP_Text.text = "보통";
                        break;
                }
                break;
            case 2:
                switch (SettingManager.I.setting.locale)
                {
                    case 0:
                        tMP_Text.text = "Hard";
                        break;
                    case 1:
                        tMP_Text.text = "어려움";
                        break;
                }
                break;
        }
    }
    public void Pop3Right()
    {
        if (lobbyStoryPanel == null) lobbyStoryPanel = FindAnyObjectByType<LobbyStoryPanel>();
        AudioManager.I.PlaySFX("UIClick");
        TMP_Text tMP_Text = allPopups[3].transform.Find("Difficulty/Text").GetComponent<TMP_Text>();
        pop3Diff++;
        if (pop3Diff > 2) pop3Diff = 2;
        lobbyStoryPanel.diff = pop3Diff;
        switch (pop3Diff)
        {
            case 0:
                switch (SettingManager.I.setting.locale)
                {
                    case 0:
                        tMP_Text.text = "Easy";
                        break;
                    case 1:
                        tMP_Text.text = "쉬움";
                        break;
                }
                break;
            case 1:
                switch (SettingManager.I.setting.locale)
                {
                    case 0:
                        tMP_Text.text = "Normal";
                        break;
                    case 1:
                        tMP_Text.text = "보통";
                        break;
                }
                break;
            case 2:
                switch (SettingManager.I.setting.locale)
                {
                    case 0:
                        tMP_Text.text = "Hard";
                        break;
                    case 1:
                        tMP_Text.text = "어려움";
                        break;
                }
                break;
        }
    }
    public void Pop3Start()
    {
        if (lobbyStoryPanel == null) lobbyStoryPanel = FindAnyObjectByType<LobbyStoryPanel>();
        //AudioManager.I.PlaySFX("UIClick");
        Debug.Log(pop3Diff);
        lobbyStoryPanel.diff = pop3Diff;
        lobbyStoryPanel.StartNewGameButton();
    }
    float _time;
    public void Pop4Button()
    {
        if (Time.time - _time < 1f) return;
        _time = Time.time;
        if (lobbyStoryPanel == null) lobbyStoryPanel = FindAnyObjectByType<LobbyStoryPanel>();
        lobbyStoryPanel.Pop4DeleteButton();
    }

    // 환경 설정 팝업
    Toggle pop1FsTogle;
    Button pop1LangLBtn;
    Button pop1LangRBtn;
    TMP_Text pop1LangText;
    Slider pop1BritSlder;
    Slider pop1BGMSlder;
    Slider pop1SFXSlder;
    void Pop1Init()
    {
        Transform tr = allPopups[1].transform;
        pop1FsTogle = tr.Find("Toggle").GetComponent<Toggle>();
        pop1LangLBtn = tr.Find("Language/LeftArrowButton").GetComponent<Button>();
        pop1LangText = tr.Find("Language/LanguageText").GetComponent<TMP_Text>();
        pop1LangRBtn = tr.Find("Language/RightArrowButton").GetComponent<Button>();
        pop1BritSlder = tr.Find("BrightnessSlider").GetComponent<Slider>();
        pop1BGMSlder = tr.Find("BGMSlider").GetComponent<Slider>();
        pop1SFXSlder = tr.Find("SFXSlider").GetComponent<Slider>();
        //
        pop1FsTogle.onValueChanged.AddListener(SetFullscreen);
        pop1LangLBtn.onClick.AddListener(SetLocaleLeft);
        pop1LangRBtn.onClick.AddListener(SetLocaleRight);
        pop1BritSlder.onValueChanged.AddListener(SetBrightness);
        pop1BGMSlder.onValueChanged.AddListener(SetBGMVolume);
        pop1SFXSlder.onValueChanged.AddListener(SetSFXVolume);
        //
        SettingData settings = SettingManager.I.setting;
        pop1FsTogle.isOn = settings.fullscreenMode == FullScreenMode.FullScreenWindow;
        switch (settings.locale)
        {
            case 0:
                pop1LangText.text = "English";
                break;
            case 1:
                pop1LangText.text = "Korean";
                break;
        }
        pop1BritSlder.value = settings.brightness;
        pop1BGMSlder.value = settings.bgmVolume;
        pop1SFXSlder.value = settings.sfxVolume;
    }
    void Pop1UnInit()
    {
        pop1FsTogle.onValueChanged.RemoveListener(SetFullscreen);
        pop1LangLBtn.onClick.RemoveListener(SetLocaleLeft);
        pop1LangRBtn.onClick.RemoveListener(SetLocaleRight);
        pop1BritSlder.onValueChanged.RemoveListener(SetBrightness);
        pop1BGMSlder.onValueChanged.RemoveListener(SetBGMVolume);
        pop1SFXSlder.onValueChanged.RemoveListener(SetSFXVolume);
        SettingManager.I.SaveSettings();
    }
    void SetFullscreen(bool value)
    {
        SettingManager.I.setting.fullscreenMode = (value) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreenMode = SettingManager.I.setting.fullscreenMode;
        SettingManager.I.SaveSettings();
    }
    void SetLocaleLeft() => ChangeLocale(-1);
    void SetLocaleRight() => ChangeLocale(1);
    async void ChangeLocale(int direction)
    {
        try
        {
            await LocalizationSettings.InitializationOperation.Task;
            var locales = LocalizationSettings.AvailableLocales.Locales;

            // 1. 현재 인덱스 계산 및 순환(Loop) 로직 추가
            int currentLocaleIndex = SettingManager.I.setting.locale;
            currentLocaleIndex = (currentLocaleIndex + direction + locales.Count) % locales.Count;

            // 2. 실제 로컬라이제이션 설정 변경
            if (currentLocaleIndex >= 0 && currentLocaleIndex < locales.Count)
            {
                LocalizationSettings.SelectedLocale = locales[currentLocaleIndex];

                // UI 텍스트 업데이트 (English / Korean 등)
                pop1LangText.text = locales[currentLocaleIndex].LocaleName.Split("(")[0];
            }

            // 3. 데이터 저장
            SettingManager.I.setting.locale = currentLocaleIndex;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Locale Change Failed: {e.Message}");
        }
    }
    private const float MIN_BRIGHTNESS = 0.06f;
    void SetBrightness(float value)
    {
        Image brightnessPanel = GameManager.I.transform.Find("BrightnessCanvas").GetComponentInChildren<Image>();
        SettingManager.I.setting.brightness = value;
        if (brightnessPanel != null)
        {
            brightnessPanel.color = new Color(0, 0, 0, Mathf.Clamp(1 - value, 0, 1 - MIN_BRIGHTNESS));
        }
        SettingManager.I.setting.brightness = Mathf.Clamp(value, MIN_BRIGHTNESS, 1f);
    }
    [SerializeField] AudioMixer audioMixer;
    void SetBGMVolume(float value)
    {
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20); // 오디오 믹서 연동 시
        SettingManager.I.setting.bgmVolume = value;
    }
    void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20); // 오디오 믹서 연동 시
        SettingManager.I.setting.sfxVolume = value;

    }
    public void ClickSound()
    {
        AudioManager.I.PlaySFX("UIClick");
    }








#if UNITY_EDITOR
    [Header("Editor Test")]
    public int testIndex;
    [Button]
    public void TestOpen()
    {
        OpenPop(testIndex);
    }
#endif






}
