using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using UnityEngine;
using Steamworks;
using UnityEngine.Events;
using NaughtyAttributes;
using UnityEngine.Localization.Settings;
public class DBManager : SingletonBehaviour<DBManager>
{
    protected override bool IsDontDestroy() => true;
    // 0,1,2 -> Steam Slot 
    // 3,4,5 -> Local Slot
    public int currSlot = 0;
    [HideInInspector] public CharacterData savedData;
    public SaveData allSaveDatasInSteam;
    public SaveData allSaveDatasInLocal;
    private string saveDirectoryPath
    {
        get
        {
            // 1. Windows 환경 처리 (#if UNITY_STANDALONE_WIN)
            // UNITY_STANDALONE_WIN이 명시적으로 Windows 빌드를 의미하지만,
            // STANDALONE_WIN/OSX가 아닌 환경(Android/iOS/WebGL 등)을 포괄하는 else 블록을 사용해 구분합니다.
#if UNITY_STANDALONE_WIN // Windows 환경 (사용자님의 기존 경로 유지)
            // 예: C:\Users\<User>\Documents\My Games\REKINDLE
            string windowsPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                "My Games",
                "REKINDLE"
            );
            return windowsPath;

            // 2. 기타 플랫폼 처리 (Mac, Android, iOS 등)
#else
        // Windows 외 모든 플랫폼은 유니티 표준 경로인 Application.persistentDataPath를 사용
        // 예: Mac -> ~/Library/Application Support/...
        // 예: Android -> /storage/...
        return Path.Combine(Application.persistentDataPath, "REKINDLE_SaveData");
#endif
        }
    }
    private string saveFilePath => Path.Combine(saveDirectoryPath, "SaveData");
    private string steamSaveFileName => Path.GetFileName(saveFilePath);
    public UnityAction onLogOut = () => { };
    public UnityAction onReLogIn = () => { };
    public UnityAction onSteamFail = () => { };
    public UnityAction onSteamResume = () => { };
    public ItemDatabase itemDatabase;
    [Space(60)]
    [Header("-----현재 플레이중인 캐릭터-----")]
    public CharacterData currData;
    public void Save()
    {
        CharacterData dataToSave;
        dataToSave = currData;
        dataToSave.maxHealth = RoundToOneDecimal(dataToSave.maxHealth);
        dataToSave.maxBattery = RoundToOneDecimal(dataToSave.maxBattery);
        dataToSave.currHealth = RoundToOneDecimal(dataToSave.currHealth);
        dataToSave.currBattery = RoundToOneDecimal(dataToSave.currBattery);
        dataToSave.lastPos.x = RoundToOneDecimal(dataToSave.lastPos.x);
        dataToSave.lastPos.y = RoundToOneDecimal(dataToSave.lastPos.y);
        for (int i = 0; i < dataToSave.sceneDatas.Count; i++)
        {
            CharacterData.SceneData sData = dataToSave.sceneDatas[i];
            if (sData.monsterPositionDatas != null)
            {
                for (int j = 0; j < sData.monsterPositionDatas.Count; j++)
                {
                    CharacterData.MonsterPositionData mData = sData.monsterPositionDatas[j];
                    mData.lastHealth = RoundToOneDecimal(mData.lastHealth);
                    mData.lastPos.x = RoundToOneDecimal(mData.lastPos.x);
                    mData.lastPos.y = RoundToOneDecimal(mData.lastPos.y);
                    sData.monsterPositionDatas[j] = mData;
                }
            }
            dataToSave.sceneDatas[i] = sData;
        }

        //
        savedData = dataToSave;
        if (currSlot >= 0 && currSlot <= 2)
        {
            if (allSaveDatasInSteam.characterDatas == null)
            {
                allSaveDatasInSteam.characterDatas = new List<CharacterData>();
            }
            if (allSaveDatasInSteam.characterDatas.Count <= currSlot)

            {
                allSaveDatasInSteam.characterDatas.Add(savedData);
            }
            else
            {
                allSaveDatasInSteam.characterDatas[currSlot] = savedData;
            }
            SaveSteam();
        }
        else if (currSlot >= 3 && currSlot <= 5)
        {
            allSaveDatasInLocal.characterDatas[currSlot - 3] = savedData;
            SaveLocal();
        }
        else return;
    }

    IEnumerator Start()
    {
        // YSH [추가] ItemDatabase의 로컬라이제이션 이벤트 등록 및 초기화
        if (!LocalizationSettings.InitializationOperation.IsDone)
        {
            yield return LocalizationSettings.InitializationOperation;
        }

        // 이제 로컬라이제이션 시스템이 준비되었으므로 데이터베이스 초기화
        if (itemDatabase != null)
        {
            itemDatabase.Initialize();
        }

        yield return null;
        StartSteam();
        float _time = Time.time;
        while (Time.time - _time < 3f)
        {
            if (IsSteam())
            {
                yield return YieldInstructionCache.WaitForSeconds(0.2f);
                LoadSteam();
                LoadLocal();

                // [추가] 2. 데이터 로드 후 모든 아이템/기록물의 번역 텍스트 강제 갱신
                if (itemDatabase != null) itemDatabase.RefreshAllData();

                yield return YieldInstructionCache.WaitForSeconds(0.2f);
                StopCoroutine(nameof(CheckLoop));
                StartCoroutine(nameof(CheckLoop));
                yield break;
            }
            yield return YieldInstructionCache.WaitForSeconds(0.5f);
        }

        // [추가] 3. 스팀이 없더라도 로컬 데이터를 불러온 후 갱신
        LoadLocal();
        if (itemDatabase != null) itemDatabase.RefreshAllData();
    }
    public void StartSteam()
    {
        if (SteamAPI.Init())
            _isInitialized = true;
        else
            _isInitialized = false;
    }
    public bool IsSteam()
    {
        bool result = false;
        result = SteamAPI.IsSteamRunning();
        return result;
    }
    bool _isInitialized;
    public bool IsSteamInit()
    {
        return _isInitialized;
    }
    public void StopSteam()
    {
        if (IsSteam())
        {
#if !UNITY_SERVER // 서버 환경이 아닌 경우에만 종료
            SteamAPI.Shutdown();
            _isInitialized = false;
#endif
        }
    }
    IEnumerator CheckLoop()
    {
        while (true)
        {
            bool prevBool1 = IsSteam();
            bool prevBool2 = IsSteamInit();
            yield return YieldInstructionCache.WaitForSeconds(Random.Range(0.5f, 1.2f));
            if (prevBool1 && !IsSteam())
            {
                onSteamFail.Invoke();
            }
            else if (!prevBool1 && IsSteam())
            {
                onSteamResume.Invoke();
            }
            if (prevBool2 && !IsSteamInit())
            {
                onLogOut.Invoke();
            }
            else if (!prevBool2 && IsSteamInit())
            {
                onReLogIn.Invoke();
            }
        }
    }
    private void OnApplicationQuit()
    {
        if (IsSteam())
        {
            SteamAPI.Shutdown();
        }
    }
    void OnEnable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += EditorPlayChanged;
#endif
    }
    void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged -= EditorPlayChanged;
#endif 
    }
#if UNITY_EDITOR
    private void EditorPlayChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            if (IsSteam())
            {
                SteamAPI.Shutdown();
            }
        }
    }
#endif
    public void SaveSteam()
    {
        if (!IsSteam())
        {
            Debug.LogWarning("[DBManager] 스팀이 실행 중이 아니거나 초기화에 실패하여 저장할 수 없습니다.");
            return;
        }
        while (allSaveDatasInSteam.characterDatas.Count > 3)
            allSaveDatasInSteam.characterDatas.RemoveAt(allSaveDatasInSteam.characterDatas.Count - 1);
        try
        {
            string sd = JsonUtility.ToJson(allSaveDatasInSteam, true);
            // JSON 문자열을 UTF-8 바이트 배열로 변환
            byte[] data = Encoding.UTF8.GetBytes(sd);

            // SteamRemoteStorage.FileWrite를 사용해 클라우드에 파일 쓰기
            if (SteamRemoteStorage.FileWrite(steamSaveFileName, data, data.Length))
            {
                //Debug.Log($"[DBManager] 스팀 클라우드 저장 성공: {steamSaveFileName}");
            }
            else
            {
                Debug.LogError($"[DBManager] 스팀 클라우드 저장 실패.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DBManager] 스팀 저장 중 예외 발생: {e.Message}");
        }
    }
    public void LoadSteam()
    {
        if (!IsSteam())
        {
            Debug.LogWarning("[DBManager] 스팀이 실행 중이 아니거나 초기화에 실패하여 불러올 수 없습니다.");
            return;
        }
        // 1. 스팀 클라우드에 파일이 존재하는지 확인
        if (!SteamRemoteStorage.FileExists(steamSaveFileName))
        {
            Debug.LogWarning($"[DBManager] 스팀 클라우드에 로드할 파일 없음: {steamSaveFileName}");
            // 파일이 없으면 로컬 로드를 시도하거나 새 데이터를 생성할 수 있습니다.
            // 여기서는 일단 로드를 중단합니다.
            return;
        }
        try
        {
            // 2. 파일 크기를 가져와서 바이트 배열 할당
            int fileSize = SteamRemoteStorage.GetFileSize(steamSaveFileName);
            byte[] data = new byte[fileSize];
            // 3. 파일 읽기
            int bytesRead = SteamRemoteStorage.FileRead(steamSaveFileName, data, data.Length);
            if (bytesRead > 0)
            {
                // 4. 바이트 배열을 UTF-8 문자열로 변환
                string sd = Encoding.UTF8.GetString(data);
                // 5. JSON을 객체로 역직렬화
                allSaveDatasInSteam = JsonUtility.FromJson<SaveData>(sd);
                //Debug.Log($"[DBManager] 스팀 클라우드 불러오기 성공: {steamSaveFileName}");
            }
            else
            {
                Debug.LogError($"[DBManager] 스팀 클라우드 파일 읽기 실패 (0 bytes read).");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DBManager] 스팀 불러오기 중 예외 발생 (파일 손상 가능성): {e.Message}");
            allSaveDatasInSteam = new SaveData(); // 문제 발생 시 새 데이터로 초기화
            allSaveDatasInSteam.characterDatas = new List<CharacterData>();
        }
    }
    string key = "fjlskj@!321dfjkog#$";
    // XOR 암호화
    private byte[] EncryptDecryptXOR(byte[] dataBytes, string key)
    {
        byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
        int keyLength = keyBytes.Length;
        // 결과는 입력 데이터와 동일한 길이의 byte 배열
        byte[] result = new byte[dataBytes.Length];
        for (int i = 0; i < dataBytes.Length; i++)
        {
            // 바이트 단위로 XOR 연산
            result[i] = (byte)(dataBytes[i] ^ keyBytes[i % keyLength]);
        }
        return result;
    }
    public void SaveLocal()
    {
        while (allSaveDatasInLocal.characterDatas.Count > 3)
            allSaveDatasInLocal.characterDatas.RemoveAt(allSaveDatasInLocal.characterDatas.Count - 1);
        try
        {
            // 1. saveData를 JSON 문자열로 변환 (true: 가독성 좋게 포맷팅)
            string sd = JsonUtility.ToJson(allSaveDatasInLocal, true);

            // 암호화
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(sd);
            byte[] obfuscatedBytes = EncryptDecryptXOR(dataBytes, key);
            sd = System.Convert.ToBase64String(obfuscatedBytes);

            // 2. 저장할 디렉토리(폴더)가 없으면 생성
            Directory.CreateDirectory(saveDirectoryPath);
            // 3. 파일 쓰기
            File.WriteAllText(saveFilePath, sd);
            //Debug.Log($"[DBManager] 로컬 저장 성공: {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DBManager] 로컬 저장 실패: {e.Message}");
        }
    }
    public void LoadLocal()
    {
        // 1. 저장 파일이 있는지 확인
        if (!File.Exists(saveFilePath))
        {
            //Debug.LogWarning($"[DBManager] 로드할 파일 없음. 새 데이터 생성: {saveFilePath}");
            allSaveDatasInLocal = new SaveData(); // 새 SaveData 객체 생성
            return;
        }
        try
        {
            // 2. 파일 읽기
            string sd = File.ReadAllText(saveFilePath);

            // 복호화
            byte[] obfuscatedBytes = System.Convert.FromBase64String(sd);
            byte[] dataBytes = EncryptDecryptXOR(obfuscatedBytes, key);
            sd = System.Text.Encoding.UTF8.GetString(dataBytes);

            // 3. JSON 문자열을 saveData 객체로 변환
            allSaveDatasInLocal = JsonUtility.FromJson<SaveData>(sd);
            //Debug.Log($"[DBManager] 로컬 불러오기 성공: {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DBManager] 로컬 불러오기 실패 (파일 손상 가능성): {e.Message}");
            allSaveDatasInLocal = new SaveData(); // 문제 발생 시 새 데이터로 초기화
        }
    }
    public void AddItem(string Name, int count = 1)
    {
        if (count == 0) return;
        int find = currData.itemDatas.FindIndex(x => x.Name == Name);
        if (find == -1)
        {
            CharacterData.ItemData newData = new CharacterData.ItemData();
            newData.Name = Name;
            newData.count = count;
            newData.isNew = true;
            currData.itemDatas.Add(newData);
        }
        else
        {
            CharacterData.ItemData findData = currData.itemDatas[find];
            int _count = findData.count;
            if (_count + count <= 0)
            {
                currData.itemDatas.Remove(findData);
            }
            else
            {
                findData.count = _count + count;
                currData.itemDatas[find] = findData;
            }
        }
    }
    /// <summary>
    /// 아이템 습득 메소드
    /// </summary>
    public void AddGear(string Name, int count = 1)
    {
        if (count == 0) return;
        int find = currData.gearDatas.FindIndex(x => x.Name == Name);
        if (find == -1)
        {
            CharacterData.GearData newData = new CharacterData.GearData();
            newData.Name = Name;
            newData.isNew = true;
            newData.isEquipped = false;
            currData.gearDatas.Add(newData);
        }
        else
        {
            CharacterData.GearData findData = currData.gearDatas[find];
            if (count < 0)
            {
                currData.gearDatas.Remove(findData);
            }
        }
    }
    public void AddRecord(string Name, int count = 1)
    {
        if (count == 0) return;
        int find = currData.recordDatas.FindIndex(x => x.Name == Name);
        if (find == -1)
        {
            CharacterData.RecordData newData = new CharacterData.RecordData();
            newData.Name = Name;
            newData.isNew = true;
            currData.recordDatas.Add(newData);
        }
        else
        {
            CharacterData.RecordData findData = currData.recordDatas[find];
            if (count < 0)
            {
                currData.recordDatas.Remove(findData);
            }
        }
    }
    public void AddLantern(string Name, int count = 1)
    {
        if (count == 0) return;
        int find = currData.itemDatas.FindIndex(x => x.Name == Name);
        if (find == -1)
        {
            CharacterData.LanternData newData = new CharacterData.LanternData();
            newData.Name = Name;
            newData.isNew = true;
            newData.isEquipped = false;
            currData.lanternDatas.Add(newData);
        }
        else
        {
            CharacterData.LanternData findData = currData.lanternDatas[find];
            if (count < 0)
            {
                currData.lanternDatas.Remove(findData);
            }
        }
    }
    /// <summary>
    /// 아이템 소지여부 (+몇개 소지하고있는지 +장착중인지도) 검사해주는 메소드
    /// </summary>
    public bool HasItem(string Name, out int count)
    {
        count = currData.itemDatas.Count(x => x.Name == Name);
        return count > 0;
    }
    public bool HasGear(string Name, out bool isEquip)
    {
        var findItems = currData.gearDatas.FindIndex(x => x.Name == Name);
        isEquip = false;
        if (findItems != -1)
        {
            isEquip = currData.gearDatas[findItems].isEquipped;
        }
        return findItems != -1;
    }
    public bool HasLantern(string Name, out bool isEquip)
    {
        var findItems = currData.lanternDatas.FindIndex(x => x.Name == Name);
        isEquip = false;
        if (findItems != -1)
        {
            isEquip = currData.gearDatas[findItems].isEquipped;
        }
        return findItems != -1;
    }
    public bool HasRecord(string Name)
    {
        var findItems = currData.recordDatas.FindIndex(x => x.Name == Name);
        return findItems != -1;
    }
    public void SetLastTimeReplayObject(ISavable savable)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string Name = savable.transform.name.Split("(")[0];
        if (Name == "") return;
        if (currData.sceneDatas.Count == 0) return;
        int find = currData.sceneDatas.FindIndex(x => x.sceneName == sceneName);
        if (find == -1) return;
        if (currData.sceneDatas[find].objectPositionDatas.Count == 0) return;
        int find2 = currData.sceneDatas[find].objectPositionDatas.FindIndex(x => x.Name == Name);
        if (find2 == -1) return;
        System.DateTime now = System.DateTime.Now;
        string datePart = now.ToString("yyyy.MM.dd");
        int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
        var objectPositionData = currData.sceneDatas[find].objectPositionDatas[find2];
        objectPositionData.lastCompleteTime = $"{datePart}-{secondsOfDay}";
        currData.sceneDatas[find].objectPositionDatas[find2] = objectPositionData;
    }
    public void SetProgress(string Name, int progress)
    {
        int find = currData.progressDatas.FindIndex(x => x.Name == Name);
        if (find == -1)
        {
            CharacterData.ProgressData progressData = new CharacterData.ProgressData();
            progressData.Name = Name;
            progressData.progress = progress;
            currData.progressDatas.Add(progressData);
        }
        else
        {
            var temp = currData.progressDatas[find];
            temp.progress = progress;
            currData.progressDatas[find] = temp;
        }
    }
    public int GetProgress(string Name)
    {
        int find = currData.progressDatas.FindIndex(x => x.Name == Name);
        if (find == -1)
        {
            return -1;
        }
        else
        {
            return currData.progressDatas[find].progress;
        }
    }

    public int GetGearLevel(string name)
    {
        int index = currData.gearDatas.FindIndex(x => x.Name == name);
        if (index != -1)
        {
            return currData.gearDatas[index].level;
        }
        return 0; // 없으면 0 리턴
    }

    public void LevelUpGear(string name)
    {
        int index = currData.gearDatas.FindIndex(x => x.Name == name);
        if (index != -1)
        {
            // 구조체는 값 타입이므로 복사 -> 수정 -> 재할당 해야 함
            CharacterData.GearData data = currData.gearDatas[index];
            data.level = 1; // 1회 제한이므로 1로 설정 (또는 data.level++)
            currData.gearDatas[index] = data;

            // (선택사항) 저장 기능을 바로 호출하고 싶다면
            // Save(); 
        }
    }

#if UNITY_EDITOR
    [Space(60)]
    [Header("-----아이템 습득 테스트용-----")]
    public int testGold;
    public InventoryItem[] testItemDatas;
    public GearData[] testGearDatas;
    public LanternFunctionData[] testLanternDatas;
    public RecordData[] testRecordDatas;

    [Button("아이템 습득 테스트")]
    public void TestAddItem()
    {
        currData.gold += testGold;
        for (int i = 0; i < testItemDatas.Length; i++)
        {
            AddItem(testItemDatas[i].data.name, testItemDatas[i].quantity);
        }
        for (int i = 0; i < testGearDatas.Length; i++)
        {
            AddGear(testGearDatas[i].name);
        }
        for (int i = 0; i < testLanternDatas.Length; i++)
        {
            AddLantern(testLanternDatas[i].name);
        }
        for (int i = 0; i < testRecordDatas.Length; i++)
        {
            AddRecord(testRecordDatas[i].name);
        }
    }
    [Button("내 계정의 모든 세이브 삭제 (주의)")]
    public void DeleteAllSaveData()
    {
        allSaveDatasInLocal = new SaveData();
        allSaveDatasInLocal.characterDatas = new List<CharacterData>();
        allSaveDatasInSteam = new SaveData();
        allSaveDatasInSteam.characterDatas = new List<CharacterData>();
        SaveSteam();
        SaveLocal();
    }

#endif
    private float RoundToOneDecimal(float value)
    {
        return Mathf.Round(value * 100f) * 0.01f;
    }
}
[System.Serializable]
public struct SaveData
{
    public List<CharacterData> characterDatas;
}
[System.Serializable]
public struct CharacterData
{
    public float maxHealth;
    public float maxBattery;
    public float currHealth;
    public float currBattery;
    public int gold;
    public int maxGearCost;
    public int maxPotionCount;
    public int currPotionCount;
    public int difficulty;
    public string sceneName;
    public int death;
    public string lastTime;
    public Vector2 lastPos;
    public List<ItemData> itemDatas;
    public List<GearData> gearDatas;
    public List<LanternData> lanternDatas;
    public List<RecordData> recordDatas;
    public List<SceneData> sceneDatas;
    public List<ProgressData> progressDatas;
    public List<KillCount> killCounts;
    [System.Serializable]
    public struct ItemData
    {
        public string Name;
        public int count;
        public bool isNew;
    }
    [System.Serializable]
    public struct GearData
    {
        public string Name;
        public bool isNew;
        public bool isEquipped;
        public int level;
    }
    [System.Serializable]
    public struct LanternData
    {
        public string Name;
        public bool isNew;
        public bool isEquipped;
    }
    [System.Serializable]
    public struct RecordData
    {
        public string Name;
        public bool isNew;
    }

    [System.Serializable]
    public struct SceneData
    {
        public string sceneName;
        public List<MonsterPositionData> monsterPositionDatas;
        public List<ObjectPositionData> objectPositionDatas;
        public string lastTime;
    }
    [System.Serializable]
    public struct MonsterPositionData
    {
        public string Name;
        public int index; // 이름이 동일한 몬스터일시 구분 번호
        public string lastDeathTime; // 빈문자열 ""일시 죽지 않고 살아있는 상태
        public Vector2 lastPos;
        public float lastHealth;
    }
    [System.Serializable]
    public struct ObjectPositionData
    {
        public string Name;
        public int index; // 이름이 동일한 오브젝트일시 구분 번호
        public string lastCompleteTime; // 빈문자열 ""일시 아직 작동완료 안된 상태
    }
    [System.Serializable]
    public struct ProgressData
    {
        public string Name;
        public int progress;
        public bool isComplete;
        public int replayWaitTimeSecond;
    }
    // 업적용
    public struct KillCount
    {
        public string Name;
        public int count;
    }
}
