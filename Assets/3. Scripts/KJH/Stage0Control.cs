using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
public class Stage0Control : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActionAsset;
    DialogControl dialogControl;
    PlayerControl playerControl;
    void Awake()
    {
        dialogControl = FindAnyObjectByType<DialogControl>();
        playerControl = FindAnyObjectByType<PlayerControl>();
    }
    void Init()
    {
        GameManager.I.onSimpleTriggerEnter += HandlerSimpleTriggerEnter;
        GameManager.I.onSimpleTriggerExit += HandlerSimpleTriggerExit;
    }
    void OnDisable()
    {
        GameManager.I.onSimpleTriggerEnter -= HandlerSimpleTriggerEnter;
        GameManager.I.onSimpleTriggerExit -= HandlerSimpleTriggerExit;
    }
    IEnumerator Start()
    {
        playerControl.fsm.ChangeState(playerControl.stop);
        yield return YieldInstructionCache.WaitForSeconds(0.1f);
        Init();
        if (DBManager.I.currData.progress1 == 0)
        {
            while (GameManager.I.isSceneWaiting)
            {

                yield return YieldInstructionCache.WaitForSeconds(0.5f);
            }
            yield return YieldInstructionCache.WaitForSeconds(0.8f);
            dialogControl.Open(0);
        }


    }
    void HandlerSimpleTriggerEnter(int index, SimpleTrigger trigger)
    {
        switch (index)
        {
            case 0:
            case 1:
            case 3:
                trigger.transform.GetChild(0).gameObject.SetActive(true);
                CanvasGroup cg = trigger.GetComponentInChildren<CanvasGroup>();
                DOTween.Kill(cg);
                cg.alpha = 0f;
                cg.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
                break;
        }
        if (index == 2)
        {
            if (DBManager.I.currData.progress1 == 0)
            {
                DBManager.I.currData.progress1 = 1;
                dialogControl.Open(1);
            }
        }
    }
    void HandlerSimpleTriggerExit(int index, SimpleTrigger trigger)
    {
        switch (index)
        {
            case 0:
            case 1:
            case 3:
                CanvasGroup cg = trigger.GetComponentInChildren<CanvasGroup>();
                DOTween.Kill(cg);
                cg.DOFade(0f, 1.3f).SetEase(Ease.InSine).OnComplete(() => trigger.transform.GetChild(0).gameObject.SetActive(false));
                break;
        }
    }


}
