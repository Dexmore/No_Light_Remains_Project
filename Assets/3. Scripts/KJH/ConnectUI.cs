using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ConnectUI : MonoBehaviour
{
    [ReadOnlyInspector] public string userName;
    [ReadOnlyInspector] public Sprite userIcon;
    [SerializeField] Sprite offlineIcon;
    CanvasGroup canvasGroup;
    TMP_Text userNameTxt;
    Image userNameImg;
    void Awake()
    {
        transform.Find("Canvas/TopRight").TryGetComponent(out canvasGroup);
        userNameImg = canvasGroup.transform.Find("UserIcon").GetComponent<Image>();
        userNameTxt = canvasGroup.transform.Find("UserName").GetComponent<TMP_Text>();
    }
    public void Start()
    {
        SetUserName();
    }
    public void SetUserName()
    {
        if (!DBManager.I.IsSteam())
        {
            userName = "Offline";
            userIcon = offlineIcon;
            userNameTxt.text = userName;
            userNameImg.sprite = userIcon;
        }
        else
        {
            //표시 글자수 제한
            //NewTextTe...
        }
    }

    public void ReconnectionButton()
    {

    }

    public void LogoutButton()
    {
        
    }






}
