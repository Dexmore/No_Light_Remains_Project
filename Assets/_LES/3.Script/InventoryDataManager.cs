using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using NaughtyAttributes; // C# Event (Action)을 사용하기 위해

// <summary>
// 플레이어의 모든 인벤토리 관련 데이터를 소유하고 관리하는 중앙 싱글톤 매니저.
// 몬스터, 상점 등 모든 외부 시스템은 이 매니저의 공개 함수(API)만을 호출해야 합니다.

public class InventoryDataManager : MonoBehaviour
{
    // 1. 싱글톤 패턴 (어디서든 InventoryDataManager.Instance로 접근)
    public static InventoryDataManager Instance { get; private set; }

    // 2. 실제 플레이어 데이터 (ScriptableObject 원본을 참조)
    [Header("플레이어 소유 데이터")]
    public List<ItemData> PlayerItems = new List<ItemData>();
    public List<GearData> PlayerGears = new List<GearData>();
    public List<LanternFunctionData> PlayerLanternFunctions = new List<LanternFunctionData>();
    public List<RecordData> PlayerRecords = new List<RecordData>();

    [Header("플레이어 스탯")]
    public int PlayerMoney = 0;
    public int MaxGearCost = 3; // (기본 3개 시작, 상점에서 6까지 확장)

    // 3. UI에게 "데이터 변경됨!"을 알릴 C# 이벤트
    public event Action OnInventoryChanged;
    public event Action OnGearsChanged;
    public event Action OnLanternsChanged;
    public event Action OnRecordsChanged;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음
        }
    }

    // --- 4. [핵심] 팀원들이 호출할 공개 API (AddItem) ---

    // (1번) 소지템/재료를 추가합니다. (기존과 동일)    
    public void AddItem(ItemData itemToAdd)
    {
        if (itemToAdd == null) return;
        PlayerItems.Add(itemToAdd);
        OnInventoryChanged?.Invoke();
        Debug.Log($"[InventoryDataManager] 아이템 추가: {itemToAdd.name}");
    }

    // (2번) 기어를 추가합니다. (이름이 AddGear -> AddItem으로 변경됨)
    public void AddItem(GearData gearToAdd)
    {
        if (gearToAdd == null) return;
        PlayerGears.Add(gearToAdd);
        OnGearsChanged?.Invoke();
        Debug.Log($"[InventoryDataManager] 기어 추가: {gearToAdd.name}");
    }

    // (3번) 랜턴 기능을 추가합니다. (이름이 AddLanternFunction -> AddItem으로 변경됨)    
    public void AddItem(LanternFunctionData functionToAdd)
    {
        if (functionToAdd == null) return;
        PlayerLanternFunctions.Add(functionToAdd);
        OnLanternsChanged?.Invoke();
        Debug.Log($"[InventoryDataManager] 랜턴 기능 추가: {functionToAdd.name}");
    }

    public void AddItem(RecordData recordToAdd)
    {
        if(recordToAdd == null) return;

        if(!PlayerRecords.Contains(recordToAdd))
        {
            PlayerRecords.Add(recordToAdd);
            OnRecordsChanged!?.Invoke();
            Debug.Log($"[InventoryDataManager] 기록물 추가 : {recordToAdd.name}");
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

    //[캐릭터 담당자용 API] 현재 '장착 중'인 모든 기어 리스트를 반환합니다.
    public List<GearData> GetEquippedGears()
    {
        //LINQ를 사용해 PlayerGears 리스트에서 isEquipped == true인 모든 항목을 찾아 새 리스트로 반환
        return PlayerGears.Where(gear => gear != null && gear.isEquipped).ToList();
    }

    //[캐릭터 담당자용 API] 현재 '장착 중'인 랜턴 기능을 반환합니다. (최대 1개)
    public LanternFunctionData GetEquippedLanternFunction()
    {
        //LINQ를 사용해 isEquipped == true인 첫 번째 항목을 반환 (없으면 null)
        return PlayerLanternFunctions.FirstOrDefault(func => func != null && func.isEquipped);
    }

    #region 테스트용 코드

    /// <summary>
    [Button("Test: 현재 장착 중인 모든 아이템 출력")]
    private void PrintEquippedItems()
    {
        Debug.Log("--- 현재 장착 중인 아이템 목록 ---");

        // 1. 장착된 기어 확인
        List<GearData> equippedGears = GetEquippedGears();
        if (equippedGears.Count > 0)
        {
            foreach (GearData gear in equippedGears)
            {
                Debug.Log($"[기어] {gear.gearName} (코스트: {gear.cost})");
            }
        }
        else
        {
            Debug.Log("[기어] 장착된 기어 없음.");
        }

        // 2. 장착된 랜턴 확인
        LanternFunctionData equippedLantern = GetEquippedLanternFunction();
        if (equippedLantern != null)
        {
            Debug.Log($"[랜턴] {equippedLantern.functionName}");
        }
        else
        {
            Debug.Log("[랜턴] 장착된 랜턴 기능 없음.");
        }
        
        Debug.Log("------------------------------------");
    }

    #endregion
}