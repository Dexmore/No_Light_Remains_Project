using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class LobbyUIManager_KWY : MonoBehaviour
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
        OnEsc();
    }

    private void Start()
    {
        float b = GameSettingDataManager_KWY.Instance.setting.brightness;
        Brightness_p.color = new Color(0, 0, 0, 1 - b);
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
            CloseTopPanel();
        }
        else
        {
            if (!Exit_p.activeSelf)
                OnExit();
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
