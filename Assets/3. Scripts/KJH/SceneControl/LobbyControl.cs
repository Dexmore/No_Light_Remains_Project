using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
public class LobbyControl : MonoBehaviour
{
    [SerializeField] Button storyModeButton;
    void OnEnable()
    {
        storyModeButton.onClick.AddListener(StoryMode);
    }
    void OnDisable()
    {
        storyModeButton.onClick.RemoveListener(StoryMode);
    }
    void OnDestroy()
    {
        try { storyModeButton.onClick.RemoveListener(StoryMode); } catch { }
    }
    async void StoryMode()
    {
        AudioManager.I.PlaySFX("UIClick");
        AllButtonDisable();
        await Task.Delay(700);
        GameManager.I.LoadSceneAsync(2, true);
    }
    void AllButtonDisable()
    {
        storyModeButton.interactable = false;
    }



}
