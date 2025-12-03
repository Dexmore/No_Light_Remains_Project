using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Steamworks;
using NaughtyAttributes;

public class DBManager : SingletonBehaviour<DBManager>
{
    protected override bool IsDontDestroy() => true;

    [Tooltip("0,1,2 -> 스팀슬롯  3,4,5 -> 로컬슬롯")]
    public int currentSlotIndex = 0;
    public CharacterData currentCharData;
    CharacterData savedCharData;
    [HideInInspector] public SaveData allSaveDataSteam;
    [HideInInspector] public SaveData allSaveDataLocal;
    [HideInInspector] public bool isLanternOn;

    // Steam API 초기화 성공 여부를 저장하는 플래그
    private bool isSteamInitialized = false;
    private string saveDirectoryPath => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "My Games", "REKINDLE");
    private string saveFilePath => Path.Combine(saveDirectoryPath, "SaveData");
    private string steamSaveFileName => Path.GetFileName(saveFilePath);

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

    IEnumerator Start()
    {
        yield return null;
        StartCoroutine(StartSteam());
        yield return new WaitUntil(() => IsSteam() == true);
        LoadSteam();
        LoadLocal();
    }

    private void OnApplicationQuit()
    {
        // if (IsSteam())
        // {
        //     SteamAPI.Shutdown();
        //     //Debug.Log("[DBManager] SteamAPI Shutdown.");
        // }
    }

#if UNITY_EDITOR
    private void EditorPlayChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            // if (IsSteam())
            // {
            //     SteamAPI.Shutdown();
            //     //Debug.Log("[DBManager] SteamAPI Shutdown.");
            // }
        }
    }
#endif 

    public IEnumerator StartSteam()
    {
        yield return null;
        SteamAPI.Init();
    }
    
    public bool IsSteam()
    {
        bool result = false;
        result = SteamAPI.IsSteamRunning();
        return result;
    }
    
    [Button]
    public void Save()
    {
        if (currentSlotIndex >= 0 && currentSlotIndex <= 2)
        {
            savedCharData = currentCharData;
            if (allSaveDataSteam.characterDatas == null)
            {
                allSaveDataSteam.characterDatas = new List<CharacterData>();
            }
            if (allSaveDataSteam.characterDatas.Count <= currentSlotIndex)
            {
                allSaveDataSteam.characterDatas.Add(savedCharData);
            }
            else
            {
                allSaveDataSteam.characterDatas[currentSlotIndex] = savedCharData;
            }
            SaveSteam();
        }
        else if (currentSlotIndex >= 3 && currentSlotIndex <= 5)
        {
            savedCharData = currentCharData;
            allSaveDataLocal.characterDatas[currentSlotIndex - 3] = savedCharData;
            SaveLocal();
        }
        else return;
    }
    
    [Button]
    public void Load()
    {
        // 1. 스팀 슬롯 (0~2)
        if (currentSlotIndex >= 0 && currentSlotIndex <= 2)
        {
            // 리스트가 존재하고, 해당 인덱스에 데이터가 있는지 확인 (안전장치)
            if (allSaveDataSteam.characterDatas != null && allSaveDataSteam.characterDatas.Count > currentSlotIndex)
            {
                savedCharData = allSaveDataSteam.characterDatas[currentSlotIndex];
                currentCharData = savedCharData;
                Debug.Log($"[DBManager] 스팀 슬롯 {currentSlotIndex} 로드 완료.");
            }
            else
            {
                Debug.LogWarning($"[DBManager] 스팀 슬롯 {currentSlotIndex}에 데이터가 없습니다. 새 데이터를 시작합니다.");
                currentCharData = new CharacterData(); // 빈 데이터로 초기화
            }
        }
        // 2. 로컬 슬롯 (3~5)
        else if (currentSlotIndex >= 3 && currentSlotIndex <= 5)
        {
            int localIndex = currentSlotIndex - 3;

            // 리스트가 존재하고, 해당 인덱스에 데이터가 있는지 확인 (안전장치)
            if (allSaveDataLocal.characterDatas != null && allSaveDataLocal.characterDatas.Count > localIndex)
            {
                savedCharData = allSaveDataLocal.characterDatas[localIndex];
                currentCharData = savedCharData;
                Debug.Log($"[DBManager] 로컬 슬롯 {localIndex} 로드 완료.");
            }
            else
            {
                Debug.LogWarning($"[DBManager] 로컬 슬롯 {localIndex}에 데이터가 없습니다. 새 데이터를 시작합니다.");
                currentCharData = new CharacterData(); // 빈 데이터로 초기화
            }
        }
    }

    void SaveSteam()
    {
        if (!IsSteam())
        {
            Debug.LogWarning("[DBManager] 스팀이 실행 중이 아니거나 초기화에 실패하여 저장할 수 없습니다.");
            return;
        }
        while (allSaveDataSteam.characterDatas.Count > 3)
            allSaveDataSteam.characterDatas.RemoveAt(allSaveDataSteam.characterDatas.Count - 1);
        try
        {
            string sd = JsonUtility.ToJson(allSaveDataSteam, true);
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
                allSaveDataSteam = JsonUtility.FromJson<SaveData>(sd);
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
            allSaveDataSteam = new SaveData(); // 문제 발생 시 새 데이터로 초기화
        }
    }

    public void SaveLocal()
    {
        while (allSaveDataLocal.characterDatas.Count > 3)
            allSaveDataLocal.characterDatas.RemoveAt(allSaveDataLocal.characterDatas.Count - 1);
        try
        {
            // 1. saveData를 JSON 문자열로 변환 (true: 가독성 좋게 포맷팅)
            string sd = JsonUtility.ToJson(allSaveDataLocal, true);
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
            allSaveDataLocal = new SaveData(); // 새 SaveData 객체 생성
            return;
        }
        try
        {
            // 2. 파일 읽기
            string sd = File.ReadAllText(saveFilePath);
            // 3. JSON 문자열을 saveData 객체로 변환
            allSaveDataLocal = JsonUtility.FromJson<SaveData>(sd);
            //Debug.Log($"[DBManager] 로컬 불러오기 성공: {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DBManager] 로컬 불러오기 실패 (파일 손상 가능성): {e.Message}");
            allSaveDataLocal = new SaveData(); // 문제 발생 시 새 데이터로 초기화
        }
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
    public int money;
    public string sceneName;
    public Vector2 lastPosition;
    public float HP;
    public float MP;
    public int potionCount;
    public List<ItemData> itemDatas;
    public List<GearData> gearDatas;
    public List<LanternData> lanternDatas;
    public List<RecordData> recordDatas;

    [System.Serializable]
    public struct ItemData
    {
        public string Name;
        public int count;
    }

    [System.Serializable]
    public struct GearData
    {
        public string Name;
        public bool isEquipped;
    }

    [System.Serializable]
    public struct LanternData
    {
        public string Name;
        public bool isEquipped;
    }

    [System.Serializable]
    public struct RecordData
    {
        public string Name;
    }
}
