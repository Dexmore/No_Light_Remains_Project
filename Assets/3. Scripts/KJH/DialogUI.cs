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
                    "....\n...011101....\n.0x0010110",
                    "....11001100...\n..110111..\n..\n..Exiting hibernation mode.",
                    "..0x01111011...\n....System active."
                },
                //대사1 (튜토리얼 씬 중앙의 컴퓨터 대사)
                new string[]
                {
                    "0x01111011...\n(Scanning data logs...)",
                    "Based on this data... 'that phenomenon' is escalating.\nNo time to waste. I have to get to the 'Core' now."
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
                    "An old upgrade gear... Why is this here?",
                    //2페이지
                    "Gears can be equipped in the Gear tab of your inventory.\nWarning: Socket capacity varies by gear type."
                },
                //대사4
                new string[]
                {
                    //1페이지
                    "aaaaaaaaaa\nbbbbbbbbbbb\ncccccccc\nddddddddd",
                    //2페이지
                    "eeeeee\nfffffffff\nggggggggggg\nhhhhhhhhhhh",
                    //3페이지
                    "iiiiiiiii\njjjjjjjjjjjj\nkkkkkkkkkk\nlllllllll",
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
                    "....\n...0x011101....\n.0x0010110",
                    "....11001100...\n..0111..\n..\n..휴면상태 해제",
                    "..0x01111011...\n....시스템 가동"
                },
                //대사1  (튜토리얼 씬 중앙의 컴퓨터 대사)
                new string[]
                {
                    "0x01111011...\n(장치로부터 데이터 확인 중)",
                    "이 자료에 따르면... 최근 '그 현상'이 더 심화되고 있는 것으로 보인다.\n서둘러 '코어' 시설로 향해야한다."
                },
                //대사2 (튜토리얼 씬의 절대 열리지 않는 장식용 문)
                new string[]
                {
                    //Page 1
                    "문이 단단히 잠겨있다.",
                },
                //대사3 (스테이지 1의 테스트 기어발견 & & 기어 튜토리얼겸)
                new string[]
                {
                    //1페이지
                    "낡은 업그레이드 기어다. 왜 이런곳에 떨어져 있지?",
                    //2페이지
                    "인벤토리 내 기어 탭을 통해 장착이 가능합니다.\n단, 기어별로 요구하는 소켓 공간이 다르므로 주의해야 합니다."
                },
                //대사4
                new string[]
                {
                    "가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다",
                    "가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다",
                    "가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다",
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
        GameManager.I.onDialog += HandlerDialogTrigger;
        Close(false);
    }
    void OnDisable()
    {
        nextPageAction1.action.performed -= InputButton;
        nextPageAction2.action.performed -= InputButton;
        nextPageAction3.action.performed -= InputButton;
        GameManager.I.onDialog -= HandlerDialogTrigger;
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
    void HandlerDialogTrigger(int index, Transform sender)
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
        canvasObject.transform.GetChild(0).DOScale(1f, 0.1f).SetEase(Ease.OutBack);
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
            canvasObject.transform.GetChild(0).DOScale(1f, 0.6f).SetEase(Ease.OutBack);
            rt.sizeDelta = new Vector2(800, 80);
            rt.DOSizeDelta(new Vector2(800, 200), 1.4f).SetEase(Ease.OutQuad);
        }
        else
        {
            //대사창 교체 연출
            DOTween.Kill(canvasObject.transform.GetChild(0));
            DOTween.Kill(rt);
            canvasObject.transform.GetChild(0).localScale = 0.6f * Vector3.one;
            canvasObject.transform.GetChild(0).DOScale(1f, 0.19f).SetEase(Ease.OutBack);
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
        });
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
        tweenTriangle = triangle.DOFade(1f, 0.15f).SetLoops(-1, LoopType.Yoyo).Play();
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
        tweenTriangle = triangle.DOFade(1f, 0.15f).SetLoops(-1, LoopType.Yoyo).Play();
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