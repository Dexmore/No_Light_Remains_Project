using System.Collections.Generic;
using System.Collections;
using UnityEngine;
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
                //대사0
                new string[]
                {
                    //1페이지
                    "aaaaaaaaaa\nbbbbbbbbbbb",
                    //2페이지
                    "ccccccccc\nddddddddd\neeeeeeeee",
                    //3페이지
                    "fffffffffffffff"
                },
                //대사1
                new string[]
                {
                    "aassd"
                },
                //대사2
                new string[]
                {
                    "dialog2 page1"
                },
                //대사3
                new string[]
                {
                    "dialog3 page1 line1\ndialog 3-1 line2\ndialog 3-1 line3",
                    "dialog3 page2 line1\ndialog 3-1 line2"
                },
            };
        }
        // 한국어
        else if (SettingManager.I.setting.locale == 1)
        {
            allDialogTexts = new List<string[]>()
            {
                //대사0
                new string[]
                {
                    "도시 외곽 지역을 중심으로 어둠이 퍼지고 있다.\n어둠은 생명체와 기계들을 오염시켜서 점점 괴물로 만들고 있다.",
                    "나는 숨어서 오래 연구한 끝에 막대한 빛 에너지 '일리오스'를\n방출하는 장치를 만드는 데 성공했다. 이것만 있으면 도시를\n뒤덮은 괴물 '칼리고'들을 정화할 수 있을 것이다.",
                    "...슬슬 출발할 시간이 되었다. 움직여보자."
                },
                //대사1
                new string[]
                {
                    "가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다\n가나다라마바사아가나다라마바사아가나다"
                },
                //대사2
                new string[]
                {
                    "대사2의 1페이지입니다 (단일 페이지 예시)"
                },
                //대사3
                new string[]
                {
                    "대사3의 1페이지입니다...\n..2번째줄..\n.3번째줄.",
                    "대사3의 2페이지입니다...\n..2번째줄..\n.3번째줄.",
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
    private int currentDialogIndex = -1;
    private int currentPageIndex = 0;
    private Coroutine typingCoroutine;
    private enum DialogState
    {
        ReadyForAdvance = 0,
        TypingSlow = 1,
        TypingFast = 2,
        TypingComplete = 3
    }
    private DialogState currentState = DialogState.ReadyForAdvance;
    float slowTypingSpeed = 0.05f; // 기본 속도 (입력 0)
    float fastTypingSpeed = 0.01f; // 빠른 속도 (입력 1 이후)
    void Awake()
    {
        playerControl = FindAnyObjectByType<PlayerControl>();
        canvasObject = transform.GetChild(0).gameObject;
        contentText = transform.GetComponentInChildren<TMP_Text>(true);
        canvasObject.SetActive(false);
    }
    void OnEnable()
    {
        nextPageAction1.action.performed += InputButton;
        nextPageAction2.action.performed += InputButton;
        nextPageAction3.action.performed += InputButton;
        GameManager.I.onDialog += HandlerDialogTrigger;
    }
    void OnDisable()
    {
        nextPageAction1.action.performed -= InputButton;
        nextPageAction2.action.performed -= InputButton;
        nextPageAction3.action.performed -= InputButton;
        GameManager.I.onDialog -= HandlerDialogTrigger;
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
                // 1단계: 느린 타이핑 중 -> 빠른 타이핑으로 전환
                currentState = DialogState.TypingFast;
                break;

            case DialogState.TypingFast:
                // 2단계: 빠른 타이핑 중 -> 텍스트 완성
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
    public void Close()
    {
        // 종료 전 타이핑 강제 완료
        if (typingCoroutine != null) SkipTyping();
        // 닫는 연출 (Scale Out)
        canvasObject.transform.GetChild(0).DOScale(0f, 0.15f).SetEase(Ease.InSine).OnComplete(() =>
        {
            canvasObject.SetActive(false);
            GameManager.I.isOpenDialog = false;
        });
        AudioManager.I.PlaySFX("UIClick");
        currentDialogIndex = -1;
        currentPageIndex = 0;
        currentState = DialogState.ReadyForAdvance; // 상태 초기화
    }
    IEnumerator ShowTextCoroutine(string text)
    {
        currentState = DialogState.TypingSlow;
        contentText.text = text;
        contentText.maxVisibleCharacters = 0;
        for (int i = 0; i < text.Length; i++)
        {
            // TypingComplete 상태가 되면 즉시 중단
            if (currentState == DialogState.TypingComplete)
                break;
            // 상태에 따른 속도 결정
            float currentSpeed = (currentState == DialogState.TypingFast) ? fastTypingSpeed : slowTypingSpeed;
            contentText.maxVisibleCharacters = i + 1;
            if (currentState == DialogState.TypingSlow && i % 2 == 0)
                AudioManager.I.PlaySFX("Tick1");
            else if (currentState == DialogState.TypingFast && i % 5 == 0)
                AudioManager.I.PlaySFX("Tick1");
            yield return new WaitForSeconds(currentSpeed);
        }
        // 자연스럽게 끝났거나, 중단 후 마지막 처리
        contentText.maxVisibleCharacters = int.MaxValue;
        currentState = DialogState.TypingComplete;
        typingCoroutine = null;
    }
    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        contentText.maxVisibleCharacters = int.MaxValue;
        currentState = DialogState.TypingComplete;
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