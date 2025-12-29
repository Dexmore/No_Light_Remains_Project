using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class TutorialTrigger : MonoBehaviour
{
    TutorialControl tutorialControl;
    GameObject canvasObject;
    CanvasGroup canvasGroup;
    int playerLayer;
    Tween tween;
    Text[] keyTexts;
    IEnumerator Start()
    {
        ready = false;
        transform.parent.TryGetComponent(out tutorialControl);
        playerLayer = LayerMask.NameToLayer("Player");
        canvasObject = transform.GetChild(0).gameObject;
        canvasObject.TryGetComponent(out canvasGroup);
        Transform warp = canvasObject.transform.GetChild(0);
        keyTexts = new Text[warp.childCount - 1];
        for (int i = 0; i < warp.childCount-1; i++)
        {
            keyTexts[i] = warp.GetChild(i+1).GetChild(0).GetComponent<Text>();
        }
        if (transform.name == "TutorialMove")
        {
            keyTexts[0].text = SettingManager.I.GetBindingName("Move", 0);
            keyTexts[1].text = SettingManager.I.GetBindingName("Move", 1);
        }
        if (transform.name == "TutorialLantern")
        {
            keyTexts[0].text = SettingManager.I.GetBindingName("Lantern");
        }
        if (transform.name == "TutorialJump")
        {
            keyTexts[0].text = SettingManager.I.GetBindingName("Jump");
        }
        if (transform.name == "TutorialDash")
        {
            keyTexts[0].text = SettingManager.I.GetBindingName("Move", 1);
            keyTexts[1].text = SettingManager.I.GetBindingName("Move", 1);
        }
        if (transform.name == "TutorialAttack")
        {
            keyTexts[0].text = SettingManager.I.GetBindingName("Attack");
        }
        if (transform.name == "TutorialInventory")
        {
            keyTexts[0].text = SettingManager.I.GetBindingName("Inventory");
        }
        if (transform.name == "TutorialMenu")
        {
            keyTexts[0].text = "Esc";
        }
        if (transform.name == "TutorialDownJump")
        {
            keyTexts[0].text = SettingManager.I.GetBindingName("Move", 2);
            keyTexts[1].text = SettingManager.I.GetBindingName("Jump");
        }
        if (transform.name == "TutorialParry")
        {
            keyTexts[0].text = SettingManager.I.GetBindingName("Parry");
        }
        if (transform.name == "TutorialHeal")
        {
            keyTexts[0].text = SettingManager.I.GetBindingName("Potion");
        }
        yield return YieldInstructionCache.WaitForSeconds(1.8f);
        ready = true;
    }
    bool ready = false;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != playerLayer) return;
        StopCoroutine(nameof(RunWait));
        StartCoroutine(nameof(RunWait));
    }
    //SettingManager.I.GetBindingName("LanternInteraction")
    IEnumerator RunWait()
    {
        //Debug.Log("aaa2");
        yield return new WaitUntil(() => !GameManager.I.isOpenDialog && !GameManager.I.isOpenPop && !GameManager.I.isOpenInventory && ready);
        //Debug.Log("aaa3");
        tutorialControl.TutorialTrigger(transform.name);
        canvasObject.SetActive(true);
        tween?.Kill();
        canvasGroup.alpha = 0f;
        tween = canvasGroup.DOFade(0.8f, 1f).SetEase(Ease.InSine).Play();

    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != playerLayer) return;
        tween?.Kill();
        float myFloat = canvasGroup.alpha;
        tween = canvasGroup.DOFade(0f, 1.6f - myFloat).SetEase(Ease.InSine).OnComplete(() => { canvasObject.SetActive(false); }).Play();
    }





}
