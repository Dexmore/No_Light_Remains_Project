using System.Collections;
using UnityEngine;
public class TutorialControl : MonoBehaviour
{
    [SerializeField] PlayerControl playerControl;

    IEnumerator Start()
    {
        playerControl.stop.duration = 9999999;
        playerControl.fsm.ChangeState(playerControl.stop);
        yield return YieldInstructionCache.WaitForSeconds(1f);
        yield return new WaitUntil(() => !GameManager.I.isSceneWaiting);
        yield return YieldInstructionCache.WaitForSeconds(1f);
        Debug.Log("튜토리얼 1. 다이얼로그 출력");
        GameManager.I.onDialog.Invoke(0, transform);
        yield return YieldInstructionCache.WaitForSeconds(1f);
        yield return new WaitUntil(() => !GameManager.I.isOpenDialog && !GameManager.I.isOpenPop);
        playerControl.fsm.ChangeState(playerControl.idle);
    }
    

    


}
