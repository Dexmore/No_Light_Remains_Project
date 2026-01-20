using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class LogoControl : MonoBehaviour
{
    [SerializeField] Canvas logo_UI;
    RectTransform logoRT;
    RectTransform[] logoChildrenRT;
    Vector2[] logoChildrenInitPos;
    Image[] logoChildrenImg;
    void Awake()
    {
        logoRT = logo_UI.transform.Find("Logo").GetComponent<RectTransform>();
    }
    IEnumerator Start()
    {
        DOTween.Init();
        #region Logo Animation
        logoChildrenRT = new RectTransform[logoRT.childCount];
        logoChildrenInitPos = new Vector2[logoRT.childCount];
        logoChildrenImg = new Image[logoRT.childCount];
        // Setting
        for (int i = 0; i < logoChildrenRT.Length; i++)
        {
            logoChildrenRT[i] = logoRT.GetChild(i).GetComponent<RectTransform>();
            logoChildrenInitPos[i] = logoChildrenRT[i].anchoredPosition;
            logoChildrenImg[i] = logoChildrenRT[i].GetComponent<Image>();
            logoChildrenImg[i].color = new Color(1f, 1f, 1f, 0f);
        }
        yield return null;
        yield return YieldInstructionCache.WaitForSeconds(1f);
        // 글자 'i' 등장 연출
        logoChildrenRT[7].anchoredPosition = new Vector2(480, logoChildrenInitPos[7].y);
        logoChildrenImg[7].DOFade(1f, 0.3f).SetEase(Ease.InSine).SetLink(gameObject);
        yield return YieldInstructionCache.WaitForSeconds(0.2f);
        AudioManager.I.PlaySFX("Stretch");
        logoChildrenRT[7].DOScaleY(2.5f, 0.7f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            logoChildrenRT[7].DOScaleY(1f, 0.25f).SetEase(Ease.OutBounce).SetLink(gameObject);
        }).SetLink(gameObject);
        logoChildrenRT[16].anchoredPosition = new Vector2(logoChildrenInitPos[16].x + (480 - logoChildrenInitPos[7].x), logoChildrenInitPos[16].y);
        yield return YieldInstructionCache.WaitForSeconds(1.3f);
        logoChildrenImg[16].DOFade(1f, 1.5f).SetLink(gameObject);
        logoChildrenRT[7].DOAnchorPos(logoChildrenInitPos[7], 0.55f).SetEase(Ease.OutQuad).SetLink(gameObject);
        logoChildrenRT[16].DOAnchorPos(logoChildrenInitPos[16], 0.55f).SetEase(Ease.OutQuad).SetLink(gameObject);
        yield return null;
        // 나머지 글자 켜지면서 오른쪽으로 펼치기
        for (int i = 0; i < logoChildrenRT.Length; i++)
        {
            if (i == 7 || i == 16) continue;
            logoChildrenRT[i].anchoredPosition = new Vector2(480, logoChildrenInitPos[7].y);
            logoChildrenRT[i].DOAnchorPos(logoChildrenInitPos[i], 0.55f).SetEase(Ease.OutQuad).SetLink(gameObject);
            logoChildrenImg[i].DOFade(1f, 0.3f).SetEase(Ease.InSine).SetLink(gameObject);
        }
        AudioManager.I.PlaySFX("Logo");
        yield return YieldInstructionCache.WaitForSeconds(1.9f);
        for (int i = 0; i < logoChildrenRT.Length; i++)
        {
            logoChildrenImg[i].DOFade(0f, 1.3f).SetEase(Ease.OutSine).SetLink(gameObject);
        }
        #endregion
        yield return YieldInstructionCache.WaitForSeconds(1f);
        GameManager.I.LoadSceneAsync("Lobby");
    }

}
