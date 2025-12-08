using System.Collections;
using UnityEngine;
public class Stage1Control : MonoBehaviour
{
    DialogControl dialogControl;
    PlayerControl playerControl;
    void Awake()
    {
        dialogControl = FindAnyObjectByType<DialogControl>();
        playerControl = FindAnyObjectByType<PlayerControl>();
    }

    IEnumerator Start()
    {
        playerControl.fsm.ChangeState(playerControl.stop);
        yield return YieldInstructionCache.WaitForSeconds(0.1f);
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


}
