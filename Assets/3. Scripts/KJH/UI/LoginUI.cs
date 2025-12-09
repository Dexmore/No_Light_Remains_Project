using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using DG.Tweening;
public class LoginUI : MonoBehaviour
{
    [ReadOnlyInspector] public string userName;
    [ReadOnlyInspector] public Sprite userIcon;
    [SerializeField] Sprite offlineIcon;
    [HideInInspector] public CanvasGroup canvasGroup;
    TMP_Text userNameTxt;
    const int MAX_NICKNAME_LENGTH = 9; // 영어 기준 최대 9글자
    Image userNameImg;
    GameObject buttonLogout;
    GameObject buttonConnect;
    void Awake()
    {
        transform.Find("Canvas/TopRight").TryGetComponent(out canvasGroup);
        userNameImg = canvasGroup.transform.Find("UserIcon(Mask)/UserIcon").GetComponent<Image>();
        userNameTxt = canvasGroup.transform.Find("UserName").GetComponent<TMP_Text>();
        buttonLogout = canvasGroup.transform.Find("Button/LogoutButton").gameObject;
        buttonConnect = canvasGroup.transform.Find("Button/ConnectButton").gameObject;
    }
    IEnumerator Start()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.5f);
        RefreshUserInfoInUI();
    }
    public void RefreshUserInfoInUI()
    {
        if (!DBManager.I.IsSteamInit())
        {
            userName = "Offline";
            userIcon = offlineIcon;
            userNameTxt.text = userName;
            userNameImg.sprite = userIcon;
            buttonLogout.SetActive(false);
            buttonConnect.SetActive(true);
        }
        else
        {
            string steamName = SteamFriends.GetPersonaName();
            if (steamName.Length > MAX_NICKNAME_LENGTH)
                userName = steamName.Substring(0, MAX_NICKNAME_LENGTH) + "...";
            else
                userName = steamName;
            userNameTxt.text = userName;
            buttonLogout.SetActive(true);
            buttonConnect.SetActive(false);
            int steamImageHandle = SteamFriends.GetLargeFriendAvatar(SteamUser.GetSteamID());
            Texture2D avatarTexture = GetSteamImageAsTexture2D(steamImageHandle);
            if (avatarTexture != null)
            {
                userIcon = Sprite.Create(
                    avatarTexture,
                    new Rect(0, 0, avatarTexture.width, -avatarTexture.height),
                    Vector2.one * 0.5f,
                    100f
                );
                userNameImg.sprite = userIcon;
            }
            else
            {
                userIcon = offlineIcon;
                userNameImg.sprite = userIcon;
            }
        }
    }
    public void ConnectButton()
    {
        if (Time.time - connectCoolTime > 1.2f)
        {
            connectCoolTime = Time.time;
            AudioManager.I.PlaySFX("UIClick2");
            StopCoroutine(nameof(ConnectButton_co));
            StartCoroutine(nameof(ConnectButton_co));
        }
    }
    float connectCoolTime = 0;
    IEnumerator ConnectButton_co()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.3f);
        buttonConnect.SetActive(false);
        Image waiting = userNameTxt.transform.Find("Waiting").GetComponent<Image>();
        waiting.gameObject.SetActive(true);
        DOTween.Kill(waiting);
        DOTween.Kill(waiting.transform);
        waiting.color = new Color(1f, 1f, 1f, 0f);
        waiting.DOFade(1f, 0.8f);
        waiting.transform.DOLocalRotate(-360f * Vector3.forward, 2.5f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
        // 기다리는 연출 (Circle wating)
        DBManager.I.StartSteam();
        yield return YieldInstructionCache.WaitForSeconds(1f);
        float _time = Time.time;
        while (Time.time - _time < 3f)
        {
            yield return YieldInstructionCache.WaitForSeconds(0.5f);
            if (DBManager.I.IsSteam())
            {
                yield return YieldInstructionCache.WaitForSeconds(0.2f);
                DBManager.I.LoadSteam();
                DBManager.I.LoadLocal();
                yield return YieldInstructionCache.WaitForSeconds(0.2f);
                RefreshUserInfoInUI();
                DOTween.Kill(waiting);
                DOTween.Kill(waiting.transform);
                waiting.gameObject.SetActive(false);
                yield break;
            }
        }
        // 시간초과
        DOTween.Kill(waiting);
        DOTween.Kill(waiting.transform);
        waiting.gameObject.SetActive(false);
        buttonConnect.SetActive(true);
    }
    float logoutCoolTime = 0;
    public void LogoutButton()
    {
        if (Time.time - logoutCoolTime > 1.2f)
        {
            logoutCoolTime = Time.time;
            AudioManager.I.PlaySFX("UIClick2");
            StopCoroutine(nameof(LogoutButton_co));
            StartCoroutine(nameof(LogoutButton_co));
        }
    }
    IEnumerator LogoutButton_co()
    {
        yield return null;
        DBManager.I.StopSteam();
        yield return null;
        yield return new WaitUntil(() => DBManager.I.IsSteamInit() == false);
        RefreshUserInfoInUI();
    }
    // Steam 아바타 핸들에서 Texture2D를 가져오는 헬퍼 함수 (예시)
    private Texture2D GetSteamImageAsTexture2D(int iImage)
    {
        if (iImage == -1)
        {
            Debug.LogError("Invalid Steam Image Handle.");
            return null;
        }
        uint width = 0, height = 0;
        if (!SteamUtils.GetImageSize(iImage, out width, out height))
        {
            Debug.LogError("Failed to get Steam image size.");
            return null;
        }
        int dataSize = (int)(width * height * 4);
        byte[] pvData = new byte[dataSize];
        if (SteamUtils.GetImageRGBA(iImage, pvData, dataSize))
        {
            Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
            texture.LoadRawTextureData(pvData);
            texture.Apply();
            return texture;
        }
        else
        {
            Debug.LogError("Failed to get Steam image RGBA data.");
            return null;
        }
    }



}
