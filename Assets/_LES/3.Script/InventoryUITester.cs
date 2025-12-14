// using UnityEngine;
// using UnityEngine.InputSystem;
// using NaughtyAttributes; // 버튼 생성용
// using System.Collections.Generic;
// using System.IO;
// using Project.UI; // 파일 삭제 테스트용

// namespace YourProject.UI.Test
// {
//     public class InventoryUITester : MonoBehaviour
//     {
//         [Header("제어할 UI")]
//         [SerializeField] private InventoryUI inventoryUI;
//         [SerializeField] private Key toggleKey = Key.I;

//         [Header("테스트 데이터 설정")]
//         [SerializeField] private List<ItemData> testItemsToAdd;
//         [SerializeField] private List<GearData> testGearsToAdd;
//         [SerializeField] private List<LanternFunctionData> testLanternsToAdd;
//         [SerializeField] private List<RecordData> testRecordsToAdd;
//         [SerializeField] private int testMoneyToAdd = 1000;

//         private void Awake()
//         {
//             if (inventoryUI == null) this.enabled = false;
//         }

//         private void Update()
//         {
//             if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
//             {
//                 if (inventoryUI.gameObject.activeInHierarchy) inventoryUI.Close();
//                 else inventoryUI.Open();
//             }
//         }

//         // ========================================================================
//         // [테스트 섹션 1] 데이터 조작 (Adding)
//         // ========================================================================

//         [Button("1. 아이템/돈 일괄 추가 (Add All)")]
//         private void AddAllTestItems()
//         {
//             if (InventoryDataManager.Instance == null) return;

//             // 1. 아이템/기어/랜턴/기록물 추가
//             foreach (var item in testItemsToAdd) InventoryDataManager.Instance.AddItem(item);
//             foreach (var gear in testGearsToAdd) InventoryDataManager.Instance.AddItem(gear);
//             foreach (var lantern in testLanternsToAdd) InventoryDataManager.Instance.AddItem(lantern);
//             foreach (var record in testRecordsToAdd) InventoryDataManager.Instance.AddItem(record);

//             // 2. 돈 추가
//             InventoryDataManager.Instance.AddMoney(testMoneyToAdd);

//             Debug.Log($"[TEST] 아이템들과 돈({testMoneyToAdd})을 추가했습니다.");
//         }

//         // ========================================================================
//         // [테스트 섹션 2] 저장 및 불러오기 (Save / Load)
//         // ========================================================================

//         [Button("2. DB에 저장하기 (Save)")]
//         private void TestSave()
//         {
//             if (InventoryDataManager.Instance == null) return;
            
//             InventoryDataManager.Instance.SaveToDB();
//             Debug.Log("[TEST] 현재 상태를 저장했습니다. (SaveToDB 호출)");
//         }

//         [Button("3. 인벤토리 비우기 (Clear Memory)")]
//         private void TestClearInMemory()
//         {
//             if (InventoryDataManager.Instance == null) return;

//             // 메모리 상의 데이터만 날립니다 (저장 파일은 건드리지 않음)
//             InventoryDataManager.Instance.PlayerItems.Clear();
//             InventoryDataManager.Instance.PlayerGears.Clear();
//             InventoryDataManager.Instance.PlayerLanternFunctions.Clear();
//             InventoryDataManager.Instance.PlayerRecords.Clear();
//             InventoryDataManager.Instance.PlayerMoney = 0;

//             // 강제로 UI 갱신 방송
//             // (실제로는 RemoveItem 등을 써야 하지만 테스트니 강제로 호출)
//             // *주의: InventoryDataManager에 이 이벤트를 public으로 열어두거나, 
//             //  별도 Refresh 함수가 필요할 수 있으나, 여기서는 Load 테스트를 위한 눈속임용입니다.
//             Debug.Log("[TEST] (테스트용) 메모리 상의 인벤토리를 비웠습니다. UI는 갱신되지 않을 수 있습니다.");
//         }

//         [Button("4. DB에서 불러오기 (Load)")]
//         private void TestLoad()
//         {
//             if (InventoryDataManager.Instance == null) return;

//             InventoryDataManager.Instance.LoadFromDB();
//             Debug.Log("[TEST] 저장된 데이터를 다시 불러왔습니다. (LoadFromDB 호출)");
//         }

//         [Button("5. 저장 파일 삭제 (Delete Save File)")]
//         private void DeleteSaveFile()
//         {
//             // DBManager의 경로 로직을 참고하여 경로 추정
//             string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "My Games", "REKINDLE", "SaveData");
            
//             if (File.Exists(path))
//             {
//                 File.Delete(path);
//                 Debug.Log($"[TEST] 저장 파일을 삭제했습니다: {path}");
//             }
//             else
//             {
//                 Debug.LogWarning("[TEST] 삭제할 저장 파일이 없습니다.");
//             }
//         }
//     }
// }