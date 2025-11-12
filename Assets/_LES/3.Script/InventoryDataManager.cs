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
            
            // TODO: (나중에) 여기서 저장된 게임 데이터를 불러옵니다. (예: LoadData())
        }
    }

    // --- 4. [핵심] 팀원들이 호출할 공개 API (AddItem) ---

    /// <summary>
    /// (1번) 몬스터 드롭 등으로 소지템/재료를 추가합니다.
    /// </summary>
    public void AddItem(ItemData itemToAdd)
    {
        if (itemToAdd == null) return;
        
        // TODO: (중요) ScriptableObject는 '원본'이므로, '복사본'을 만들어 추가해야 합니다.
        // ItemData newItemInstance = Instantiate(itemToAdd);
        // PlayerItems.Add(newItemInstance);
        
        // (지금은 간단하게 원본 참조로 추가)
        PlayerItems.Add(itemToAdd);
        
        // "소지템 UI야, 갱신해!"라고 방송(Invoke)
        OnInventoryChanged?.Invoke();
        Debug.Log($"[InventoryDataManager] 아이템 추가: {itemToAdd.name}");
    }

    /// <summary>
    /// (2번) 보스 드롭 등으로 랜턴 기능을 추가합니다.
    /// </summary>
    public void AddLanternFunction(LanternFunctionData functionToAdd)
    {
        if (functionToAdd == null) return;
        PlayerLanternFunctions.Add(functionToAdd);
        OnLanternsChanged?.Invoke();
    }

    /// <summary>
    /// (3번) 드롭 등으로 기어를 추가합니다.
    /// </summary>
    public void AddGear(GearData gearToAdd)
    {
        if (gearToAdd == null) return;
        PlayerGears.Add(gearToAdd);
        OnGearsChanged?.Invoke();
    }

    // --- 5. (핵심) 아이템 제거 API ---
    public void RemoveItem(ItemData itemToRemove)
    {
        if (itemToRemove == null) return;
        
        if (PlayerItems.Remove(itemToRemove))
        {
            OnInventoryChanged?.Invoke(); // 제거 성공 시 갱신
        }
    }
    
    // (기어/랜턴 제거 함수도 동일하게 만드시면 됩니다)
    
    // --- 6. (3번) 기어 코스트 구매 API ---
    public bool PurchaseGearCostSlot(int cost)
    {
        if (PlayerMoney >= cost && MaxGearCost < 6)
        {
            PlayerMoney -= cost;
            MaxGearCost++;
            OnGearsChanged?.Invoke();       // 기어 UI 갱신 방송
            OnInventoryChanged?.Invoke();   // 돈 UI 갱신 방송
            return true; // 구매 성공
        }
        return false; // 구매 실패
    }
    
    public void AddMoney(int amount)
    {
        PlayerMoney += amount;
        OnInventoryChanged?.Invoke();
    }
}