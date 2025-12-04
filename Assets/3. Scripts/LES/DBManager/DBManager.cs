using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using Steamworks;
using UnityEngine.Events;
using NaughtyAttributes;
public class DBManager : SingletonBehaviour<DBManager>
{
    protected override bool IsDontDestroy() => true;
    // 0,1,2 -> Steam Slot 
    // 3,4,5 -> Local Slot
    public int currSlot = 0;
    public CharacterData currData;
    CharacterData savedData;
    public SaveData allSaveDatasInSteam;
    public SaveData allSaveDatasInLocal;
    private string saveDirectoryPath => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "My Games", "REKINDLE");
    private string saveFilePath => Path.Combine(saveDirectoryPath, "SaveData");
    private string steamSaveFileName => Path.GetFileName(saveFilePath);
    public UnityAction onLogout = () => { };
    public UnityAction onReLogin = () => { };
    public void Save()
    {
        if (currSlot >= 0 && currSlot <= 2)
        {
            savedData = currData;
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
            savedData = currData;
            allSaveDatasInLocal.characterDatas[currSlot - 3] = savedData;
            SaveLocal();
        }
        else return;
    }
    IEnumerator Start()
    {
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
                yield return YieldInstructionCache.WaitForSeconds(0.2f);
                StopCoroutine(nameof(CheckLoop));
                StartCoroutine(nameof(CheckLoop));
                yield break;
            }
            yield return YieldInstructionCache.WaitForSeconds(0.5f);
        }
        // 시간 초과
    }
    public void StartSteam()
    {
        if(SteamAPI.Init())
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
            bool prevStepIsSteam = IsSteam();
            yield return YieldInstructionCache.WaitForSeconds(Random.Range(0.3f, 1.2f));
            if (!prevStepIsSteam && IsSteam())
            {
                onReLogin.Invoke();
            }
            else if (prevStepIsSteam && !IsSteam())
            {
                onLogout.Invoke();
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
    void SaveSteam()
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
    public void SaveLocal()
    {
        while (allSaveDatasInLocal.characterDatas.Count > 3)
            allSaveDatasInLocal.characterDatas.RemoveAt(allSaveDatasInLocal.characterDatas.Count - 1);
        try
        {
            // 1. saveData를 JSON 문자열로 변환 (true: 가독성 좋게 포맷팅)
            string sd = JsonUtility.ToJson(allSaveDatasInLocal, true);
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
    public ItemDatabase itemDatabase;
    public void AddItem(string Name, int count)
    {
        if (count == 0) return;
        int find = currData.itemDatas.FindIndex(x => x.Name == Name);
        if (find == -1)
        {
            CharacterData.ItemData itd = new CharacterData.ItemData();
            itd.Name = Name;
            itd.count = count;
            itd.isNew = true;
            currData.itemDatas.Add(itd);
        }
        else
        {
            CharacterData.ItemData currItd = currData.itemDatas[find];
            int _count = currItd.count;
            if (_count + count <= 0)
            {
                currData.itemDatas.Remove(currItd);
            }
            else
            {
                currItd.count = _count + count;
                currData.itemDatas[find] = currItd;
            }
        }
    }
#if UNITY_EDITOR
    [Button]
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
    public int potionCount;
    public int difficulty;
    public int language;
    public string sceneName;
    public Vector2 lastPos;
    public List<ItemData> itemDatas;
    public List<GearData> gearDatas;
    public List<LanternData> lanternDatas;
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
    }
    [System.Serializable]
    public struct LanternData
    {
        public string Name;
    }

    // [System.Serializable]
    // public struct SpawnData
    // {
    //     public string sceneName;
    //     public string monsterName;
    //     public Vector2 lastPos;
    //     public bool isDie;
    //     public float currHealth;
    // }



}
