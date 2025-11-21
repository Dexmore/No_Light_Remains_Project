using UnityEngine;
using System.Collections.Generic;
using System;
using NaughtyAttributes;
using System.Linq; // NaughtyAttributes 사용

public class InventoryDataManager : MonoBehaviour
{
    public static InventoryDataManager Instance { get; private set; }

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