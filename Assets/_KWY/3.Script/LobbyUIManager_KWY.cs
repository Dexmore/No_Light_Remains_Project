using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LobbyUIManager_KWY : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject Title_p;
    [SerializeField] private GameObject Story_p;
    [SerializeField] private GameObject Boss_p;
    [SerializeField] private GameObject Setting_p;
    [SerializeField] private GameObject Exit_p;

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

    public void OnDontExit()
    {
        CloseTopPanel();
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
