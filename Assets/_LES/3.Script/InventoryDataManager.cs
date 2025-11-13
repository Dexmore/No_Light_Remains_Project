using UnityEngine;
using System.Collections.Generic;
using System; // C# Event (Action)을 사용하기 위해

/// <summary>
/// 플레이어의 모든 인벤토리 관련 데이터를 소유하고 관리하는 중앙 싱글톤 매니저.
/// 몬스터, 상점 등 모든 외부 시스템은 이 매니저의 공개 함수(API)만을 호출해야 합니다.
/// </summary>
public class InventoryDataManager : MonoBehaviour
{
    // 1. 싱글톤 패턴 (어디서든 InventoryDataManager.Instance로 접근)
    public static InventoryDataManager Instance { get; private set; }

    // 2. 실제 플레이어 데이터 (ScriptableObject 원본을 참조)
    [Header("플레이어 소유 데이터")]
    public List<ItemData> PlayerItems = new List<ItemData>();
    public List<GearData> PlayerGears = new List<GearData>();
    public List<LanternFunctionData> PlayerLanternFunctions = new List<LanternFunctionData>();

    [Header("플레이어 스탯")]
    public int PlayerMoney = 0;
    public int MaxGearCost = 3; // (기본 3개 시작, 상점에서 6까지 확장)

    // 3. UI에게 "데이터 변경됨!"을 알릴 C# 이벤트 (방송)
    public event Action OnInventoryChanged; // 소지템/재료/돈 변경 시
    public event Action OnGearsChanged;       // 기어/기어코스트 변경 시
    public event Action OnLanternsChanged;  // 랜턴 기능 변경 시

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

    /// <summary>
    /// (1번) 소지템/재료를 추가합니다. (기존과 동일)
    /// </summary>
    public void AddItem(ItemData itemToAdd)
    {
        if (itemToAdd == null) return;
        PlayerItems.Add(itemToAdd);
        OnInventoryChanged?.Invoke();
        Debug.Log($"[InventoryDataManager] 아이템 추가: {itemToAdd.name}");
    }

    /// <summary>
    /// (2번) 기어를 추가합니다. (이름이 AddGear -> AddItem으로 변경됨)
    /// </summary>
    public void AddItem(GearData gearToAdd)
    {
        if (gearToAdd == null) return;
        PlayerGears.Add(gearToAdd);
        OnGearsChanged?.Invoke();
        Debug.Log($"[InventoryDataManager] 기어 추가: {gearToAdd.name}");
    }

    /// <summary>
    /// (3번) 랜턴 기능을 추가합니다. (이름이 AddLanternFunction -> AddItem으로 변경됨)
    /// </summary>
    public void AddItem(LanternFunctionData functionToAdd)
    {
        if (functionToAdd == null) return;
        PlayerLanternFunctions.Add(functionToAdd);
        OnLanternsChanged?.Invoke();
        Debug.Log($"[InventoryDataManager] 랜턴 기능 추가: {functionToAdd.name}");
    }

    // --- (이하 RemoveItem, PurchaseGearCostSlot, AddMoney 함수는 기존과 동일) ---
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
}