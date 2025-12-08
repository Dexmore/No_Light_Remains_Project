using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LobbyControl : MonoBehaviour
{
    [Header("Input Action")]
    [SerializeField] private InputActionReference cancelAction;

    [Header("UI Panel")]
    [SerializeField] private GameObject Title_p;
    [SerializeField] private GameObject Story_p;
    [SerializeField] private GameObject Boss_p;
    [SerializeField] private GameObject Setting_p;
    [SerializeField] private GameObject Exit_p;
    [SerializeField] private GameObject ESC_i;
    [SerializeField] private Image Brightness_p;

    private Stack<GameObject> uiPanelStack = new Stack<GameObject>();


    private void Awake()
    {
        if (Title_p != null)
            Title_p.SetActive(true);

        if (Story_p != null)
            Story_p.SetActive(false);

        if (Boss_p != null)
            Boss_p.SetActive(false);

        if (Setting_p != null)
            Setting_p.SetActive(false);

        if (Exit_p != null)
            Exit_p.SetActive(false);

        if (ESC_i != null)
            ESC_i.SetActive(false);
    }

    private void OnEnable()
    {
        if (cancelAction != null)
        {
            cancelAction.action.performed += OnCancelPerformed;
        }
    }

    private void OnDisable()
    {
        if (cancelAction != null)
        {
            cancelAction.action.performed -= OnCancelPerformed;
        }
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        if(GameManager.I.isOpenPop) return;
        OnEsc();
    }

    private IEnumerator Start()
    {
        allButtons = Title_p.transform.root.GetComponentsInChildren<Button>();
        for (int i = 0; i < allButtons.Length; i++)
            allButtons[i].enabled = false;
        DBManager.I.LoadLocal();
        yield return YieldInstructionCache.WaitForSeconds(0.5f);
        Brightness_p = GameManager.I.transform.Find("BrightnessCanvas").GetComponentInChildren<Image>();
        float b = SettingManager.Instance.setting.brightness;
        Brightness_p.color = new Color(0, 0, 0, 1 - b);
        yield return YieldInstructionCache.WaitForSeconds(1.5f);
        InitSteam();
    }

    Button[] allButtons;
    PopupControl popupControl;
    void InitSteam()
    {
        if (!DBManager.I.IsSteam())
        {
            if (popupControl == null) GameManager.I.TryGetComponent(out popupControl);
            // 스팀 로그인에 실패하였습니다.
            popupControl.OpenPop(0);
            DBManager.I.GetComponent<LoginUI>().canvasGroup.enabled = false;
        }
        for (int i = 0; i < allButtons.Length; i++)
            allButtons[i].enabled = true;
    }

    private void Update()
    {
        if (Story_p.activeSelf || Boss_p.activeSelf || Setting_p.activeSelf || Exit_p.activeSelf)
            ESC_i.SetActive(true);
        else
            ESC_i.SetActive(false);
    }

    public void OnEsc()
    {
        if (uiPanelStack.Count > 0)
        {
            GameObject topPanel = uiPanelStack.Peek();
            if (topPanel == Setting_p)
            {
                LobbySettingPanel settingManager = Setting_p.GetComponent<LobbySettingPanel>();
                if (settingManager != null)
                {
                    bool handledBySettingManager = settingManager.OnEscPressed();
                    if (handledBySettingManager)
                    {
                        return;
                    }
                }
            }
            AudioManager.I.PlaySFX("Tick1");
            CloseTopPanel();
        }
        else
        {
            if (!Exit_p.activeSelf) { }
            else
                OnDontExit();
        }
    }

    public void OnStory()
    {
        OpenPanel(Story_p);
    }

    public void OnBoss()
    {
        OpenPanel(Boss_p);
    }

    public void OnSetting()
    {
        OpenPanel(Setting_p);
    }

    public void OnExit()
    {
        OpenPanel(Exit_p);
    }

    private void OnDontExit()
    {
        OnEsc();
    }

    public void OnExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OpenPanel(GameObject panelToOpen)
    {
        AudioManager.I.PlaySFX("UIClick");
        GameObject panelToHide = (uiPanelStack.Count > 0) ? uiPanelStack.Peek() : Title_p;
        uiPanelStack.Push(panelToOpen);

        panelToHide.SetActive(false);
        panelToOpen.SetActive(true);
    }

    private void CloseTopPanel()
    {
        if (uiPanelStack.Count > 0)
        {
            GameObject panelToClose = uiPanelStack.Pop();
            panelToClose.SetActive(false);
            GameObject panelToShow = (uiPanelStack.Count > 0) ? uiPanelStack.Peek() : Title_p;
            panelToShow.SetActive(true);
        }
    }
}
