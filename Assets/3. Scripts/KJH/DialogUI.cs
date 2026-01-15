using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;

public class DialogUI : MonoBehaviour
{
    List<string[]> allDialogTexts = new List<string[]>();
    void InitLocale()
    {
        // 영어
        if (SettingManager.I.setting.locale == 0)
        {
            allDialogTexts = new List<string[]>()
            {
                //대사0 (게임시작 (이 대사 이후 바로 튜토리얼 시작))
                new string[]
                {
                    "[BOOT SEQUENCE INITIATED]\nCORE TEMPERATURE STABLEIZING...COMPLETE.\nNEURAL RESPONSE CORE CONNECTION...COMPLETE",
                    "COMBAT MODULE PARTIALLY DAMAGED.\nINITIATE REPAIR PROCESS.\nMUSCULOSKELETAL ASSISTANT FRAME...FULLY FUNCTIONAL",
                    "HIGH CONTAMINATION LEVEL DECTECTED...\n'TENEBRAE' LEVEL DANGEROUSLY HIGH",
                    "LANTERN SYSTEM OFFLINE\nREBOOTING...25%...50%...84%...100%.",
                    "L-07, This is L.U.M.O.S, your Assistant AI for this operation.\nYour mission is to enter Sector 13 and retrieve the HyperCell",
                    "Good luck, L-07."
                },
                //대사1 (튜토리얼 씬 중앙의 컴퓨터 대사)
                new string[]
                {
                    "LANTERN Test Log.37",
                    "Stablizing the core has finally succeeded.\nProcess of recharging using TENBRAE as scource is ongoing.\nNeed more tests."
                },
                //대사2 (튜토리얼 씬의 절대 열리지 않는 장식용 문)
                new string[]
                {
                    //Page 1
                    "The door is locked tight."
                },
                //대사3 (스테이지 1의 테스트 기어발견 & 기어 튜토리얼겸)
                new string[]
                {
                    //1페이지
                    "An old equiptable assistant gear... It will help you on your mission",
                    //2페이지
                    "Gears can be equipped in the Gear tab of your inventory.\nWarning: You cannot exceed the Gear Install Capacity. Select wisely."
                },
                //대사4 (lightAppear 튜토리얼)
                new string[]
                {
                    "A 'BiFrost'. A machine designed to only be activated using LUMEN.\nThe LUMEN from the Lantern should suffice",
                    "Using the BiFrost, you will be able to move much easier."
                },
                //대사5 (DarkVanish 튜토리얼)
                new string[]
                {
                    "Halt. TENEBRAE Level is off the charts.\n Status. Limited Vision and Unable to Move Forward.",
                    "Use the Lantern to purify the TENEBRAE.",
                    "Remember. In the midst of darkness, light persists."
                },
                //대사6 기어 강화 튜토리얼
                new string[]
                {
                    "I detect a still working machine. Inspection is advised.\nIt's a Gear Amplifier Machine. I am still surpized that it works given the state of it.",
                    "It will be done if you have the materials needed.\nI expect it will help our mission greatly."
                },
                //대사7 Log1
                new string[]
                {
                    "Damn it. Even the machines are affected by TENEBRAE...\nI can't feel my legs.",
                    "I don't wanna die. I don't wanna die",
                    "Idon'twannadieIdon'twannadieIdon'twannadieIdon'twannadieIdon'twannadieIdon'twannadieIdon'twannadieIdon'twannadie",
                    "I...don'...t...wa...nna..."
                },
                //대사8 Log2
                new string[]
                {
                    "Every time I look at the darkness and around me, I wonder.\nIs this divine judgement?",
                    "Is this the price of trying to build the Tower of Bable?\nAll we wanted...was to rediscover the technology lost in time.",
                    "Damn it...the infection is not being healed.\nI'm so tired...Maybe...A quick nap...might...help..."
                },
                //대사9 Log3
                new string[]
                {
                    "몬스트라레 연구 일지.13",
                    "하늘에 도전한 대가를 받는 것일까?\n그저 인류를 위해...잃어버린 기술을 다시 발굴하고 싶었던 것 뿐이었는데.",
                    "젠장...감염된 상처가 낳지를 않는구만.\n피곤해...잠깐이라면...눈을...감아도..."
                },
                //대사10 세이브포인트
                new string[]
                {
                    "It's a Data Save Module\nYou will be able to refill your Core Energy Package and Lantern.",
                    "I recommend you activate it everytime you find it since it's an important device."
                },
                
            };
        }
        // 한국어
        else if (SettingManager.I.setting.locale == 1)
        {
            allDialogTexts = new List<string[]>()
            {
                //대사0 (게임시작 (이 대사 이후 바로 튜토리얼 시작))
                new string[]
                {
                    "[시스템 시작 활성화]\n코어 온도 안정화 중…완료\n신경 반응 회로…연결 확인",
                    "전투 모듈 부분 손상\n복구 프로세스 진행.\n근골격 보조 프레임… 정상 가동",
                    "외부 오염 감지...\n'테네브레' 수치 매우 위험.",
                    "랜턴 시스템 오프라인\n재부팅 중. 25%...50%...84%...100%.",
                    "전 시스템 온라인. 이상 무.\n프로젝트 'GEPPETTO' 성공적으로 가동 완료",
                    "오퍼레이션 REKINDLE. 가동 시작.",
                    "반갑습니다, P-07. 보조 AI 'L.U.M.O.S'입니다.\n당신의 임무는 섹터 13에서 하이퍼셀을 회수해오는 것 입니다.",
                    "그럼, 행운을 빕니다. P-07."
                },
                //대사1  (튜토리얼 씬 중앙의 컴퓨터 대사)
                new string[]
                {
                    "랜턴 테스트 로그.37",
                    "코어의 안정화에 성공했다.\n테네브레를 활용한 충전은 현재 개발 중.\n더 많은 실험이 필요하다."
                },
                //대사2 (튜토리얼 씬의 절대 열리지 않는 장식용 문)
                new string[]
                {
                    //Page 1
                    "문이 단단히 잠겨있습니다.",
                },
                //대사3 (스테이지 1의 테스트 기어발견 & & 기어 튜토리얼겸)
                new string[]
                {
                    //1페이지
                    "오래된 보조 장착 기어군요. 장착하면 도움이 될 것 같습니다.",
                    //2페이지
                    "인벤토리 내 기어 탭을 통해 장착이 가능합니다.\n기어는 기어 장착 용량을 초과할 수 없습니다. 현명하게 장착해야 합니다."
                },
                //대사4 (lightAppear 튜토리얼)
                new string[]
                {
                    "'비프로스트'군요. 루멘에 반응하도록 설계된 장치입니다.\n루멘에 반응하니 랜턴을 활용해 재가동하면 될겁니다",
                    "비프로스트를 활용해 막힌 길도 이동할 수 있겠군요."
                },
                //대사5 (DarkVanish 튜토리얼)
                new string[]
                {
                    "정지. 테네브레 수치가 매우 높습니다.\n시아도 제한되고, 앞으로 이동할 수 없군요",
                    "랜턴을 활용해 테네브레를 정화하세요.",
                    "명심하세요. 아무리 깊은 어둠 속에서도, 빛은 사라지지 않습니다."
                },
                //대사6 기어 강화 튜토리얼
                new string[]
                {
                    "작동 가능 기계가 감지됩니다. 확인 요망합니다.\n기어 강화 기계군요. 상태를 보아하니 아직도 작동하는 것이 있다니 놀랍습니다",
                    "재료들만 있다면 기어를 강화할 수 있겠군요.\n임무 수행에 큰 도움이 될 것으로 예상됩니다."
                },
                //대사7 Log1
                new string[]
                {
                    "젠장. 테네브레가 기계에도 영향을 미치다니...\n다리가 느껴지지 않아.",
                    "죽고 싶지 않아. 죽고 싶지 않아",
                    "죽고싶지않아죽고싶지않아죽고싶지않아죽고싶지않아죽고싶지않아죽고싶지않아죽고싶지않아죽고싶지않아",
                    "죽고...싶...지...않..."
                },
                //대사8 Log2
                new string[]
                {
                    "짙은 어둠과 이 암울한 현실을 볼 때마다 생각한다.\n이것은 신의 형벌일까?",
                    "하늘에 도전한 대가를 받는 것일까?\n그저 인류를 위해...잃어버린 기술을 다시 발굴하고 싶었던 것 뿐이었는데.",
                    "젠장...감염된 상처가 낳지를 않는구만.\n피곤해...잠깐이라면...눈을...감아도..."
                },
                //대사9 Log3
                new string[]
                {
                    "몬스트라레 연구 일지.13",
                    "하늘에 도전한 대가를 받는 것일까?\n그저 인류를 위해...잃어버린 기술을 다시 발굴하고 싶었던 것 뿐이었는데.",
                    "젠장...감염된 상처가 낳지를 않는구만.\n피곤해...잠깐이라면...눈을...감아도..."
                },
                //대사10 세이브포인트
                new string[]
                {
                    "데이터 세이브 모듈이군요\n코어 에너지 패키지와 랜턴 게이지를 충전할 수 있겠군요.",
                    "중요한 시설이니 발견할 때마다 작동시키는 것을 권장드립니다."
                },
                //대사11 랜턴 충전 라이트
                new string[]
                {
                    "미약하지만 루멘의 흔적이 느껴지는군요\n활성화시킨다면 소량이지만 랜턴을 충전시킬 수 있을 것으로 보입니다.\n랜턴 게이지가 매우 부족할 때 유용하겠군요."
                },
            };
        }
    }


    [SerializeField] private InputActionReference nextPageAction1;
    [SerializeField] private InputActionReference nextPageAction2;
    [SerializeField] private InputActionReference nextPageAction3;
    GameObject canvasObject;
    TMP_Text contentText;
    PlayerControl playerControl;
    Image triangle;
    private int currentDialogIndex = -1;
    private int currentPageIndex = 0;
    private Coroutine typingCoroutine;
    private enum DialogState
    {
        ReadyForAdvance = 0,
        TypingSlow = 1,
        TypingNormal = 2,
        TypingFast = 3,
        TypingComplete = 4
    }
    private DialogState currentState = DialogState.ReadyForAdvance;
    float slowTypingSpeed = 0.13f; // 기본 속도 (입력 0)
    float normalTypingSpeed = 0.08f; // 기본 속도 (입력 1)
    float fastTypingSpeed = 0.04f; // 빠른 속도 (입력 2)
    void Awake()
    {
        playerControl = FindAnyObjectByType<PlayerControl>();
        canvasObject = transform.GetChild(0).gameObject;
        contentText = transform.GetComponentInChildren<TMP_Text>(true);
        canvasObject.SetActive(false);
        triangle = transform.GetChild(0).Find("Wrap/Triangle").GetComponent<Image>();
    }
    void OnEnable()
    {
        nextPageAction1.action.performed += InputButton;
        nextPageAction2.action.performed += InputButton;
        nextPageAction3.action.performed += InputButton;
        GameManager.I.onDialog += DialogTriggerHandler;
        Close(false);
    }
    void OnDisable()
    {
        nextPageAction1.action.performed -= InputButton;
        nextPageAction2.action.performed -= InputButton;
        nextPageAction3.action.performed -= InputButton;
        GameManager.I.onDialog -= DialogTriggerHandler;
        StopCoroutine(nameof(SometimesGlitchTextLoop));
        tweenTriangle?.Kill();
        triangle.gameObject.SetActive(false);
        Close(false);
    }
    IEnumerator SometimesGlitchTextLoop()
    {
        Transform parent = canvasObject.transform;
        yield return YieldInstructionCache.WaitForSeconds(0.9f);
        TMP_Text[] texts1 = parent.GetComponentsInChildren<TMP_Text>();
        Text[] texts2 = parent.GetComponentsInChildren<Text>();
        while (true)
        {
            yield return null;
            yield return null;
            yield return null;
            if (Random.value < 0.3f)
            {
                int rnd = Random.Range(0, texts1.Length + texts2.Length);
                if (rnd >= texts1.Length)
                {
                    if (texts2.Length <= rnd - texts1.Length) continue;
                    Text text2 = texts2[rnd - texts1.Length];
                    if (text2 == null) continue;
                    if (!text2.gameObject.activeInHierarchy) continue;
                    if (text2.transform.name == "EmptyText") continue;
                    GameManager.I.GlitchPartialText(text2, 3, 0.16f);
                    if (Random.value < 0.73f)
                        AudioManager.I.PlaySFX("Glitch1");
                }
                else
                {
                    if (texts1.Length <= rnd) continue;
                    TMP_Text text1 = texts1[rnd];
                    if (text1 == null) continue;
                    if (!text1.gameObject.activeInHierarchy) continue;
                    if (text1.transform.name == "EmptyText") continue;
                    GameManager.I.GlitchPartialText(text1, 3, 0.16f);
                    if (Random.value < 0.73f)
                        AudioManager.I.PlaySFX("Glitch1");
                }
            }
            yield return YieldInstructionCache.WaitForSeconds(Random.Range(0.2f, 1.5f));
            if (!canvasObject.activeInHierarchy) yield break;
        }
    }
    void DialogTriggerHandler(int index, Transform sender)
    {
        Open(index);
    }
    void InputButton(InputAction.CallbackContext callbackContext)
    {
        if (!GameManager.I.isOpenDialog || currentDialogIndex == -1) return;
        if (currentDialogIndex >= allDialogTexts.Count) return;
        int numPages = allDialogTexts[currentDialogIndex].Length;
        switch (currentState)
        {
            case DialogState.TypingSlow:
                currentState = DialogState.TypingFast;
                break;

            case DialogState.TypingNormal:
                currentState = DialogState.TypingNormal;
                break;

            case DialogState.TypingFast:
                SkipTyping();
                break;

            case DialogState.TypingComplete:
                // 3단계: 텍스트 완성됨 -> 다음 페이지로 이동 또는 종료
                if (currentPageIndex >= numPages - 1)
                {
                    Close(); // 마지막 페이지: 대화 종료
                }
                else
                {
                    AudioManager.I.PlaySFX("UIClick2");
                    NextPage(); // 다음 페이지: 타이핑 재시작 (TypingSlow 상태로 자동 전환)
                }
                break;

            case DialogState.ReadyForAdvance:
                break;
        }
    }
    public void NextPage()
    {
        currentPageIndex++;
        // 타이핑 시작
        string nextText = allDialogTexts[currentDialogIndex][currentPageIndex];
        StartTyping(nextText);
        RectTransform rt = canvasObject.transform.GetChild(0) as RectTransform;
        DOTween.Kill(canvasObject.transform.GetChild(0));
        DOTween.Kill(rt);
        canvasObject.transform.GetChild(0).localScale = 0.8f * Vector3.one;
        canvasObject.transform.GetChild(0).DOScale(1f, 0.1f).SetEase(Ease.OutBack).SetLink(gameObject);
        rt.sizeDelta = new Vector2(800, 160);
        rt.DOSizeDelta(new Vector2(800, 200), 0.4f).SetEase(Ease.OutQuad);
    }
    public void Open(int index)
    {
        InitLocale();
        if (index < 0 || index >= allDialogTexts.Count)
        {
            Debug.LogError($"유효하지 않은 대화 인덱스: {index}");
            return;
        }
        playerControl.rb.linearVelocity = 0.1f * playerControl.rb.linearVelocity;
        currentDialogIndex = index;
        currentPageIndex = 0;
        // --- 연출 ---
        RectTransform rt = canvasObject.transform.GetChild(0) as RectTransform;
        if (!GameManager.I.isOpenDialog)
        {
            //최초 켜지는 연출
            canvasObject.SetActive(true);
            DOTween.Kill(canvasObject.transform.GetChild(0));
            DOTween.Kill(rt);
            canvasObject.transform.GetChild(0).localScale = 0.5f * Vector3.one;
            canvasObject.transform.GetChild(0).DOScale(1f, 0.6f).SetEase(Ease.OutBack).SetLink(gameObject).SetLink(gameObject);
            rt.sizeDelta = new Vector2(800, 80);
            rt.DOSizeDelta(new Vector2(800, 200), 1.4f).SetEase(Ease.OutQuad);
        }
        else
        {
            //대사창 교체 연출
            DOTween.Kill(canvasObject.transform.GetChild(0));
            DOTween.Kill(rt);
            canvasObject.transform.GetChild(0).localScale = 0.6f * Vector3.one;
            canvasObject.transform.GetChild(0).DOScale(1f, 0.19f).SetEase(Ease.OutBack).SetLink(gameObject).SetLink(gameObject);
            rt.sizeDelta = new Vector2(800, 130);
            rt.DOSizeDelta(new Vector2(800, 200), 0.6f).SetEase(Ease.OutQuad);
        }
        AudioManager.I.PlaySFX("OpenPopup");
        string firstText = allDialogTexts[index][0];
        StartTyping(firstText);
        GameManager.I.isOpenDialog = true;
        if (playerControl != null && playerControl.fsm.currentState != playerControl.stop)
            playerControl.fsm.ChangeState(playerControl.stop);
    }
    public void Close(bool isSFX = true)
    {
        // 종료 전 타이핑 강제 완료
        if (typingCoroutine != null) SkipTyping(isSFX);
        // 닫는 연출 (Scale Out)
        canvasObject.transform.GetChild(0).DOScale(0f, 0.15f).SetEase(Ease.InSine).OnComplete(() =>
        {
            canvasObject.SetActive(false);
            GameManager.I.isOpenDialog = false;
        }).SetLink(gameObject);
        if (isSFX)
        {
            AudioManager.I.PlaySFX("UIClick");
        }
        currentDialogIndex = -1;
        currentPageIndex = 0;
        currentState = DialogState.ReadyForAdvance; // 상태 초기화
    }
    IEnumerator ShowTextCoroutine(string text)
    {
        triangle.gameObject.SetActive(false);
        currentState = DialogState.TypingSlow;
        contentText.text = text;
        contentText.maxVisibleCharacters = 0;
        for (int i = 0; i < text.Length; i++)
        {
            // TypingComplete 상태가 되면 즉시 중단
            if (currentState == DialogState.TypingComplete)
                break;
            // 상태에 따른 속도 결정
            float currentSpeed = 0.05f;
            switch (currentState)
            {
                case DialogState.TypingSlow:
                    currentSpeed = slowTypingSpeed;
                    break;
                case DialogState.TypingNormal:
                    currentSpeed = normalTypingSpeed;
                    break;
                case DialogState.TypingFast:
                    currentSpeed = fastTypingSpeed;
                    break;
            }
            contentText.maxVisibleCharacters = i + 1;
            if (currentState == DialogState.TypingSlow)
                AudioManager.I.PlaySFX("Tick1");
            else if (currentState == DialogState.TypingNormal && i % 2 == 0)
                AudioManager.I.PlaySFX("Tick1");
            else if (currentState == DialogState.TypingFast && i % 4 == 0)
                AudioManager.I.PlaySFX("Tick1");
            yield return new WaitForSeconds(currentSpeed);
        }
        // 자연스럽게 끝났거나, 중단 후 마지막 처리
        contentText.maxVisibleCharacters = int.MaxValue;
        currentState = DialogState.TypingComplete;
        typingCoroutine = null;
        triangle.gameObject.SetActive(true);
        tweenTriangle?.Kill();
        triangle.color = new Color(triangle.color.r, triangle.color.g, triangle.color.b, 0.2f);
        tweenTriangle = triangle.DOFade(1f, 0.15f).SetLink(gameObject).SetLoops(-1, LoopType.Yoyo).Play();
        StopCoroutine(nameof(SometimesGlitchTextLoop));
        StartCoroutine(nameof(SometimesGlitchTextLoop));
    }
    Tween tweenTriangle;
    private void SkipTyping(bool isSFX = true)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        if (isSFX)
        {
            AudioManager.I.PlaySFX("Tick1");
        }
        contentText.maxVisibleCharacters = int.MaxValue;
        currentState = DialogState.TypingComplete;
        triangle.gameObject.SetActive(true);
        tweenTriangle?.Kill();
        triangle.color = new Color(triangle.color.r, triangle.color.g, triangle.color.b, 0.2f);
        tweenTriangle = triangle.DOFade(1f, 0.15f).SetLink(gameObject).SetLoops(-1, LoopType.Yoyo).Play();
        StopCoroutine(nameof(SometimesGlitchTextLoop));
        StartCoroutine(nameof(SometimesGlitchTextLoop));
    }
    private void StartTyping(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(ShowTextCoroutine(text));
    }


#if UNITY_EDITOR
    [Header("Editor Test")]
    public int testIndex;
    [Button]
    public void TestOpen()
    {
        Open(testIndex);
    }
#endif
}