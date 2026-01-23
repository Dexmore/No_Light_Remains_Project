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
                    "[SEQUENCE BOOTING INITIATED]\nCORE TEMPERATURE STABLEIZING...COMPLETE.\nNEURAL RESPONSE CORE CONNECTION...COMPLETE",
                    "COMBAT MODULE PARTIALLY DAMAGED.\nINITIATE REPAIR PROCESS.\nMUSCULOSKELETAL ASSISTANT FRAME...FULLY FUNCTIONAL",
                    "HIGH EXTERNAL CONTAMINATION LEVEL DECTECTED...\n'TENEBRAE' LEVEL: Critical",
                    "ALL SYSTEMS ONLINE. STATUS: READY TO GO.\nOperation REKINDLE: COMMENCING.",
                    "Greeting, P-07., This is L.U.M.O.S, your Assistant AI.\nSurface conditions are currently at a breaking point due to the 'Black Mist' incident.",
                    "Your mission is to cleanse the TENEBRAE erosion and retrieve the ASTRA Cell from Sector 13",
                    "Good luck, P-07."
                },
                //대사1 (튜토리얼 씬 중앙의 컴퓨터 대사)
                new string[]
                {
                    "LANTERN Test Log #37",
                    "Stablizing the core has finally succeeded.\nResearch regarding purifying TENBRAE into LUMEN is ongoing.\nNeed more tests."
                },
                //대사2 (튜토리얼 씬의 절대 열리지 않는 장식용 문)
                new string[]
                {
                    //Page 1
                    "Detecting a broken door.\nThere seems to be no way to open it.\nI advise you move on."
                },
                //대사3 (스테이지 1의 테스트 기어발견 & 기어 튜토리얼겸)
                new string[]
                {
                    "An old Auxiliary Gear Module... It will help you on your mission",
                    "Finding an active Gear Enhancement Machine allows you to upgrade your gear's capabilities.\nHowever, it is uncertain such machines currently remain functional.",
                    "Gears can be equipped in the Gear tab of your inventory.\nWarning: You cannot exceed the Gear Install Capacity.\n I advise you select wisely."
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
                    "Halt. TENEBRAE Level is off the charts.\nStatus. Limited Vision and Unable to Move Forward.",
                    "Use the Lantern to purify the TENEBRAE.",
                    "Remember. In the midst of darkness, light persists."
                },
                //대사6 기어 강화 튜토리얼
                new string[]
                {
                    "I detect a still working machine. Inspection is advised.\nIt's a Gear Amplifier Machine. I am still surpized that it works given the state of it.",
                    "You will be able to upgrade your gear's capabilities if you have the materials needed.\nI expect it will help your mission greatly."
                },
                //대사7 Log1(Stage2)
                new string[]
                {
                    "Damn it. Even the machines are corrupted by TENEBRAE...\nI can't feel my legs.",
                    "I can't die here. I don't wanna die",
                    "Idon'twannadieIdon'twannadieIdon'twannadieIdon'twannadieIdon'twannadieIdon'twannadieIdon'twannadieIdon'twannadie",
                    "I...don'...t...wa...nna..."
                },
                //대사8 Log2(Stage1)
                new string[]
                {
                    "Every time I look at the darkness and around me, I wonder.\nIs this divine judgement?",
                    "Is this the price of trying to build the Tower of Bable?\nAll we wanted...was to rediscover the technology lost in time.",
                    "Damn it...the infection from the wound is getting worse.\nI'm so tired...Maybe...A quick nap...might...help..."
                },
                //대사9 Log3(Stage3)
                new string[]
                {
                    "Monstrare Research Log #03",
                    "Energy readings from Monstrare are off the charts.\nWe may be looking at the ultimate energy substitute.",
                    "If SOLARIS Company is able to monopolize this…\nIt would allow the company to grow on an unprecedented scale."
                },
                //대사10 세이브포인트
                new string[]
                {
                    "It's a Data Save Module\nYou will be able to refill your Core Energy Package and Lantern.",
                    "I recommend you activate it everytime you find it since it's an important device."
                },
                //대사11 랜턴 충전 라이트
                new string[]
                {
                    "Faint traces of LUMEN detectect.\nIt's a miniaturized LUMEN amplifier.",
                    "This should help when the room is dark or when the Lantern is low on LUMEN.\nActivating it should charge your Lantern half full.",
                    "[SYSTEM] Some objects can be interacted with by 'pressing the Lantern key '."
                },
                //대사12 아폴리온 처치
                new string[]
                {
                    "Apollyon: Deactivation confirmed.\nThreat level was critical; however, combat efficiency was optimal. Excellent work."
                },
                //대사13 아스트라 셀 회수
                new string[]
                {
                    "Scanning...High-Level LUMEN signature detected.\nEstimated Astra Cell position reached.",
                    "......",                    
                    "Astra Cell successfully retrieved.\nRelocating to the uploaded coordinates. Awaiting your arrival."
                },
                //대사14 실험실
                new string[]
                {
                    "Welcome P-07, to the Deep Research Facility in Zion.\nThis unit serves as a stasis chamber for the last shard of the MONSTRARE.",
                    "To counter the Tenebrae outbreak after the 'Black Mist' Incident, Operation REKINDLE was launched",
                    "Awaiting Astra Cell integration...\n......",
                    "Integration verified.\n[Analyzing...] Core Restoration 27%...45%\n[Scanning...] TENEBRAE contamination in Zion, 78%...58%",
                    "TENEBRAE corruption has been decelerated. MONSTRARE core is stable.",
                    "Operation REKINDLE: Phase 1 marked as Success.\nYour contribution has been invaluable."
                },
                //대사15 Log4(Stage1)
                new string[]
                {
                    "Monstrare Research Log #107",
                    "We have a problem. A massive one.\nA mysterious substance known as 'Tenebrae' has been detected\nA substance toxic and corrupting organic life, causing severe genetic mutations.",
                    "In the beginning, it seemed insignificant\nIt would vanish almost as soon as it appeared.",
                    "But as the concentration levels rose, so did the danger.\nIt started on the outskirts of Zion; the Outer Rim.",
                    "But now, traces are being detected even in the heart of the city.",
                },
                //대사16 Log4
                new string[]
                {
                    "The outskirts are now crawling with those... things\nCan we even call them living beings anymore?",
                    "Regardless, casualties are mounting due to these unknown entities and the Tenebrae itself.",
                    "Outside, the world is swallowed by a thick, dark haze saturated with lethal Tenebrae levels.\nWe have come to call this catastrophe 'The Black Mist.'"
                },
                //대사17 상자 튜토리얼
                new string[]
                {
                    "It's an ARCA-05. It was used for storing supplies.\nIt might contain things that could come handy later."
                },
                //대사18 적 처음 감지
                new string[]
                {
                    "Beings corrupted by TENEBRAE...\nThe surface is currently saturated with these hostile lifeforms."
                },
                //대사19 랜턴 활성화
                new string[]
                {
                    "LANTERN SYSTEM OFFLINE\nREBOOTING...25%...50%...84%...100%.",
                    "LANTERN SYSTEM ONLINE.\nThe Light of ASTRUM: Operational status confirmed. Machine ready."
                },
                //대사20 로그4
                new string[]
                {
                    "It has been a month since the streets were overtaken by those black...things.\nI'm now almost out for things to eat.",
                    "I miss a warm stew...mom's were the best.\nShould have asked her for the recipe before I left her place.",
                    "Mom... I miss you so much.\nPlease, please be alive.\n...Please."
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
                    "[시스템 시작 활성화]\n코어 온도 안정화 중...완료\n신경 반응 회로...연결 확인",
                    "전투 모듈 부분 손상\n복구 프로세스 진행.\n근골격 보조 프레임...정상 가동",
                    "전 시스템 온라인. 이상 무.\n오퍼레이션 REKINDLE. 가동 시작.",
                    "반갑습니다 P-07, 보조 AI 'L.U.M.O.S'입니다.\n현재 지상은 '검은 안개' 사태로 인해 절체절명의 위기입니다.\n당신의 임무는 테네브레 침식 정화 및 섹터 13에서 아스트라 셀을\n회수해 오는 것입니다.",
                    "그럼, 행운을 빕니다. P-07."
                },
                //대사1  (튜토리얼 씬 중앙의 컴퓨터 대사)
                new string[]
                {
                    "랜턴 테스트 로그 #37",
                    "코어의 안정화에 성공했다.\n테네브레를 루멘으로 변환시키는 기술은 현재 개발 중이다.\n더 많은 실험이 필요할 것으로 예상된다."
                },
                //대사2 (튜토리얼 씬의 절대 열리지 않는 장식용 문)
                new string[]
                {
                    //Page 1
                    "망가진 문이군요.\n열 방법은 없어 보입니다.\n이동하시는 걸 추천합니다.",
                },
                //대사3 (스테이지 1의 테스트 기어발견 & & 기어 튜토리얼겸)
                new string[]
                {
                    //1페이지
                    "오래된 보조 장착 기어군요. 장착하면 도움이 될 것 같습니다.",
                    "작동하는 기어 강화 기계를 발견한다는 기어의\n성능을 강화할 수 있습니다.\n아직 작동하는게 있을지는 미지수군요.",
                    //2페이지
                    "인벤토리 내 기어 탭을 통해 장착이 가능합니다.\n기어는 기어 장착 용량을 초과할 수 없습니다.\n현명하게 장착해야 합니다."
                },
                //대사4 (lightAppear 튜토리얼)
                new string[]
                {
                    "'비프로스트' 감지. 루멘에 반응하도록 설계된 장치입니다.\n루멘에 반응하니 랜턴을 활용해 가동하면 될겁니다",
                    "비프로스트를 활용해 막힌 길도 이동할 수 있겠군요."
                },
                //대사5 (DarkVanish 튜토리얼)
                new string[]
                {
                    "정지. 테네브레 수치가 매우 높습니다.\n시야도 제한되고, 앞으로 이동할 수 없군요",
                    "랜턴을 활용해 테네브레를 정화하세요.",
                    "명심하세요. 아무리 깊은 어둠 속에서도, 빛은 사라지지 않습니다."
                },
                //대사6 기어 강화 튜토리얼
                new string[]
                {
                    "작동 가능 기계가 감지됩니다. 확인 요망합니다.\n기어 강화 기계군요.\n상태를 보아하니 아직도 작동하는 것이 있다니 놀랍습니다",
                    "재료들만 있다면 기어를 강화할 수 있겠군요.\n임무 수행에 큰 도움이 될 것으로 예상됩니다."
                },
                //대사7 Log1
                new string[]
                {
                    "젠장. 테네브레가 기계에도 영향을 미치다니...\n다리가 느껴지지 않아.",
                    "여기서 죽을 수는 없어. 죽고 싶지 않아",
                    "죽고싶지않아죽고싶지않아죽고싶지않아죽고싶지않아죽고싶지않아죽고싶지않아죽고싶지않아죽고싶지않아",
                    "죽고...싶...지...않..."
                },
                //대사8 Log2
                new string[]
                {
                    "짙은 어둠과 이 암울한 현실을 볼 때마다 생각한다.\n이것은 신의 형벌일까?",
                    "하늘에 도전한 대가를 받는 것일까?\n그저 인류를 위해...잃어버린 기술을 다시 발굴하고\n싶었던 것 뿐이었는데.",
                    "젠장...감염된 상처가 낳지를 않는구만.\n피곤해...잠깐이라면...눈을...감아도..."
                },
                //대사9 Log3
                new string[]
                {
                    "몬스트라레 연구 일지 #03",
                    "몬스트라레에서 확인되는 에너지량이 가히 상상을 초월한다.\n어쩌면 우리는 모든 에너지를 대체할 수 있는 새로운\n물질을 발견한 것이 아닐까 싶다.",
                    "이 기술만 독점할 수 있다면…\n솔라리온 컴퍼니는 지금과는 상상을 초월하는 규모로 성장할 수 있을지도 모르겠다."
                },
                //대사10 세이브포인트
                new string[]
                {
                    "데이터 세이브 모듈이군요\n코어 에너지 패키지와 랜턴 게이지를 충전할 수 있겠군요.",
                    "중요한 장치이니 발견할 때마다 작동시키는 것을 권장드립니다."
                },
                //대사11 랜턴 충전 라이트
                new string[]
                {
                    "미약하지만 루멘의 흔적이 느껴지는군요\n활성화시킨다면 소량이지만 랜턴을 충전시킬 수 \n있을 것으로 보입니다.\n랜턴 게이지가 매우 부족할 때 유용하겠군요.",
                    "[시스템] 랜턴 키를 길게 누르고 있으면 상호작용이 가능합니다."
                },
                //대사12 아폴리온 처치
                new string[]
                {
                    "아폴리온 기능 정지 확인.\n수고하셨습니다. 까다로운 상대였지만 잘하셨습니다."
                },
                //대사13 아스트라 셀 회수
                new string[]
                {
                    "고동노의 루멘이 관측됩니다.\n아스트라 셀은 이곳에 있을 것으로 예상됩니다.",
                    "......",
                    "아스트라 셀을 확보 완료.\n전송한 좌표로 이동하시길 바랍니다."
                },
                //대사14 실험실
                new string[]
                {
                    "자이온의 심층 연구실에 환영합니다.\n당신 앞에 있는 기계는 남아있는 몬스트라레를 보존시키는 장치입니다.",
                    "'검은 안개' 사태 이후 자이온의 테네브레 침식 저지 및 몬스트라레 복구를 위해 오퍼레이션 REKINDLE이 기획되고 1차 작전 실행이 성공적으로 완료되었습니다.",
                    "아스트라 셀을 기계에 장착해주시길 바랍니다.\n......\n......",
                    "아스트라 셀 장착 확인.\n몬스트라레 코어 복구율 27%...45%.\n자이온 내 테네브레 수치 78%...58%",
                    "자이온 내 테네브레 침식 지연 및 몬스트라레 일부 복구 성공.",
                    "제 1차 오퍼레이션 REKINDLE, 성공적으로 완료되었습니다.\n노고에 감사합니다."
                },
                //대사15 Log4
                new string[]
                {
                    "몬스트라레 연구 일지 #107",
                    "문제가 발생했다. 그것도 대형 문제가.\n유기체에게 해롭고 침식해 유전적 돌연변이를 발생시키는\n미지의 물질 '테네브레'가 관측되고 있다.",
                    "처음에는 별거 아니었다. 생성돼도 금방 사라졌으니까.\n문제는 수치가 높아질수록 문제였다. 처음은 자이온 외각이었다.\n이제는 자이온 중심부에서도 소량이지만 관측되고 있다.",
                    "죽음이...눈앞까지 다가오고 있다."
                },
                //대사16 Log4
                new string[]
                {
                    "현재 자이온 외각에는 테네브레에 침식된...\n저것들을 생명체라고 불러도 되는 걸까?\n어쨌든 미지의 존재들과 테네브레 자체로 인해\n인명 피해가 증가하고 있다.",
                    "현재 밖은 높은 테네브레 수치가 관측되는\n검정색 안개로 가득하다.\n우리는 이 사태를 '검은 안개'라고 부르고 있다."
                },
                //대사17 상자 튜토리얼
                new string[]
                {
                    "아르카-05군요. 보급을 저장하기 위해 사용된 상자입니다.\n차후 도움이 될 물건들이 담겨있을 수도 있겠군요."
                },
                //대사18 적 처음 감지
                new string[]
                {
                    "테네브레에 침식된 존재입니다.\n지상은 현재 이러한 존재들로 가득합니다."
                },
                //대사19 랜턴 활성화
                new string[]
                {
                    "랜턴 시스템 오프라인 확인.\n재부팅 중. 25%...50%...84%...100%.",
                    "랜턴 시스템 온라인.\n아스트룸의 불빛: 가동 상태 정상. 장비 활성화가 가능합니다."
                },
                //대사20 로그4
                new string[]
                {
                    "도로가 저 검은 무언가들에 점령당하고 1달.\n더 이상 먹을 것도 떨어지고 있다.",
                    "집밥 먹고 싶다.\n엄마가 만들어준...따뜻한 스튜...\n그때 어떻게 만드는지 여쭤볼걸.",
                    "엄마...보고 싶어\n제발 살아 있어 줘요..."
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
    private Coroutine glitchCoroutine;

    private int currentDialogIndex = -1;
    private int currentPageIndex = 0;
    private Coroutine typingCoroutine;

    // 중복 입력 방지를 위한 쿨타임 변수
    private float lastInputTime = 0f;
    private const float inputCooldown = 0.26f;

    private enum DialogState
    {
        ReadyForAdvance = 0, // 페이지 전환 준비/진행 중 (입력 무시)
        TypingSlow = 1,
        TypingNormal = 2,
        TypingFast = 3,
        TypingComplete = 4
    }

    private DialogState currentState = DialogState.ReadyForAdvance;

    float slowTypingSpeed = 0.13f;
    float normalTypingSpeed = 0.08f;
    float fastTypingSpeed = 0.04f;
    Tween tweenTriangle;

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
        StopAllCoroutines();
        tweenTriangle?.Kill();
        triangle.gameObject.SetActive(false);
        Close(false);
    }

    IEnumerator SometimesGlitchTextLoop()
    {
        // 코루틴 시작 시점의 원본 대사를 저장해둡니다.
        string originalText = contentText.text;
        Transform parent = canvasObject.transform;

        // 첫 글리치 시작 전 대기
        yield return YieldInstructionCache.WaitForSeconds(0.9f);

        TMP_Text[] texts1 = parent.GetComponentsInChildren<TMP_Text>();
        Text[] texts2 = parent.GetComponentsInChildren<Text>();

        while (true)
        {
            // [핵심 가드] 만약 페이지가 넘어가서 contentText의 내용이 바뀌었다면 이 루프를 즉시 종료합니다.
            if (contentText.text != originalText)
            {
                glitchCoroutine = null;
                yield break;
            }

            if (Random.value < 0.3f)
            {
                int rnd = Random.Range(0, texts1.Length + texts2.Length);
                if (rnd >= texts1.Length)
                {
                    int idx = rnd - texts1.Length;
                    if (idx < texts2.Length && texts2[idx] != null && texts2[idx].gameObject.activeInHierarchy && texts2[idx].name != "EmptyText")
                    {
                        GameManager.I.GlitchPartialText(texts2[idx], 3, 0.16f);
                        if (Random.value < 0.73f) AudioManager.I.PlaySFX("Glitch1");
                    }
                }
                else
                {
                    if (rnd < texts1.Length && texts1[rnd] != null && texts1[rnd].gameObject.activeInHierarchy && texts1[rnd].name != "EmptyText")
                    {
                        // 메인 대사창(contentText)에 글리치를 먹일 때, 원본을 해치지 않도록 주의해야 합니다.
                        GameManager.I.GlitchPartialText(texts1[rnd], 3, 0.16f);
                        if (Random.value < 0.73f) AudioManager.I.PlaySFX("Glitch1");
                    }
                }
            }

            yield return YieldInstructionCache.WaitForSeconds(Random.Range(0.2f, 1.5f));

            // UI가 꺼졌다면 종료
            if (!canvasObject.activeInHierarchy)
            {
                glitchCoroutine = null;
                yield break;
            }
        }
    }

    void DialogTriggerHandler(int index, Transform sender)
    {
        Open(index);
    }


    void InputButton(InputAction.CallbackContext callbackContext)
    {
        // 1. performed 상태일 때만 실행 (누르는 순간만 체크)
        if (!callbackContext.performed) return;

        // 2. 대화창이 꺼져있거나 인덱스가 비정상일 때 무시
        if (!GameManager.I.isOpenDialog || currentDialogIndex == -1) return;

        // 3. 입력 쿨타임 체크 (실제 시간 기준으로 중복 클릭 방지)
        if (Time.realtimeSinceStartup - lastInputTime < inputCooldown) return;

        // 인덱스 범위 초과 방지 가드
        if (currentDialogIndex >= allDialogTexts.Count) return;
        int numPages = allDialogTexts[currentDialogIndex].Length;

        switch (currentState)
        {
            case DialogState.TypingSlow:
                // 타이핑 속도를 빠르게 변경
                currentState = DialogState.TypingFast;
                lastInputTime = Time.realtimeSinceStartup; // 입력 시점 기록
                break;

            case DialogState.TypingFast:
                // 타이핑 연출 생략하고 즉시 전체 출력
                SkipTyping();
                lastInputTime = Time.realtimeSinceStartup; // 입력 시점 기록
                break;

            case DialogState.TypingComplete:
                // 모든 글자가 출력된 상태에서 버튼을 눌렀을 때
                if (currentPageIndex >= numPages - 1)
                {
                    // 마지막 페이지라면 닫기
                    Close();
                }
                else
                {
                    // [수정 핵심] 다음 페이지로 넘어가기 전, 상태를 즉시 '전환 중'으로 변경
                    // 이렇게 하면 다음 프레임에서 중복 입력이 들어와도 이 switch문에 걸리지 않습니다.
                    currentState = DialogState.ReadyForAdvance;

                    // 쿨타임을 한 번 더 갱신하여 연타 방지
                    lastInputTime = Time.realtimeSinceStartup;

                    AudioManager.I.PlaySFX("UIClick2");
                    NextPage();
                }
                break;

            case DialogState.ReadyForAdvance:
                // 페이지 전환 애니메이션 중에는 아무 로직도 타지 않음 (중복 방지)
                break;
        }
    }

    public void NextPage()
    {
        // [보안] 현재 다이얼로그가 유효하지 않으면 중단
        if (currentDialogIndex == -1) return;

        int numPages = allDialogTexts[currentDialogIndex].Length;

        // [중요] 다음 페이지가 있는지 먼저 확인
        if (currentPageIndex < numPages - 1)
        {
            // 1. 인덱스를 먼저 안전하게 증가
            currentPageIndex++;

            // 2. 상태를 확실하게 초기화 (InputButton에서도 하지만 이중 잠금)
            currentState = DialogState.ReadyForAdvance;

            // 3. 증가된 인덱스의 텍스트를 가져와서 타이핑 시작
            string nextText = allDialogTexts[currentDialogIndex][currentPageIndex];
            StartTyping(nextText);

            // --- UI 연출 부분 ---
            // 기존 실행 중인 연출이 있다면 중단 (중복 방지)
            RectTransform rt = canvasObject.transform.GetChild(0) as RectTransform;
            canvasObject.transform.GetChild(0).DOKill();
            rt.DOKill();

            // 약간 작아졌다가 커지는 탄성 효과
            canvasObject.transform.GetChild(0).localScale = 0.8f * Vector3.one;
            canvasObject.transform.GetChild(0).DOScale(1f, 0.1f).SetEase(Ease.OutBack).SetLink(gameObject);

            // 대사창 크기 조절 (높이가 약간 늘어나는 연출)
            rt.sizeDelta = new Vector2(800, 160);
            rt.DOSizeDelta(new Vector2(800, 200), 0.4f).SetEase(Ease.OutQuad).SetLink(gameObject);
        }
        else
        {
            // 더 이상 페이지가 없는데 호출되었다면 닫기 처리
            Close();
        }
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

        RectTransform rt = canvasObject.transform.GetChild(0) as RectTransform;
        canvasObject.transform.GetChild(0).DOKill();
        rt.DOKill();

        if (!GameManager.I.isOpenDialog)
        {
            canvasObject.SetActive(true);
            canvasObject.transform.GetChild(0).localScale = 0.5f * Vector3.one;
            canvasObject.transform.GetChild(0).DOScale(1f, 0.6f).SetEase(Ease.OutBack).SetLink(gameObject);
            rt.sizeDelta = new Vector2(800, 80);
            rt.DOSizeDelta(new Vector2(800, 200), 1.4f).SetEase(Ease.OutQuad).SetLink(gameObject);
        }
        else
        {
            canvasObject.transform.GetChild(0).localScale = 0.6f * Vector3.one;
            canvasObject.transform.GetChild(0).DOScale(1f, 0.19f).SetEase(Ease.OutBack).SetLink(gameObject);
            rt.sizeDelta = new Vector2(800, 130);
            rt.DOSizeDelta(new Vector2(800, 200), 0.6f).SetEase(Ease.OutQuad).SetLink(gameObject);
        }

        AudioManager.I.PlaySFX("OpenPopup");
        StartTyping(allDialogTexts[index][0]);
        GameManager.I.isOpenDialog = true;

        if (playerControl != null && playerControl.fsm.currentState != playerControl.stop)
            playerControl.fsm.ChangeState(playerControl.stop);
    }

    public void Close(bool isSFX = true)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);

        canvasObject.transform.GetChild(0).DOKill();
        canvasObject.transform.GetChild(0).DOScale(0f, 0.15f).SetEase(Ease.InSine).OnComplete(() =>
        {
            canvasObject.SetActive(false);
            GameManager.I.isOpenDialog = false;
        }).SetLink(gameObject);

        if (isSFX) AudioManager.I.PlaySFX("UIClick");

        currentDialogIndex = -1;
        currentPageIndex = 0;
        currentState = DialogState.ReadyForAdvance;
    }

    IEnumerator ShowTextCoroutine(string text)
    {
        // 1. 초기화: 시작하자마자 텍스트를 비우고 상태를 고정
        triangle.gameObject.SetActive(false);
        currentState = DialogState.ReadyForAdvance; // 애니메이션 도중 입력 방지

        contentText.text = text;
        contentText.maxVisibleCharacters = 0;
        contentText.ForceMeshUpdate();

        // 2. [핵심] UI 애니메이션 대기
        // Open 시 SizeDelta 애니메이션이 1.4초이므로, 최소 0.5초~1초는 기다려야 창이 커진게 보입니다.
        // 첫 페이지(currentPageIndex == 0)일 때만 조금 더 기다려줍니다.
        if (currentPageIndex == 0)
        {
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // 다음 페이지일 때는 연출이 짧으므로(0.4s) 짧게 대기
            yield return new WaitForSeconds(0.1f);
        }

        // 타이핑 시작 상태로 변경
        currentState = DialogState.TypingSlow;
        int totalVisibleCharacters = contentText.textInfo.characterCount;

        if (totalVisibleCharacters == 0)
        {
            CompleteTypingDisplay();
            yield break;
        }

        // 3. 타이핑 루프
        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            if (currentState == DialogState.TypingComplete) break;

            float currentSpeed = slowTypingSpeed;
            switch (currentState)
            {
                case DialogState.TypingNormal: currentSpeed = normalTypingSpeed; break;
                case DialogState.TypingFast: currentSpeed = fastTypingSpeed; break;
            }

            contentText.maxVisibleCharacters = i;

            // 사운드 재생
            if (currentState == DialogState.TypingSlow) AudioManager.I.PlaySFX("Tick1");
            else if (currentState == DialogState.TypingNormal && i % 2 == 0) AudioManager.I.PlaySFX("Tick1");
            else if (currentState == DialogState.TypingFast && i % 4 == 0) AudioManager.I.PlaySFX("Tick1");

            yield return new WaitForSeconds(currentSpeed);
        }

        CompleteTypingDisplay();
    }

    private void CompleteTypingDisplay()
    {
        contentText.maxVisibleCharacters = int.MaxValue;
        currentState = DialogState.TypingComplete;
        typingCoroutine = null;

        triangle.gameObject.SetActive(true);
        tweenTriangle?.Kill();
        triangle.color = new Color(triangle.color.r, triangle.color.g, triangle.color.b, 0.2f);
        tweenTriangle = triangle.DOFade(1f, 0.15f).SetLink(gameObject).SetLoops(-1, LoopType.Yoyo).Play();

        // [수정] 명확하게 참조를 저장하며 시작
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
        glitchCoroutine = StartCoroutine(SometimesGlitchTextLoop());
    }

    private void SkipTyping(bool isSFX = true)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        if (isSFX) AudioManager.I.PlaySFX("Tick1");
        CompleteTypingDisplay();
    }

    private void StartTyping(string text)
    {
        // [추가] 타이핑 시작 시 기존 글리치 코루틴이 있다면 확실히 정지
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
            glitchCoroutine = null;
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(ShowTextCoroutine(text));
    }

#if UNITY_EDITOR
    [Header("Editor Test")]
    public int testIndex;
    [Button]
    public void TestOpen() => Open(testIndex);
#endif
}