using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
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

        // 2. 씬 데이터 청소 (뒤에서부터 순회해야 삭제 시 인덱스가 안 꼬임)
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        for (int i = dataToSave.sceneDatas.Count - 1; i >= 0; i--)
        {
            // struct일 경우 값을 수정하려면 원본 리스트에 다시 넣어줘야 하므로 직접 참조
            var sData = dataToSave.sceneDatas[i];

            if (sData.sceneName == sceneName) continue;

            // 몬스터 리스폰 시간 체크 및 데이터 비우기
            if (sData.md != null && sData.md.Count > 0)
            {
                System.DateTime deathTime = System.DateTimeOffset.FromUnixTimeSeconds(sData.t).DateTime;
                System.TimeSpan timePassed = System.DateTime.UtcNow - deathTime;

                if (timePassed.TotalSeconds >= 300) // 5분
                {
                    sData.md = new List<CharacterData.MData>(0);
                    sData.t = 0;
                }
            }

            // 수정된 데이터를 리스트에 다시 반영 (struct 대응)
            dataToSave.sceneDatas[i] = sData;

            // 아무 기록도 남지 않은 씬 데이터는 리스트에서 완전 삭제
            if ((sData.od == null || sData.od.Count == 0) &&
                (sData.md == null || sData.md.Count == 0))
            {
                dataToSave.sceneDatas.RemoveAt(i);
            }
        }

        // savedData = dataToSave;(이줄 제거)
        // 깊은 복사
        string json = JsonUtility.ToJson(dataToSave);
        savedData = JsonUtility.FromJson<CharacterData>(json);

        if (currSlot >= 0 && currSlot <= 2)
        {
            if (allSaveDatasInSteam.cds == null)
            {
                allSaveDatasInSteam.cds = new List<CharacterData>();
            }
            if (allSaveDatasInSteam.cds.Count <= currSlot)

            {
                allSaveDatasInSteam.cds.Add(savedData);
            }
            else
            {
                allSaveDatasInSteam.cds[currSlot] = savedData;
            }
            SaveSteam();
        }
        else if (currSlot >= 3 && currSlot <= 5)
        {
            allSaveDatasInLocal.cds[currSlot - 3] = savedData;
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

                yield return YieldInstructionCache.WaitForSeconds(1.5f);
                yield return new WaitUntil(() => IsSteam() && IsSteamInit());
                yield return null;
                CleanSteamCloudExceptMine();

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
        if (_isInitialized)
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
        if (IsSteam() || IsSteamInit())
        {
            SteamAPI.Shutdown();
        }
    }
    void OnEnable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += EditorPlayChanged;
#endif
        GameManager.I.onSceneChangeBefore += SceneChangeBeforeHandler;
    }
    void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged -= EditorPlayChanged;
#endif 
        GameManager.I.onSceneChangeBefore -= SceneChangeBeforeHandler;
    }
#if UNITY_EDITOR
    private void EditorPlayChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            if (IsSteam() || IsSteamInit())
            {
                SteamAPI.Shutdown();
            }
        }
    }
#endif
    public void SaveSteam()
    {
        if (!IsSteam() || !IsSteamInit())
        {
            Debug.LogWarning("[DBManager] 스팀이 실행 중이 아니거나 초기화에 실패하여 저장할 수 없습니다.");
            return;
        }
        while (allSaveDatasInSteam.cds.Count > 3)
            allSaveDatasInSteam.cds.RemoveAt(allSaveDatasInSteam.cds.Count - 1);
        try
        {
            string sd = JsonUtility.ToJson(allSaveDatasInSteam, false);
            // JSON 문자열을 UTF-8 바이트 배열로 변환
            byte[] data = Encoding.UTF8.GetBytes(sd);



            // --- GZip 압축 시작 ---
            byte[] rawData = Encoding.UTF8.GetBytes(sd);
            byte[] compressedData;
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    gzip.Write(rawData, 0, rawData.Length);
                }
                compressedData = output.ToArray();
            }
            // -----------------------

            //-----------
            // 1. 기존 파일을 지워서 클라우드 점유율을 즉시 낮춤 (중요)
            if (SteamRemoteStorage.FileExists(steamSaveFileName))
            {
                SteamRemoteStorage.FileDelete(steamSaveFileName);
            }
            //-----------


            // SteamRemoteStorage.FileWrite를 사용해 클라우드에 파일 쓰기
            if (SteamRemoteStorage.FileWrite(steamSaveFileName, compressedData, compressedData.Length))
            {
                Debug.Log($"[DBManager] 스팀 클라우드 저장 성공: {steamSaveFileName}");
            }
            else
            {
                SteamRemoteStorage.GetQuota(out ulong total, out ulong available);
                Debug.LogError($"[DBManager] 스팀 저장 실패! 남은 용량: {available} 바이트. (파일명: {steamSaveFileName})");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DBManager] 스팀 저장 중 예외 발생: {e.Message}");
        }
    }
    public void LoadSteam()
    {
        if (!IsSteam() || !IsSteamInit())
        {
            Debug.LogWarning("[DBManager] 스팀이 실행 중이 아니거나 초기화에 실패하여 불러올 수 없습니다.");
            return;
        }

        // 1. 스팀 클라우드에 파일이 존재하는지 확인
        if (!SteamRemoteStorage.FileExists(steamSaveFileName))
        {
            Debug.LogWarning($"[DBManager] 스팀 클라우드에 로드할 파일 없음: {steamSaveFileName}");
            return;
        }

        try
        {
            // 2. 파일 크기 가져오기 및 압축된 데이터 읽기
            int fileSize = SteamRemoteStorage.GetFileSize(steamSaveFileName);
            byte[] compressedData = new byte[fileSize];
            int bytesRead = SteamRemoteStorage.FileRead(steamSaveFileName, compressedData, fileSize);

            if (bytesRead > 0)
            {
                // --- GZip 압축 해제 시작 ---
                using (MemoryStream input = new MemoryStream(compressedData))
                using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
                using (MemoryStream output = new MemoryStream())
                {
                    // 압축된 데이터를 풀어서 output 스트림에 복사
                    gzip.CopyTo(output);

                    // 해제된 바이트 배열을 UTF-8 문자열(JSON)로 변환
                    byte[] rawData = output.ToArray();
                    string sd = Encoding.UTF8.GetString(rawData);

                    // 3. JSON을 객체로 역직렬화
                    allSaveDatasInSteam = JsonUtility.FromJson<SaveData>(sd);
                    //Debug.Log($"[DBManager] 스팀 로드 및 압축 해제 성공! 원본 크기: {rawData.Length} bytes");
                }
                // ---------------------------
            }
            else
            {
                Debug.LogError($"[DBManager] 스팀 클라우드 파일 읽기 실패 (0 bytes read).");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DBManager] 스팀 불러오기 중 예외 발생 (데이터 형식이 다르거나 손상됨): {e.Message}");
            // 데이터 구조가 깨졌거나, 압축 안 된 옛날 파일일 경우 새 데이터로 초기화
            allSaveDatasInSteam = new SaveData { cds = new List<CharacterData>() };
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
        while (allSaveDatasInLocal.cds.Count > 3)
            allSaveDatasInLocal.cds.RemoveAt(allSaveDatasInLocal.cds.Count - 1);
        try
        {
            // 1. saveData를 JSON 문자열로 변환 (true: 가독성 좋게 포맷팅)
            string sd = JsonUtility.ToJson(allSaveDatasInLocal, false);

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
            if (_count >= 999) return;
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
        isEquip = false;

        // [추가된 안전장치]
        // 게임이 종료되는 시점에 데이터가 없거나 DBManager가 불안정하면 false 반환
        if (currData.gearDatas == null)
        {
            return false;
        }

        var findItems = currData.gearDatas.FindIndex(x => x.Name == Name);

        if (findItems != -1)
        {
            // 인덱스 범위 안전 체크 (혹시 모를 에러 방지)
            if (findItems < currData.gearDatas.Count)
            {
                isEquip = currData.gearDatas[findItems].isEquipped;
            }
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
    public void SetProgress(string Name, int progress)
    {
        int find = currData.pds.FindIndex(x => x.Name == Name);
        if (find == -1)
        {
            CharacterData.ProgressData progressData = new CharacterData.ProgressData();
            progressData.Name = Name;
            progressData.progress = progress;
            currData.pds.Add(progressData);
        }
        else
        {
            var temp = currData.pds[find];
            temp.progress = progress;
            currData.pds[find] = temp;
        }
    }
    public int GetProgress(string Name)
    {
        int find = currData.pds.FindIndex(x => x.Name == Name);
        if (find == -1)
        {
            return -1;
        }
        else
        {
            return currData.pds[find].progress;
        }
    }

    public int GetKillcount(string Name)
    {
        int find = currData.ks.FindIndex(x => x.Name == Name);
        if (find == -1)
        {
            return -1;
        }
        else
        {
            return currData.ks[find].count;
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
            GameManager.I.RefreshGears();
        }
        int count1 = itemDatabase.allGears.Count;
        int count2 = currData.gearDatas.Count;
        if (count1 == count2)
        {
            bool isAll = true;
            for (int i = 0; i < currData.gearDatas.Count; i++)
            {
                if (currData.gearDatas[i].level == 0)
                    isAll = false;
                break;
            }
            if (isAll)
            {
                SteamAchievement("ACH_ALL_UPGRADE");
            }
        }
    }

    public int ach11count;

    public void SteamAchievement(string API_Name)
    {
        //Debug.Log($"Try Achievement {API_Name}");
        if (!IsSteam()) return;
        if (!SteamAPI.Init()) return;

        SteamUserStats.GetAchievement(API_Name, out bool isUnlocked);
        if (!isUnlocked)
        {
            SteamUserStats.SetAchievement(API_Name);
            SteamUserStats.StoreStats();
        }
        // 구현된 스팀 도전과제
        // 1. 게임 시작 시 미션 수락 --> ACH_MISSION_START
        // 2. 첫 기어 획득 --> ACH_FIRST_GEAR_GET
        // 3. 메인 웨이브 클리어  --> ACH_MAIN_WAVE_CLEAR
        // 4. 보스 첫 클리어.  --> ACH_FIRST_BOSS_CLEAR
        // 5. 모든 기어 획득 --> ACH_ALL_GEAR
        // 6. 모든 기록물 획득 --> ACH_ALL_RECORD
        // 7. 모든 기어 강화완료 --> ACH_ALL_UPGRADE
        // 8. 모든 난이도 클리어 --> ACH_ALL_CLEAR (완료)
        // 9. 포션을 먹지않고 보스 클리어 --> ACH_NOPOTION_BOSS_CLEAR
        // 10. 스토리 난이도 1시간 이내 클리어 --> ACH_ONEHOUR_STORY (완료)
        // 11. 보통 난이도 1시간 이내 클리어 --> ACH_ONEHOUR_NORMAL (완료)
        // 12. 어려움 난이도 1시간 이내 클리어 --> ACH_ONEHOUR_HARD
        // 13. 루멘테크 가동횟수 20회 이상 --> ACH_LUMENTECH
        // 14. 패링 100회 이상 성공 --> ACH_PARRYCOUNT
        // 15. 모든 몬스터 처치 --> ACH_ALL_MONSTERKILL



    }
    void SceneChangeBeforeHandler()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        MonsterControl[] monsterControls = FindObjectsByType<MonsterControl>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (monsterControls.Length == 0)
        {
            switch (sceneName)
            {
                case "Stage0":
                    if (currData.ach14count == 0)
                        currData.ach14count = 1;
                    break;
                case "Stage1":
                    if (currData.ach14count == 1)
                        currData.ach14count = 2;
                    break;
                case "Stage2":
                    if (currData.ach14count == 2)
                        currData.ach14count = 3;
                    break;
                case "Stage3":
                    if (currData.ach14count == 3)
                        currData.ach14count = 4;
                    break;
            }
        }
    }


    public void OpenLoginUI()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }
    public void CloseLoginUI()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void CleanSteamCloudExceptMine()
    {
        if (!IsSteam() || !IsSteamInit())
        {
            Debug.LogError("Steam이 초기화되지 않았습니다.");
            return;
        }

        // 1. [중요] 내가 사용하는 '진짜' 파일 이름들만 여기에 등록하세요.
        // 여기에 없는 이름은 클라우드에서 발견되는 즉시 삭제됩니다.
        HashSet<string> myValidFiles = new HashSet<string>
        {
            "SaveData",        // 현재 사용 중인 파일명
            //"SettingData"      // 만약 설정 파일도 클라우드에 올린다면 추가
        };
        // 2. 클라우드에 있는 전체 파일 개수 확인
        int fileCount = SteamRemoteStorage.GetFileCount();
        if (fileCount <= 0)
        {
            Debug.Log("Cloud Is Clean");
            return;
        }
        Debug.Log($"[DBManager] 클라우드 정리 시작... 총 파일 수: {fileCount}");
        int deleteCount = 0;
        // 3. 리스트를 뒤에서부터 순회하며 삭제 (인덱스 꼬임 방지)
        for (int i = fileCount - 1; i >= 0; i--)
        {
            int fileSize;
            string fileName = SteamRemoteStorage.GetFileNameAndSize(i, out fileSize);
            // 4. 내가 허용한 이름 리스트에 없다면? 찌꺼기이므로 삭제!
            if (!myValidFiles.Contains(fileName))
            {
                bool success = SteamRemoteStorage.FileDelete(fileName);
                if (success)
                {
                    deleteCount++;
                    Debug.Log($"[DBManager] 찌꺼기 삭제 성공: {fileName} ({fileSize} bytes)");
                }
            }
        }
        if (deleteCount > 0)
        {
            Debug.Log($"[DBManager] 클라우드 정리 완료! 삭제된 파일 수: {deleteCount}");
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



#endif
    private float RoundToOneDecimal(float value)
    {
        return Mathf.Round(value * 100f) * 0.01f;
    }

    [Button("모든 저장 파일 삭제")]
    public void DangerouslyClearAllCloudFiles()
    {
        if (!IsSteam() || !IsSteamInit())
        {
            Debug.LogError("Steam이 초기화되지 않았습니다.");
            return;
        }

        // 1. 클라우드에 있는 파일 개수 파악
        int fileCount = SteamRemoteStorage.GetFileCount();
        Debug.Log($"<color=red>총 {fileCount}개의 파일을 발견했습니다.</color>");

        // 2. 모든 파일을 순회하며 삭제
        for (int i = 0; i < fileCount; i++)
        {
            int fileSize;
            string fileName = SteamRemoteStorage.GetFileNameAndSize(i, out fileSize);

            bool deleted = SteamRemoteStorage.FileDelete(fileName);
            Debug.Log($"파일 삭제: {fileName} ({fileSize} bytes) -> 결과: {deleted}");
        }

        // 3. 결과 확인
        SteamRemoteStorage.GetQuota(out ulong total, out ulong available);
        Debug.Log($"<color=green>초기화 완료! 남은 용량: {available} / {total}</color>");
    }

}
[System.Serializable]
public struct SaveData
{
    public List<CharacterData> cds;
    public int ach10bitMask;
}
[System.Serializable]
public struct CharacterData
{
    public float maxHealth;
    public float maxBattery;
    public float currHealth;
    public float currBattery;
    public int gold;
    public int mgc;
    public int mpc;
    public int cpc;
    public int difficulty;
    public string sceneName;
    public int death;
    public string lastTime;
    public int seed;
    public Vector2 lastPos;
    public List<ItemData> itemDatas;
    public List<GearData> gearDatas;
    public List<LanternData> lanternDatas;
    public List<RecordData> recordDatas;
    public List<SData> sceneDatas;
    public List<ProgressData> pds;
    public List<KillCount> ks;
    //
    public int ach12count;
    public int ach13count;
    public int ach14count;
    public long ach15time;

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
        public int level;
        public bool isEquipped;
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
    public struct SData
    {
        public string sceneName;
        public List<MData> md;
        public List<OData> od;
        public long t;
    }
    [System.Serializable]
    public struct MData
    {
        public string Name;
        public int index; // 이름이 동일한 몬스터일시 구분 번호
    }
    [System.Serializable]
    public struct OData
    {
        public string Name;
        public int index; // 이름이 동일한 오브젝트일시 구분 번호
        public bool cr;
    }
    [System.Serializable]
    public struct ProgressData
    {
        public string Name;
        public int progress;
        public bool isComplete;
    }
    public struct KillCount
    {
        public string Name;
        public int count;
    }
}
