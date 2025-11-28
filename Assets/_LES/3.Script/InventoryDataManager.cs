using UnityEngine;
using System.Collections.Generic;
using System;
using NaughtyAttributes;
using System.Linq;
using System.Collections; // NaughtyAttributes 사용

public class InventoryDataManager : MonoBehaviour
{
    public static InventoryDataManager Instance { get; private set; }

    [Header("데이터베이스 연결")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("플레이어 소유 데이터")]
    public List<ItemData> PlayerItems = new List<ItemData>();
    public List<GearData> PlayerGears = new List<GearData>();
    public List<LanternFunctionData> PlayerLanternFunctions = new List<LanternFunctionData>();
    public List<RecordData> PlayerRecords = new List<RecordData>();

    [Header("플레이어 스탯")]
    public int PlayerMoney = 0;
    public int MaxGearCost = 3;

    public event Action OnInventoryChanged;
    public event Action OnGearsChanged;
    public event Action OnLanternsChanged;
    public event Action OnRecordsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else { Instance = this; DontDestroyOnLoad(gameObject); }
    }

    private IEnumerator Start()
    {
        // 1. DBManager가 준비될 때까지 잠시 대기 (안전장치)
        // (DBManager도 Start에서 파일을 읽으므로, 순서를 보장하기 위함)
        yield return new WaitUntil(() => DBManager.I != null);
        
        // DBManager가 파일 읽기를 완료할 시간을 1프레임 정도 줍니다.
        yield return null; 

        // 2. 게임 시작 시 자동으로 불러오기 실행!
        LoadFromDB();
    }

    public void AddItem(ItemData itemToAdd)
    {
        if (itemToAdd == null) return;
        PlayerItems.Add(itemToAdd);
        OnInventoryChanged?.Invoke();
    }

    public void AddItem(GearData gearToAdd)
    {
        if (gearToAdd == null) return;

        if (!PlayerGears.Contains(gearToAdd))
        {
            PlayerGears.Add(gearToAdd);
            OnGearsChanged?.Invoke();
            Debug.Log($"[InventoryDataManager] 기어 추가: {gearToAdd.name}");
        }
        else
        {
            Debug.LogWarning($"[InventoryDataManager] 이미 소유한 기어입니다: {gearToAdd.name}");
        }
    }

    public void AddItem(LanternFunctionData functionToAdd)
    {
        if (functionToAdd == null) return;

        // [추가] 이미 가지고 있는지 확인
        if (!PlayerLanternFunctions.Contains(functionToAdd))
        {
            PlayerLanternFunctions.Add(functionToAdd);
            OnLanternsChanged?.Invoke();
            Debug.Log($"[InventoryDataManager] 랜턴 기능 추가: {functionToAdd.name}");
        }
        else
        {
            Debug.LogWarning($"[InventoryDataManager] 이미 소유한 랜턴 기능입니다: {functionToAdd.name}");
        }
    }

    public void AddItem(RecordData recordToAdd)
    {
        if (recordToAdd == null) return;
        
        if (!PlayerRecords.Contains(recordToAdd))
        {
            PlayerRecords.Add(recordToAdd);
            OnRecordsChanged?.Invoke();
            Debug.Log($"[InventoryDataManager] 기록물 추가: {recordToAdd.name}");
        }
    }
    
    public void RemoveItem(ItemData itemToRemove)
    {
        if (itemToRemove != null && PlayerItems.Remove(itemToRemove))
        {
            OnInventoryChanged?.Invoke();
        }
    }
    
    public bool PurchaseGearCostSlot(int cost)
    {
        if (PlayerMoney >= cost && MaxGearCost < 6)
        {
            PlayerMoney -= cost;
            MaxGearCost++;
            OnGearsChanged?.Invoke();
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public void AddMoney(int amount)
    {
        PlayerMoney += amount;
        OnInventoryChanged?.Invoke();
    }

    #region 테스트용 코드
    
    [Button("Test: 현재 장착 중인 모든 아이템 출력")]
    private void PrintEquippedItems()
    {
        Debug.Log("--- 현재 장착 중인 아이템 목록 ---");
        List<GearData> equippedGears = GetEquippedGears();
        if (equippedGears.Count > 0)
        {
            foreach (GearData gear in equippedGears) Debug.Log($"[기어] {gear.gearName} (코스트: {gear.cost})");
        }
        else Debug.Log("[기어] 장착된 기어 없음.");
        
        LanternFunctionData equippedLantern = GetEquippedLanternFunction();
        if (equippedLantern != null) Debug.Log($"[랜턴] {equippedLantern.functionName}");
        else Debug.Log("[랜턴] 장착된 랜턴 기능 없음.");
        Debug.Log("------------------------------------");
    }

    // [저장] 현재 인벤토리 상태를 DBManager의 CharacterData로 변환하여 보냅니다.
    public void SaveToDB()
    {
        if (DBManager.I == null) return;

        // 1. DBManager의 현재 캐릭터 데이터 가져오기 (참조)
        // (주의: 구조체는 값 복사이므로, 새로 만든 뒤 덮어씌워야 할 수도 있습니다.)
        CharacterData charData = DBManager.I.currentCharData;

        // 2. 기본 스탯 저장
        charData.money = PlayerMoney;
        // charData.MaxGearCost = MaxGearCost; (DBManager의 CharacterData에 이 필드가 없다면 추가 필요)

        // 3. 아이템 저장 (이름과 개수)
        charData.itemDatas = new List<CharacterData.ItemData>();
        foreach (var item in PlayerItems)
        {
            CharacterData.ItemData saveData = new CharacterData.ItemData();
            saveData.Name = item.name; // 파일 이름 사용 (또는 item.itemName)
            saveData.count = 1; // (중복 가능한 아이템이라면 개수 로직 추가 필요)
            charData.itemDatas.Add(saveData);
        }

        // 4. 기어 저장
        charData.gearDatas = new List<CharacterData.GearData>();
        foreach (var gear in PlayerGears)
        {
            CharacterData.GearData saveData = new CharacterData.GearData();
            saveData.Name = gear.name;
            charData.gearDatas.Add(saveData);
        }

        // 5. 랜턴 저장
        charData.lanternDatas = new List<CharacterData.LanternData>();
        foreach (var lantern in PlayerLanternFunctions)
        {
            CharacterData.LanternData saveData = new CharacterData.LanternData();
            saveData.Name = lantern.name;
            charData.lanternDatas.Add(saveData);
        }

        // 6. DBManager에 갱신된 데이터 전달
        DBManager.I.currentCharData = charData;
        
        // 7. 실제 파일/클라우드 저장 호출
        DBManager.I.Save();
        
        Debug.Log("[InventoryDataManager] DBManager에 데이터 저장 완료.");
    }

    // [불러오기] DBManager의 데이터를 가져와 인벤토리를 복구합니다.
    public void LoadFromDB()
    {
        if (DBManager.I == null || itemDatabase == null) return;

        // 1. DBManager에서 불러오기 (파일 로드)
        DBManager.I.Load();
        CharacterData charData = DBManager.I.currentCharData;

        // 2. 데이터 초기화
        PlayerItems.Clear();
        PlayerGears.Clear();
        PlayerLanternFunctions.Clear();
        
        // 3. 기본 스탯 복구
        PlayerMoney = charData.money;
        // MaxGearCost = charData.MaxGearCost; 

        // 4. 아이템 복구 (이름 -> 원본 데이터 찾기)
        if (charData.itemDatas != null)
        {
            foreach (var savedItem in charData.itemDatas)
            {
                ItemData originalData = itemDatabase.FindItemByName(savedItem.Name);
                if (originalData != null)
                {
                    // TODO: 개수(count)만큼 추가하는 로직 필요
                    PlayerItems.Add(originalData);
                }
            }
        }

        // 5. 기어 복구
        if (charData.gearDatas != null)
        {
            foreach (var savedGear in charData.gearDatas)
            {
                GearData originalData = itemDatabase.FindGearByName(savedGear.Name);
                if (originalData != null) PlayerGears.Add(originalData);
            }
        }

        // 6. 랜턴 복구
        if (charData.lanternDatas != null)
        {
            foreach (var savedLantern in charData.lanternDatas)
            {
                LanternFunctionData originalData = itemDatabase.FindLanternByName(savedLantern.Name);
                if (originalData != null) PlayerLanternFunctions.Add(originalData);
            }
        }

        // 7. UI 갱신 방송
        OnInventoryChanged?.Invoke();
        OnGearsChanged?.Invoke();
        OnLanternsChanged?.Invoke();
        
        Debug.Log("[InventoryDataManager] DBManager로부터 데이터 로드 완료.");
    }

    private void OnApplicationQuit()
    {
        SaveToDB();
    }

    #endregion
    
    #region 캐릭터 담당자용 API
    
    public List<GearData> GetEquippedGears()
    {
        return PlayerGears.Where(gear => gear != null && gear.isEquipped).ToList();
    }
    
    public LanternFunctionData GetEquippedLanternFunction()
    {
        return PlayerLanternFunctions.FirstOrDefault(func => func != null && func.isEquipped);
    }

    #endregion
}