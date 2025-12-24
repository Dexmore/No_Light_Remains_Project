using System.Collections;
using UnityEngine;
public class TutorialControl : MonoBehaviour
{
    [SerializeField] PlayerControl playerControl;
    [SerializeField] MonsterControl slicer;
    [ReadOnlyInspector][SerializeField] int currentProgress = -1;
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
            Debug.Log("튜토리얼 : Step1. 시작스토리 다이얼로그 출력");
            DBManager.I.SetProgress("Tutorial", 1);
            currentProgress = 1;
            yield return YieldInstructionCache.WaitForSeconds(1f);
            yield return new WaitUntil(() => !GameManager.I.isOpenDialog && !GameManager.I.isOpenPop && !GameManager.I.isOpenInventory);
            playerControl.fsm.ChangeState(playerControl.idle);
            StartCoroutine(nameof(TutorialParryLoop));
        }
    }
    IEnumerator TutorialParryLoop()
    {
        Transform tutParryTr = transform.Find("TutorialParry");
        while(true)
        {
            yield return null;
            tutParryTr.position = slicer.transform.position;
            yield return null;
            if(slicer == null || !slicer.gameObject.activeInHierarchy)
            {
                tutParryTr.gameObject.SetActive(false);
            }
        }
    }
    public void TutorialTrigger(string Name)
    {
        if(Name == "TutorialMove")
        {
            //Debug.Log("튜토리얼 : Step2. Move 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 2);
            currentProgress = 2;
        }
        else if(Name == "TutorialLantern")
        {
            //Debug.Log("튜토리얼 : Step3. Lantern 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 3);
            currentProgress = 3;
        }
        else if(Name == "TutorialJump")
        {
            //Debug.Log("튜토리얼 : Step4. Jump 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 4);
            currentProgress = 4;
        }
        else if(Name == "TutorialDash")
        {
            //Debug.Log("튜토리얼 : Step5. Dash 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 5);
            currentProgress = 5;
        }
        else if(Name == "TutorialAttack")
        {
            //Debug.Log("튜토리얼 : Step6. Attack 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 6);
            currentProgress = 6;
        }
        else if(Name == "TutorialInventory")
        {
            //Debug.Log("튜토리얼 : Step7. Inventory 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 7);
            currentProgress = 7;
        }
        else if(Name == "TutorialDownJump")
        {
            //Debug.Log("튜토리얼 : Step8. DownJump 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 8);
            currentProgress = 8;
        }
    }







}
