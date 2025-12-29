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
            GameManager.I.onDialog.Invoke(0, transform);
            Debug.Log("튜토리얼 시작 : 스토리 첫 대사 출력");
            DBManager.I.SetProgress("Tutorial", 1);
            currentProgress = 1;
            yield return YieldInstructionCache.WaitForSeconds(1f);
            yield return new WaitUntil(() => !GameManager.I.isOpenDialog && !GameManager.I.isOpenPop && !GameManager.I.isOpenInventory);
            playerControl.fsm.ChangeState(playerControl.idle);
            StartCoroutine(nameof(TutorialParryLoop));
            StartCoroutine(nameof(TutorialAttackLoop));
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
        // Time.timeScale = 1f;
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
                Time.timeScale = 1f;
            }
        }
    }


    IEnumerator TutorialParryLoop()
    {
        Transform tutParryTr = transform.Find("TutorialParry");
        Animator animator = slicer.GetComponentInChildren<Animator>();
        Transform childTr = slicer.transform.GetChild(0);
        int flag = 0;
        while (true)
        {
            yield return null;

            // 2. 몹이 없으면 UI 끄고 대기
            if (slicer == null || !slicer.gameObject.activeInHierarchy)
            {
                if (flag != 0) ResetParryTutorial(ref flag);
                tutParryTr.gameObject.SetActive(false);
                continue;
            }

            tutParryTr.position = slicer.transform.position;
            float nt = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            Collider2D collider = Physics2D.OverlapCircle(slicer.transform.position + childTr.right, 2f, 1 << playerLayer);

            // 1. 타임아웃 등으로 인해 외부에서 TimeScale이 복구되었다면 flag 리셋
            if (flag == 1 && Time.timeScale == 1f) flag = 0;

            // 3. [추가] 거리 체크: 몹과 너무 멀면 로직 실행 안 함
            float dist = Vector2.Distance(playerControl.transform.position, slicer.transform.position);
            if (dist > 15f) // 15미터 이상 멀면 대기
            {
                if (flag != 0) ResetParryTutorial(ref flag);
                //tutParryTr.gameObject.SetActive(false);
                continue;
            }

            bool isPlayerReady = playerControl.fsm.currentState == playerControl.idle ||
                                 playerControl.fsm.currentState == playerControl.run ||
                                 playerControl.fsm.currentState == playerControl.attack ||
                                 playerControl.fsm.currentState == playerControl.attackCombo ||
                                 playerControl.fsm.currentState == playerControl.hit;

            bool condition = false;

            // 4. 패링 타이밍 로직 (슬로우 중에는 거리만 체크)
            if (flag == 1)
            {
                condition = (collider != null); // 이미 슬로우 중이면 플레이어 상태 무관하게 유지
            }
            else
            {
                switch (slicer.state)
                {
                    case MonsterControl.State.NormalAttack:
                        condition = IsInParryWindow(animator, slicer.state.ToString(), nt, 0.42f, 0.49f, collider, isPlayerReady);
                        break;
                    case MonsterControl.State.MovingAttack:
                        condition = IsInParryWindow(animator, slicer.state.ToString(), nt, 0.46f, 0.53f, collider, isPlayerReady);
                        break;
                    case MonsterControl.State.ShortAttack:
                        condition = IsInParryWindow(animator, slicer.state.ToString(), nt, 0.45f, 0.51f, collider, isPlayerReady);
                        break;
                    default:
                        condition = false;
                        break;
                }
            }

            // 5. 실행 제어
            if (flag == 1 && playerControl.fsm.currentState == playerControl.parry)
            {
                ResetParryTutorial(ref flag);
            }
            else if (condition && flag == 0)
            {
                flag = 1;
                Time.timeScale = 0.03f;
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

        while (elapsedUnscaledTime < 2.0f) // 2초 타임아웃 적용 (원하시는 대로 수정)
        {
            float timer = elapsedUnscaledTime * blinkSpeed;
            float t = (Mathf.Sin(timer) + 1f) * 0.5f;
            Color targetColor = Color.Lerp(defaultColor, highlightColor, t);
            SetColor(noticeText, targetColor);
            SetColor(buttonText, targetColor);

            yield return null;
            elapsedUnscaledTime += Time.unscaledDeltaTime;
        }

        // 2초 경과 시 강제 복구
        Time.timeScale = 1f;
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
            //Debug.Log("튜토리얼 : Step2. Move 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 2);
            currentProgress = 2;
        }
        else if (Name == "TutorialLantern")
        {
            //Debug.Log("튜토리얼 : Step3. Lantern 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 3);
            currentProgress = 3;
        }
        else if (Name == "TutorialJump")
        {
            //Debug.Log("튜토리얼 : Step4. Jump 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 4);
            currentProgress = 4;
        }
        else if (Name == "TutorialDash")
        {
            //Debug.Log("튜토리얼 : Step5. Dash 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 5);
            currentProgress = 5;
        }
        else if (Name == "TutorialAttack")
        {
            //Debug.Log("튜토리얼 : Step6. Attack 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 6);
            currentProgress = 6;
        }
        else if (Name == "TutorialInventory")
        {
            //Debug.Log("튜토리얼 : Step7. Inventory 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 7);
            currentProgress = 7;
        }
        else if (Name == "TutorialDownJump")
        {
            //Debug.Log("튜토리얼 : Step8. DownJump 조작법 안내");
            DBManager.I.SetProgress("Tutorial", 8);
            currentProgress = 8;
        }
    }







}
