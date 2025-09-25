using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class LobbyControl : MonoBehaviour
{
    public Button[] buttons;
    IEnumerator Start()
    {
        yield return null;
        yield return YieldInstructionCache.WaitForSeconds(0.5f);
        GameManager.I.FadeIn(2f);
        yield return YieldInstructionCache.WaitForSeconds(2f);
    }
    void OnEnable()
    {
        buttons[0].onClick.AddListener(StartButton);
    }
    void OnDisable()
    {
        buttons[0].onClick.RemoveListener(StartButton);
    }
    void StartButton()
    {
        StartCoroutine(nameof(StartButton_co));
    }
    IEnumerator StartButton_co()
    {
        buttons[0].enabled = false;
        yield return YieldInstructionCache.WaitForSeconds(0.5f);
        GameManager.I.FadeOut(1.2f);
        yield return YieldInstructionCache.WaitForSeconds(1.2f);
        GameManager.I.LoadSceneAsync(2);
    }




}
