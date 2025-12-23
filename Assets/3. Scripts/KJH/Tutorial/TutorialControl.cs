using System.Collections;
using UnityEngine;
public class TutorialControl : MonoBehaviour
{
    [SerializeField] PlayerControl playerControl;
    public int index;

    IEnumerator Start()
    {
        if (DBManager.I.GetProgress("Tutorial") < 0)
        {
            playerControl.stop.duration = 9999999;
            playerControl.fsm.ChangeState(playerControl.stop);
            yield return YieldInstructionCache.WaitForSeconds(1f);
            yield return new WaitUntil(() => !GameManager.I.isSceneWaiting);
            yield return YieldInstructionCache.WaitForSeconds(1f);
            GameManager.I.onDialog.Invoke(0, transform);
            Debug.Log("튜토리얼 시작 : Step1. 시작 스토리 다이얼로그 출력");
            DBManager.I.SetProgress("Tutorial", 1);
            yield return YieldInstructionCache.WaitForSeconds(1f);
            yield return new WaitUntil(() => !GameManager.I.isOpenDialog && !GameManager.I.isOpenPop);
            playerControl.fsm.ChangeState(playerControl.idle);
        }
    }
    public void TutorialTriggerEnter(string Name)
    {
        if (DBManager.I.GetProgress("Tutorial") == 1
        && true)
        {
            
        }

    }







}
