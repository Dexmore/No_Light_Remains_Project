using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialControl : MonoBehaviour
{
    [SerializeField] PlayerControl playerControl;
    [SerializeField] MonsterControl blob;
    [SerializeField] MonsterControl slicer;
    [ReadOnlyInspector][SerializeField] int currentProgress = -1;
    IEnumerator Start()
    {
        yield return null;
        playerLayer = LayerMask.NameToLayer("Player");
        if (DBManager.I.GetProgress("Tutorial") < 0)
        {
            DBManager.I.Save();
            playerControl.stop.duration = 9999999;
            playerControl.fsm.ChangeState(playerControl.stop);
            yield return YieldInstructionCache.WaitForSeconds(1f);
            yield return new WaitUntil(() => !GameManager.I.isSceneWaiting);
            yield return YieldInstructionCache.WaitForSeconds(1f);
            if (playerControl.transform.position.x < 0)
            {
                Debug.Log("튜토리얼 시작 : 스토리 첫 대사 출력");
                GameManager.I.onDialog.Invoke(0, transform);
                DBManager.I.SetProgress("Tutorial", 1);
                currentProgress = 1;
                yield return YieldInstructionCache.WaitForSeconds(1f);
                yield return new WaitUntil(() => !GameManager.I.isOpenDialog && !GameManager.I.isOpenPop && !GameManager.I.isOpenInventory);
                playerControl.fsm.ChangeState(playerControl.idle);
                StartCoroutine(nameof(TutorialParryLoop));
                StartCoroutine(nameof(TutorialAttackLoop));
            }
            else
            {
                playerControl.fsm.ChangeState(playerControl.idle);
            }
        }
        yield return YieldInstructionCache.WaitForSeconds(1.5f);
        Transform tutParryTr = transform.Find("TutorialParry");
        if (slicer == null || !slicer.gameObject.activeInHierarchy)
        {
            StopCoroutine(nameof(TutorialParryLoop));
            tutParryTr.gameObject.SetActive(false);
        }
        Transform tutAttackTr = transform.Find("TutorialAttack");
        if (blob == null || !blob.gameObject.activeInHierarchy)
        {
            StopCoroutine(nameof(TutorialAttackLoop));
            tutAttackTr.gameObject.SetActive(false);
        }
    }
    int playerLayer;
    void OnEnable()
    {
        GameManager.I.onHit += HitHandler;
    }
    void OnDisable()
    {
        GameManager.I.onHit -= HitHandler;
    }
    void HitHandler(HitData hitData)
    {
        if (hitData.attacker != slicer.transform) return;
        if (hitData.target.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (hitData.attackType == HitData.AttackType.Chafe) return;
    }
    IEnumerator TutorialAttackLoop()
    {
        Transform tutAttackTr = transform.Find("TutorialAttack");
        while (true)
        {
            yield return null;
            tutAttackTr.position = blob.transform.position;
            yield return null;
            if (blob == null || !blob.gameObject.activeInHierarchy)
            {
                tutAttackTr.gameObject.SetActive(false);
                yield break;
            }
        }
    }


    int flag = 0;
    IEnumerator TutorialParryLoop()
    {
        Transform tutParryTr = transform.Find("TutorialParry");
        Animator animator = slicer.GetComponentInChildren<Animator>();
        Transform childTr = slicer.transform.GetChild(0);
        flag = 0;
        while (true)
        {
            yield return null;
            if (slicer == null || !slicer.gameObject.activeInHierarchy)
            {
                if (flag != 0) ResetParryTutorial(ref flag);
                tutParryTr.gameObject.SetActive(false);
                yield break;
            }
            tutParryTr.position = slicer.transform.position;
            float nt = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            Collider2D collider = Physics2D.OverlapCircle(slicer.transform.position + childTr.right, 4f, 1 << playerLayer);
            float dist = Vector2.Distance(playerControl.transform.position, slicer.transform.position);
            if (dist > 15f) // 15미터 이상 멀면 대기
            {
                if (flag != 0) ResetParryTutorial(ref flag);
                continue;
            }
            bool isPlayerReady = true;
            bool condition = false;
            if (flag == 1)
            {
                condition = (collider != null);
            }
            else
            {
                switch (slicer.state)
                {
                    case MonsterControl.State.NormalAttack:
                        condition = IsInParryWindow(animator, slicer.state.ToString(), nt, 0.42f, 0.46f, collider, isPlayerReady);
                        break;
                    case MonsterControl.State.MovingAttack:
                        condition = IsInParryWindow(animator, slicer.state.ToString(), nt, 0.53f, 0.58f, collider, isPlayerReady);
                        break;
                    case MonsterControl.State.ShortAttack:
                        condition = IsInParryWindow(animator, slicer.state.ToString(), nt, 0.46f, 0.50f, collider, isPlayerReady);
                        break;
                    default:
                        condition = false;
                        break;
                }
            }
            if (flag == 1 && playerControl.fsm.currentState == playerControl.parry)
            {
                ResetParryTutorial(ref flag);
            }
            else if (condition && flag == 0)
            {
                flag = 1;
                Time.timeScale = 0f;
                StartCoroutine(nameof(BlinkParryNotice));
            }
            else if (!condition && flag != 0)
            {
                ResetParryTutorial(ref flag);
            }
        }
    }
    private bool IsInParryWindow(Animator anim, string clipName, float nt, float start, float end, Collider2D col, bool playerReady)
    {
        return col != null && anim.GetCurrentAnimatorStateInfo(0).IsName(clipName)
               && nt >= start && nt <= end && playerReady;
    }
    private void ResetParryTutorial(ref int flag)
    {
        flag = 0;
        Time.timeScale = 1f;
        StopCoroutine(nameof(BlinkParryNotice));
        RecoverColorParryNotice();
    }
    IEnumerator BlinkParryNotice()
    {
        Transform wrap = transform.Find("TutorialParry/Canvas/Wrap");
        if (wrap == null) yield break;
        Text noticeText = wrap.Find("Text").GetComponent<Text>();
        Text buttonText = wrap.Find("Key/Text").GetComponent<Text>();
        Color defaultColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        Color highlightColor = Color.white;
        float blinkSpeed = 5.0f;
        float elapsedUnscaledTime = 0;
        while (elapsedUnscaledTime < 1.5f)
        {
            float timer = elapsedUnscaledTime * blinkSpeed;
            float t = (Mathf.Sin(timer) + 1f) * 0.5f;
            Color targetColor = Color.Lerp(defaultColor, highlightColor, t);
            SetColor(noticeText, targetColor);
            SetColor(buttonText, targetColor);
            yield return null;
            elapsedUnscaledTime += Time.unscaledDeltaTime;
        }
        Time.timeScale = 1f;
        DOVirtual.DelayedCall(0.15f, () => flag = 0);
        RecoverColorParryNotice();
    }
    void RecoverColorParryNotice()
    {
        StopCoroutine(nameof(BlinkParryNotice));
        Transform wrap = transform.Find("TutorialParry/Canvas/Wrap");
        if (wrap)
        {
            Text noticeText = wrap.Find("Text").GetComponent<Text>();
            Text buttonText = wrap.Find("Key/Text").GetComponent<Text>();
            SetColor(noticeText, new Color(0.2f, 0.2f, 0.2f, 1f));
            SetColor(buttonText, new Color(0.2f, 0.2f, 0.2f, 1f));
        }
    }
    private void SetColor(Graphic graphic, Color targetColor)
    {
        if (graphic != null) graphic.color = targetColor;
    }

    public void TutorialTrigger(string Name)
    {
        if (Name == "TutorialMove")
        {
            DBManager.I.SetProgress("Tutorial", 2);
            currentProgress = 2;
        }
        else if (Name == "TutorialLantern")
        {
            DBManager.I.SetProgress("Tutorial", 3);
            currentProgress = 3;
        }
        else if (Name == "TutorialJump")
        {
            DBManager.I.SetProgress("Tutorial", 4);
            currentProgress = 4;
        }
        else if (Name == "TutorialDash")
        {
            DBManager.I.SetProgress("Tutorial", 5);
            currentProgress = 5;
        }
        else if (Name == "TutorialAttack")
        {
            DBManager.I.SetProgress("Tutorial", 6);
            currentProgress = 6;
        }
        else if (Name == "TutorialInventory")
        {
            DBManager.I.SetProgress("Tutorial", 7);
            currentProgress = 7;
        }
        else if (Name == "TutorialDownJump")
        {
            DBManager.I.SetProgress("Tutorial", 8);
            currentProgress = 8;
        }
    }







}
