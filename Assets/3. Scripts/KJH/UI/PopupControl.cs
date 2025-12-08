using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEditor;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;

public class PopupControl : MonoBehaviour
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
            if (playerControl.fsm.currentState != playerControl.openUIMenu)
                playerControl.fsm.ChangeState(playerControl.openUIMenu);
        canvasGo.SetActive(true);
        allPopups[index].gameObject.SetActive(true);
        if (sfx)
            AudioManager.I.PlaySFX("OpenPopup");
        DOTween.Kill(allPopups[index].transform);
        allPopups[index].transform.localScale = 0.7f * Vector3.one;
        allPopups[index].transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
        isOpens[index] = true;
        openPopCount++;
        GameManager.I.isOpenPop = true;
        if (index == 3)
        {
            TMP_Text tMP_Text = allPopups[index].transform.Find("Difficulty/Text").GetComponent<TMP_Text>();
            pop3Diff = 1;
            tMP_Text.text = $"보통";
            if (lobbyStoryPanel == null) lobbyStoryPanel = FindAnyObjectByType<LobbyStoryPanel>();
            lobbyStoryPanel.diff = 1;
        }
    }
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
    }
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
    int pop3Diff = 1;
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
                tMP_Text.text = $"쉬움";
                break;
            case 1:
                tMP_Text.text = $"보통";
                break;
            case 2:
                tMP_Text.text = $"어려움";
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
                tMP_Text.text = $"쉬움";
                break;
            case 1:
                tMP_Text.text = $"보통";
                break;
            case 2:
                tMP_Text.text = $"어려움";
                break;
        }
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
