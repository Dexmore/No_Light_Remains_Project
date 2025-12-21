using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;
public class GameManager : SingletonBehaviour<GameManager>
{
    protected override bool IsDontDestroy() => true;

    [Header("로딩시 이미지 아래에서 랜덤으로 하나 출력")]
    public Sprite[] loadingSprites;
    string[] loadingTexts;
    void InitLocale()
    {
        // 영어
        if (SettingManager.I.setting.locale == 0)
        {
            loadingTexts = new string[]
            {
                "Regain your Lantern gage by parrying enemy's attacks.\nEnemies are stunned when parried certain times."
                ,
                "It's often better to use healing items early rather than waiting for an emergency."
                ,
                "Observing the patterns of stronger enemies is key to victory."
                ,
                "The Alchemists, in their pursuit of immortality, eventually monopolized the Helios energy."
            };
        }
        // 한글
        else if (SettingManager.I.setting.locale == 1)
        {
            loadingTexts = new string[]
            {
                "적의 공격을 패링하면 랜턴 게이지가 충전됩니다.\n특정 횟수 이상 패링 시 적은 기절합니다."
                ,
                "회복은 위급할 때보다 더 미리 사용하는 것이 좋습니다."
                ,
                "강한 적일수록 패턴을 관찰하는 것이 중요합니다."
                ,
                "불멸을 추구하던 알케미스트들은 결국 일리오스 에너지를 독점하고 말았습니다."
                ,
            };
        }
    }

    // 씬 넘어가도 유지시킬 변수들
    [HideInInspector] public bool isLanternOn;
    [HideInInspector] public bool isOpenPop;
    [HideInInspector] public bool isOpenDialog;
    [HideInInspector] public bool isSceneWaiting;
    [HideInInspector] public bool isShowPop0;

    // 게임의 중요 이벤트들
    public UnityAction<HitData> onHit = (x) => { };
    public UnityAction<HitData> onParry = (x) => { };
    public UnityAction<HitData> onAvoid = (x) => { };
    public UnityAction<HitData> onHitAfter = (x) => { };
    public UnityAction<HitData> onDie = (x) => { };
    public UnityAction<int, DialogTrigger> onDialogTriggerEnter = (x, y) => { };
    public UnityAction onSceneChangeAfter = () => { };
    public UnityAction onSceneChangeBefore = () => { };

    void OnEnable()
    {
        InitFade();
        InitLoading();
        isOpenPop = false;
        isOpenDialog = false;
        InitLocale();
    }
    void OnDisable()
    {

    }
    #region Load Scene
    public async void LoadSceneAsync(int index, bool loadingScreen = false, bool isDie = false)
    {
        isSceneWaiting = true;
        loadingSlider.value = 0f;
        FadeOut(0.8f);
        await Task.Delay(500);
        if (!isDie)
        {
            await SaveAllMonsterAndObject();
        }
        await Task.Delay(500);
        AsyncOperation ao = SceneManager.LoadSceneAsync(index);
        onSceneChangeBefore.Invoke();
        if (loadingScreen)
        {
            StartLoading();
        }
        while (!ao.isDone)
        {
            await Task.Delay(10);
            loadingProgress = ao.progress;
        }
        if (loadingScreen)
        {
            while (!isLoadingDone)
            {
                await Task.Delay(10);
            }
        }
        await Task.Delay(10);
        isSceneWaiting = false;
        onSceneChangeAfter.Invoke();
        await Task.Delay(500);
        FadeIn(0.4f);
        await Task.Delay(500);
    }
    public async void LoadSceneAsync(string name, bool loadingScreen = false, bool isDie = false)
    {
        isSceneWaiting = true;
        loadingSlider.value = 0f;
        FadeOut(0.8f);
        await Task.Delay(500);
        if (!isDie)
        {
            await SaveAllMonsterAndObject();
        }
        await Task.Delay(500);
        AsyncOperation ao = SceneManager.LoadSceneAsync(name);
        onSceneChangeBefore.Invoke();
        if (loadingScreen)
        {
            StartLoading();
        }
        while (!ao.isDone)
        {
            await Task.Delay(10);
            loadingProgress = ao.progress;
        }
        if (loadingScreen)
        {
            while (!isLoadingDone)
            {
                await Task.Delay(10);
            }
        }
        await Task.Delay(10);
        isSceneWaiting = false;
        onSceneChangeAfter.Invoke();
        await Task.Delay(500);
        FadeIn(0.4f);
        await Task.Delay(500);
    }
    #endregion
    #region Fade
    GameObject fadeScreen;
    Image dimImg;
    Sequence sequenceFade;
    void InitFade()
    {
        fadeScreen = transform.Find("FadeScreen").gameObject;
        dimImg = transform.Find("FadeScreen/Dim").GetComponent<Image>();
        fadeScreen.SetActive(false);
        dimImg.gameObject.SetActive(false);
        dimImg.color = new Color(0f, 0f, 0f, 0f);
    }
    public void FadeOut(float time)
    {
        sequenceFade?.Kill();
        DOTween.Kill(dimImg);
        if (time == 0)
        {
            fadeScreen.SetActive(true);
            dimImg.gameObject.SetActive(true);
            dimImg.color = new Color(0f, 0f, 0f, 1f);
            return;
        }
        float startA2 = dimImg.color.a;
        if (startA2 == 0f)
        {
            //시작
            fadeScreen.SetActive(true);
            dimImg.gameObject.SetActive(true);
            dimImg.color = new Color(0f, 0f, 0f, 0f);
            //진행
            Tween tween;
            tween = dimImg.DOFade(1f, time).SetEase(Ease.OutQuad);
            tween.OnComplete(() =>
            {
                fadeScreen.SetActive(true);
                dimImg.gameObject.SetActive(true);
                dimImg.color = new Color(0f, 0f, 0f, 1f);
            });
            sequenceFade?.Append(tween);
        }
        else
        {
            //시작
            fadeScreen.SetActive(true);
            dimImg.gameObject.SetActive(true);
            //진행
            Tween tween;
            tween = dimImg.DOFade(1f, (1f - startA2) * 1.5f + 0.5f);
            tween.OnComplete(() =>
            {
                fadeScreen.SetActive(true);
                dimImg.gameObject.SetActive(true);
                dimImg.color = new Color(0f, 0f, 0f, 1f);
            });
            sequenceFade?.Append(tween);
        }
    }
    public void FadeIn(float time)
    {
        sequenceFade?.Kill();
        DOTween.Kill(dimImg);
        if (time == 0)
        {
            fadeScreen.SetActive(false);
            dimImg.gameObject.SetActive(false);
            dimImg.color = new Color(0f, 0f, 0f, 0f);
            return;
        }
        //시작
        fadeScreen.SetActive(true);
        dimImg.gameObject.SetActive(true);
        dimImg.color = new Color(0f, 0f, 0f, 1f);
        //진행
        Tween tween;
        tween = dimImg.DOFade(0f, 2.3f).SetEase(Ease.InSine);
        tween.OnComplete(() =>
        {
            fadeScreen.SetActive(false);
            dimImg.gameObject.SetActive(false);
            dimImg.color = new Color(0f, 0f, 0f, 0f);
        });
        sequenceFade?.Append(tween);
    }
    #endregion
    #region Loading Page
    GameObject loadingScreen;
    Image loadingDim;
    Slider loadingSlider;
    float loadingProgress;
    Tween loadingTween;
    bool isLoadingDone;
    TMP_Text loadingText;
    Image loadingImage;
    void InitLoading()
    {
        loadingText = transform.Find("LoadingScreen/Text").GetComponent<TMP_Text>();
        loadingImage = transform.Find("LoadingScreen/Background").GetComponent<Image>();
        loadingScreen = transform.Find("LoadingScreen").gameObject;
        loadingDim = transform.Find("LoadingScreen/Dim").GetComponent<Image>();
        loadingSlider = transform.Find("LoadingScreen/Slider").GetComponent<Slider>();
    }
    async void StartLoading()
    {
        InitLocale();
        isLoadingDone = false;
        loadingProgress = 0f;
        loadingScreen.SetActive(true);
        loadingDim.gameObject.SetActive(true);
        loadingDim.color = new Color(0f, 0f, 0f, 1f);
        loadingTween?.Kill();
        loadingTween = loadingDim.DOFade(0f, 1.2f).SetEase(Ease.InSine);
        float elapsedTime = 0f;
        int randomInt = Random.Range(0, loadingSprites.Length);
        loadingImage.sprite = loadingSprites[randomInt];
        randomInt = Random.Range(0, loadingTexts.Length);
        loadingText.text = loadingTexts[randomInt];
        // 가짜 로딩
        while (true)
        {
            await Task.Delay(10);
            elapsedTime += 0.01f;
            loadingSlider.value = 0.4f * (elapsedTime / 3f);
            if (elapsedTime > 3f)
            {
                break;
            }
        }
        // 진짜 씬 로딩
        while (true)
        {
            await Task.Delay(10);
            loadingSlider.value = 0.4f + 0.6f * loadingProgress;
            if (loadingProgress == 1f)
            {
                break;
            }
        }
        loadingTween?.Kill();
        loadingTween = loadingDim.DOFade(1f, 1.2f).SetEase(Ease.InSine).OnComplete(() =>
        {
            loadingScreen.SetActive(false);
            loadingDim.gameObject.SetActive(false);
        });
        isLoadingDone = true;
    }

    #endregion
    #region Hit
    [Space(50)]
    public Material hitTintMat;
    #endregion
    #region HitEffect
    float hitEffectStartTime;
    Transform camMainTr;
    private Tween timeSlowTween;
    public void HitEffect(Vector2 point, float amount)
    {
        if (camMainTr == null) camMainTr = Camera.main.transform;
        if (Time.time - hitEffectStartTime < 0.3f) return;
        amount = Mathf.Clamp01(amount);
        hitEffectStartTime = Time.time;
        ScreenShake(amount);
    }
    async void ScreenShake(float amount)
    {
        if (amount <= 0f) return;
        if (camMainTr == null) return;
        float duration = 0.037f + 0.078f * amount;
        float strength = 0.136f + 0.192f * amount;
        int vibrato = 9; // 흔들림 횟수
        float randomness = 60f; // 랜덤성
        if (duration > 0.22f) duration = 0.22f;
        DOTween.Kill(camMainTr);
        await camMainTr.DOShakePosition(duration, strength, vibrato, randomness).AsyncWaitForCompletion();
    }
    // async void TimeSlow(float amount)
    // {
    //     float a = amount;
    //     if (a <= 0.01f) a = 0.01f;
    //     float slowTimeScale = 0.2f * (1f / a);
    //     slowTimeScale = Mathf.Clamp01(slowTimeScale);
    //     float slowDuration = 0.005f + 0.01f * amount;
    //     float resetDuration = 0.008f + 0.015f * amount;
    //     timeSlowTween?.Kill();
    //     timeSlowTween = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, slowTimeScale, 0.1f).SetUpdate(true);
    //     await Task.Delay((int)(1000 * 0.1f)); // 감속 트윈이 완료될 때까지 대기
    //     await Task.Delay((int)(1000 * slowDuration));
    //     timeSlowTween = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, resetDuration)
    //         .SetUpdate(true)
    //         .OnComplete(() =>
    //         {
    //             Time.timeScale = 1f; // 혹시 모를 오차를 위해 명확하게 1로 설정
    //         });
    //     await Task.Delay((int)(1000 * resetDuration)); // 복구 트윈이 완료될 때까지 대기
    // }
    #endregion
    #region Glitch Effect
    private const int glitchDelayMs = 25;
    private readonly string[] glitchChars = { "#", "$", "%", "@", "^", "&", "*", "!", "?", "█", "■", "░", "~", "_", "=" };
    public async void GlitchText(UnityEngine.UI.Text Text, float glitchDuration = 0.4f)
    {
        if (Text == null) return;
        string originalText = Text.text;
        int totalIterations = (int)(glitchDuration * 1000f / glitchDelayMs);
        for (int i = 0; i < totalIterations; i++)
        {
            await Task.Delay(glitchDelayMs);
            // 글리치 문자열 생성 (태그가 출력되지 않도록 보장)
            Text.text = GenerateGlitchString(originalText, Text.color);
        }
        // 효과 완료 후 원본 텍스트로 복구
        Text.text = originalText;
    }
    public async void GlitchText(TMPro.TMP_Text tMP_Text, float glitchDuration = 0.4f)
    {
        if (tMP_Text == null) return;
        string originalText = tMP_Text.text;
        int totalIterations = (int)(glitchDuration * 1000f / glitchDelayMs);
        for (int i = 0; i < totalIterations; i++)
        {
            await Task.Delay(glitchDelayMs);
            // 글리치 문자열 생성 (태그가 출력되지 않도록 보장)
            tMP_Text.text = GenerateGlitchString(originalText, tMP_Text.color);
        }
        // 효과 완료 후 원본 텍스트로 복구
        tMP_Text.text = originalText;
    }
    private string GenerateGlitchString(string original, Color originalColor)
    {
        if (string.IsNullOrEmpty(original))
        {
            return "";
        }
        StringBuilder sb = new StringBuilder(original.Length * 3);
        string[] InsertChars = new string[] { "*", "░", "█" };
        for (int i = 0; i < original.Length; i++)
        {
            char originalChar = original[i];
            if (Random.value < 0.1f)
            {
                int insertCount = Random.Range(1, 4);
                for (int k = 0; k < insertCount; k++)
                {
                    sb.Append(InsertChars[Random.Range(0, InsertChars.Length)]);
                }
            }
            if (Random.value < 0.6f)
            {
                if (Random.value < 0.3f)
                {
                    sb.Append(glitchChars[Random.Range(0, glitchChars.Length)]);
                }
                else
                {
                    Color randomColor = 0.6f * originalColor + 0.4f * new Color(Random.value, Random.value, Random.value);
                    string colorHex = ColorUtility.ToHtmlStringRGB(randomColor);
                    sb.Append($"<color=#{colorHex}>{originalChar}</color>");
                }
            }
            else
            {
                sb.Append(originalChar);
            }
        }
        return sb.ToString();
    }
    #endregion
    public void SetSceneFromDB(bool isLeftDirection = false)
    {
        SetScene(DBManager.I.currData.lastPos, isLeftDirection);
    }
    public async void SetScene(Vector2 position, bool isLeftDirection = false)
    {
        #region Player & Camera
        PlayerControl playerControl1 = FindAnyObjectByType<PlayerControl>();
        FollowCamera followCamera1 = FindAnyObjectByType<FollowCamera>();
        PlayerControl playerControl2 = null;
        FollowCamera followCamera2 = null;
        await Task.Delay(20);
        while (isSceneWaiting)
        {
            if (playerControl2 == null)
                playerControl2 = FindAnyObjectByType<PlayerControl>();
            if (followCamera2 == null)
                followCamera2 = FindAnyObjectByType<FollowCamera>();
            await Task.Delay(20);
            if (playerControl2 != null && playerControl2 != null
            && playerControl1 != playerControl2
            && followCamera1 != followCamera2) break;
        }
        await Task.Delay(20);
        if (playerControl2 == null)
            playerControl2 = FindAnyObjectByType<PlayerControl>();
        if (followCamera2 == null)
            followCamera2 = FindAnyObjectByType<FollowCamera>();
        if (!isLeftDirection)
        {
            playerControl2.childTR.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            playerControl2.childTR.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
        if (position != Vector2.zero)
        {
            if (playerControl2 != null)
            {
                playerControl2.transform.position = position;
                playerControl2.startPosition = position;

            }
            float boundedX = Mathf.Clamp(position.x, followCamera2.xBound.x, followCamera2.xBound.y);
            float boundedY = Mathf.Clamp(position.y, followCamera2.yBound.x, followCamera2.yBound.y);
            if (followCamera2 != null)
            {
                followCamera2.transform.position = new Vector3(boundedX, boundedY, 0f) + followCamera2.offset;
            }
        }
        #endregion
        LoadAllMonsterAndObject();
    }
    public async Task SaveAllMonsterAndObject()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Stage")) return;
        MonsterControl[] allMonsters = FindObjectsByType<MonsterControl>(FindObjectsInactive.Include, sortMode: FindObjectsSortMode.InstanceID);
        ISavable[] allSavableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID).OfType<ISavable>().ToArray();
        List<CharacterData.SceneData> sceneDatas = DBManager.I.currData.sceneDatas;
        int find = sceneDatas.FindIndex(x => x.sceneName == sceneName);
        CharacterData.SceneData sceneData;
        if (find == -1)
        {
            sceneData = new CharacterData.SceneData();
            sceneData.sceneName = sceneName;
            // 몬스터 부분
            sceneData.monsterPositionDatas = new List<CharacterData.MonsterPositionData>();
            for (int i = 0; i < allMonsters.Length; i++)
            {
                if (!allMonsters[i].transform.name.Contains("(")) continue;
                if (int.TryParse(allMonsters[i].transform.name.Split("(")[1].Split(")")[0], out int result))
                {
                    CharacterData.MonsterPositionData monsterPositionData = new CharacterData.MonsterPositionData();
                    monsterPositionData.Name = allMonsters[i].transform.name.Split("(")[0];
                    monsterPositionData.index = result;
                    monsterPositionData.lastHealth = allMonsters[i].currHealth;
                    monsterPositionData.lastPos = allMonsters[i].transform.position;
                    if (allMonsters[i].gameObject.activeInHierarchy)
                    {
                        monsterPositionData.lastDeathTime = "";
                    }
                    else
                    {
                        System.DateTime now = System.DateTime.Now;
                        string datePart = now.ToString("yyyy.MM.dd");
                        int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
                        monsterPositionData.lastDeathTime = $"{datePart}-{secondsOfDay}";
                    }
                    sceneData.monsterPositionDatas.Add(monsterPositionData);
                }
                else continue;
            }
            // 오브젝트 부분
            sceneData.objectPositionDatas = new List<CharacterData.ObjectPositionData>();
            for (int i = 0; i < allSavableObjects.Length; i++)
            {
                // 중요 : 완전히 완료되지않은 오브젝트는 상태를 저장하지 않는다. 즉 중간만 진행한 오브젝트는 씬 재입장시 처음부터 다시해야함
                if (!allSavableObjects[i].IsComplete) continue;
                if (!allSavableObjects[i].transform.name.Contains("(")) continue;
                if (int.TryParse(allSavableObjects[i].transform.name.Split("(")[1].Split(")")[0], out int result))
                {
                    CharacterData.ObjectPositionData objectPositionData = new CharacterData.ObjectPositionData();
                    objectPositionData.Name = allSavableObjects[i].transform.name.Split("(")[0];
                    objectPositionData.index = result;
                    objectPositionData.lastPos = allSavableObjects[i].transform.position;
                    if (allSavableObjects[i].transform.gameObject.activeInHierarchy)
                    {
                        objectPositionData.lastCompleteTime = "";
                    }
                    else
                    {
                        System.DateTime now = System.DateTime.Now;
                        string datePart = now.ToString("yyyy.MM.dd");
                        int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
                        objectPositionData.lastCompleteTime = $"{datePart}-{secondsOfDay}";
                    }
                    sceneData.objectPositionDatas.Add(objectPositionData);
                }
                else continue;
            }
            DBManager.I.currData.sceneDatas.Add(sceneData);
        }
        else
        {
            sceneData = sceneDatas[find];
            // 몬스터 부분
            for (int i = 0; i < allMonsters.Length; i++)
            {
                if (!allMonsters[i].transform.name.Contains("(")) continue;
                if (int.TryParse(allMonsters[i].transform.name.Split("(")[1].Split(")")[0], out int result))
                {
                    string mName = allMonsters[i].transform.name.Split("(")[0];
                    int find2 = sceneData.monsterPositionDatas.FindIndex(x => x.Name == mName && x.index == result);
                    if (find2 == -1)
                    {
                        CharacterData.MonsterPositionData monsterPositionData = new CharacterData.MonsterPositionData();
                        monsterPositionData.Name = allMonsters[i].transform.name.Split("(")[0];
                        monsterPositionData.index = result;
                        monsterPositionData.lastHealth = allMonsters[i].currHealth;
                        monsterPositionData.lastPos = allMonsters[i].transform.position;
                        if (allMonsters[i].gameObject.activeInHierarchy)
                        {
                            monsterPositionData.lastDeathTime = "";
                        }
                        else
                        {
                            System.DateTime now = System.DateTime.Now;
                            string datePart = now.ToString("yyyy.MM.dd");
                            int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
                            monsterPositionData.lastDeathTime = $"{datePart}-{secondsOfDay}";
                        }
                        sceneData.monsterPositionDatas.Add(monsterPositionData);
                    }
                    else
                    {
                        CharacterData.MonsterPositionData monsterPositionData = sceneData.monsterPositionDatas[find2];
                        monsterPositionData.index = result;
                        monsterPositionData.lastHealth = allMonsters[i].currHealth;
                        monsterPositionData.lastPos = allMonsters[i].transform.position;
                        if (allMonsters[i].gameObject.activeInHierarchy)
                        {
                            monsterPositionData.lastDeathTime = "";
                        }
                        else if (monsterPositionData.lastDeathTime == "")
                        {
                            System.DateTime now = System.DateTime.Now;
                            string datePart = now.ToString("yyyy.MM.dd");
                            int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
                            monsterPositionData.lastDeathTime = $"{datePart}-{secondsOfDay}";
                        }
                        sceneData.monsterPositionDatas[find2] = monsterPositionData;
                    }
                }
                else continue;
            }
            // 오브젝트 부분
            for (int i = 0; i < allSavableObjects.Length; i++)
            {
                // 중요 : 완전히 완료되지않은 오브젝트는 상태를 저장하지 않는다. 즉 중간만 진행한 오브젝트는 씬 재입장시 처음부터 다시해야함
                if (!allSavableObjects[i].IsComplete) continue;
                if (!allSavableObjects[i].transform.name.Contains("(")) continue;
                if (int.TryParse(allSavableObjects[i].transform.name.Split("(")[1].Split(")")[0], out int result))
                {
                    string mName = allSavableObjects[i].transform.name.Split("(")[0];
                    int find2 = sceneData.objectPositionDatas.FindIndex(x => x.Name == mName && x.index == result);
                    if (find2 == -1)
                    {
                        CharacterData.ObjectPositionData objectPositionData = new CharacterData.ObjectPositionData();
                        objectPositionData.Name = allSavableObjects[i].transform.name.Split("(")[0];
                        objectPositionData.index = result;
                        objectPositionData.lastPos = allSavableObjects[i].transform.position;
                        if (allSavableObjects[i].transform.gameObject.activeInHierarchy)
                        {
                            objectPositionData.lastCompleteTime = "";
                        }
                        else
                        {
                            System.DateTime now = System.DateTime.Now;
                            string datePart = now.ToString("yyyy.MM.dd");
                            int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
                            objectPositionData.lastCompleteTime = $"{datePart}-{secondsOfDay}";
                        }
                        sceneData.objectPositionDatas.Add(objectPositionData);
                    }
                    else
                    {
                        CharacterData.ObjectPositionData objectPositionData = sceneData.objectPositionDatas[find2];
                        objectPositionData.index = result;
                        objectPositionData.lastPos = allSavableObjects[i].transform.position;
                        if (allSavableObjects[i].transform.gameObject.activeInHierarchy)
                        {
                            objectPositionData.lastCompleteTime = "";
                        }
                        else if (objectPositionData.lastCompleteTime == "")
                        {
                            System.DateTime now = System.DateTime.Now;
                            string datePart = now.ToString("yyyy.MM.dd");
                            int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
                            objectPositionData.lastCompleteTime = $"{datePart}-{secondsOfDay}";
                        }
                        sceneData.objectPositionDatas[find2] = objectPositionData;
                    }
                }
                else continue;
            }
            DBManager.I.currData.sceneDatas[find] = sceneData;
        }
        await Task.Delay(200);
    }
    void LoadAllMonsterAndObject()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        MonsterControl[] allMonsters = FindObjectsByType<MonsterControl>(FindObjectsInactive.Include, sortMode: FindObjectsSortMode.InstanceID);
        ISavable[] allSavableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID).OfType<ISavable>().ToArray();
        List<CharacterData.SceneData> sceneDatas = DBManager.I.currData.sceneDatas;
        int find = sceneDatas.FindIndex(x => x.sceneName == sceneName);
        CharacterData.SceneData sceneData;
        if (find != -1)
        {
            sceneData = sceneDatas[find];
            //몬스터 불러오기
            for (int i = 0; i < allMonsters.Length; i++)
            {
                if (!allMonsters[i].transform.name.Contains("(")) continue;
                if (int.TryParse(allMonsters[i].transform.name.Split("(")[1].Split(")")[0], out int result))
                {
                    int findIndex = sceneData.monsterPositionDatas.FindIndex(x => x.Name == allMonsters[i].transform.name.Split("(")[0] && x.index == result);
                    if (findIndex == -1) continue;
                    var monsterPositionData = sceneData.monsterPositionDatas[findIndex];
                    // 서버 저장데이터상 현재 몬스터가 살아있는걸로 되어있는경우
                    if (monsterPositionData.lastDeathTime == "")
                    {
                        allMonsters[i].transform.Root().position = monsterPositionData.lastPos;
                        allMonsters[i].currHealth = monsterPositionData.lastHealth;
                        //allMonsters[i].transform.Root().gameObject.SetActive(true);
                    }
                    // 서버 저장데이터상 이 몬스터가 죽어있는걸로 되어있는경우. 씬 시작시 아래처럼 셋팅
                    else
                    {
                        // 1. 저장된 스트링 분리 (날짜와 초)
                        string[] parts = monsterPositionData.lastDeathTime.Split('-');
                        if (parts.Length == 2)
                        {
                            // 2. 날짜 파싱 및 시간 복구
                            // ParseExact를 사용하여 "2025.05.30" 형태를 날짜로 바꿉니다.
                            System.DateTime deathDate = System.DateTime.ParseExact(parts[0], "yyyy.MM.dd", null);
                            // 날짜에 '하루 중 지난 초'를 더해 정확한 사망 시점을 만듭니다.
                            System.DateTime deathTime = deathDate.AddSeconds(double.Parse(parts[1]));
                            // 3. 현재 시간과의 차이 계산 (TimeSpan)
                            System.TimeSpan timePassed = System.DateTime.Now - deathTime;
                            // 4. 5분(300초) 이상 경과했는지 확인
                            int waitMinutes = 3;
                            switch (allMonsters[i].data.Type)
                            {
                                case MonsterType.Small:
                                    waitMinutes = 3;
                                    break;
                                case MonsterType.Middle:
                                    waitMinutes = 5;
                                    break;
                                case MonsterType.Large:
                                    waitMinutes = 12;
                                    break;
                                case MonsterType.Boss:
                                    waitMinutes = 18;
                                    break;
                            }
                            if (timePassed.TotalMinutes >= waitMinutes)
                            {
                                // [부활 조건 충족] 
                                // 아무것도 안 함 = 하이라키에 배치된 프리팹 상태(활성화) 그대로 유지
                            }
                            else
                            {
                                // [아직 죽어있어야 함]
                                allMonsters[i].transform.Root().gameObject.SetActive(false);
                            }
                        }
                    }
                }
                else continue;
            }
            //오브젝트 불러오기
            for (int i = 0; i < allSavableObjects.Length; i++)
            {
                if (!allSavableObjects[i].transform.name.Contains("(")) continue;
                if (int.TryParse(allSavableObjects[i].transform.name.Split("(")[1].Split(")")[0], out int result))
                {
                    int findIndex = sceneData.objectPositionDatas.FindIndex(x => x.Name == allSavableObjects[i].transform.name.Split("(")[0] && x.index == result);
                    if (findIndex == -1) continue;
                    var objectPositionData = sceneData.objectPositionDatas[findIndex];
                    // 애초부터 '완전히 완료된 오브젝트'만 DB에 저장하기 때문에.
                    // 죽지않은 몬스터들도 위치와 체력 셋팅을 해줬던과 달리.
                    // 아직 완료하지 않은 오브젝트의 셋팅은 고려할 필요가 없다.
                    if (allSavableObjects[i].CanReplay)
                    {
                        // 1. 저장된 스트링 분리 (날짜와 초)
                        string[] parts = objectPositionData.lastCompleteTime.Split('-');
                        if (parts.Length == 2)
                        {
                            // 2. 날짜 파싱 및 시간 복구
                            // ParseExact를 사용하여 "2025.05.30" 형태를 날짜로 바꿉니다.
                            System.DateTime deathDate = System.DateTime.ParseExact(parts[0], "yyyy.MM.dd", null);
                            // 날짜에 '하루 중 지난 초'를 더해 정확한 사망 시점을 만듭니다.
                            System.DateTime deathTime = deathDate.AddSeconds(double.Parse(parts[1]));
                            // 3. 현재 시간과의 차이 계산 (TimeSpan)
                            System.TimeSpan timePassed = System.DateTime.Now - deathTime;
                            int waitSecond = allSavableObjects[i].ReplayWaitTimeSecond;
                            if (timePassed.TotalSeconds >= waitSecond)
                            {
                                // [부활 조건 충족] 
                                // 아무것도 안 함 = 하이라키에 배치된 프리팹 상태(활성화) 그대로 유지
                            }
                            else
                            {
                                // [아직 죽어있어야 함]
                                // 몬스터때의 비활성화와 달리. 아래 메소드가 FX,효과음 작동 연출을 전부 무시한 즉시 완료상태임
                                allSavableObjects[i].SetCompletedState();
                            }
                        }
                    }
                    else
                    {
                        allSavableObjects[i].SetCompletedState();
                    }
                }
                else continue;
            }
        }
    }
}
public struct HitData
{
    public string attackName;
    public Transform attacker;
    public Transform target;
    public Vector3 hitPoint;
    public float damage;
    public AttackType attackType;
    public StaggerType staggerType;
    public bool isCannotParry;
    public string[] particleNames;
    public enum AttackType
    {
        Default,
        Chafe,
        Bullet,
        Trap,
    }
    public enum StaggerType
    {
        None,
        Small,
        Middle,
        Large,
    }
    public HitData(string attackName, Transform attacker, Transform target, float damage, Vector3 hitPoint, string[] particleNames, StaggerType staggerType = StaggerType.Small, AttackType attackType = AttackType.Default)
    {
        this.attackName = attackName;
        this.attacker = attacker;
        this.target = target;
        this.hitPoint = hitPoint;
        this.damage = damage;
        this.staggerType = staggerType;
        this.attackType = attackType;
        this.isCannotParry = false;
        this.particleNames = particleNames;
    }
}

// {
//     English, // 영어
//     Korean,  // 한국어
//     German,  // 독일어
//     French,  // 프랑스어
//     Spanish, // 스페인어
//     ChineseSimplified, // 중국어(간체)
//     Japanese, // 일본어
//     Russian,  // 러시아어
//     PortugueseBrazil, // 브라질-포르투갈어
//     Arabic, //아랍어
//     Hindi, //인도어
// }
