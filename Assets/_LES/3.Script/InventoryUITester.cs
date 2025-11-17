using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;           // [추가] 테스트 버튼용
using System.Collections.Generic;
using Project.UI;    // [추가] List<> 사용

// 이 스크립트는 오직 테스트 목적으로만 사용됩니다.
namespace YourProject.UI.Test
{
    public class InventoryUITester : MonoBehaviour
    {
        [Header("제어할 UI")]
        [Tooltip("InventoryPanels 오브젝트에 붙어있는 InventoryUI 스크립트를 연결하세요.")]
        [SerializeField]
        private InventoryUI inventoryUI;

        [Header("테스트 설정")]
        [Tooltip("인벤토리를 열고 닫을 테스트용 키입니다.")]
        [SerializeField]
        private Key toggleKey = Key.I;

        [Header("일괄 아이템 추가 테스트")]
        [Tooltip("Test 버튼을 누르면 이 리스트의 모든 아이템이 InventoryDataManager에 추가됩니다.")]
        [SerializeField]
        private List<ItemData> testItemsToAdd;

        [SerializeField]
        private List<GearData> testGearsToAdd;

        [SerializeField]
        private List<LanternFunctionData> testLanternsToAdd;

        [SerializeField]
        private List<RecordData> testRecordsToAdd;

        [SerializeField]
        private int testMoneyToAdd;
        
        // ------------------------------

        private void Awake()
        {
            if (inventoryUI == null)
            {
                Debug.LogError("[InventoryUITester] 제어할 InventoryUI가 할당되지 않았습니다! Inspector에서 연결해주세요.");
                this.enabled = false;
            }
        }

        private void Update()
        {
            if (Keyboard.current == null) return;
            
            if (Keyboard.current[toggleKey].wasPressedThisFrame)
            {
                if (inventoryUI.gameObject.activeInHierarchy)
                {
                    inventoryUI.Close();
                }
                else
                {
                    inventoryUI.Open();
                }
            }
        }

        // [추가] NaughtyAttributes로 만든 테스트 버튼입니다.
        [Button("Test: 모든 테스트 아이템 일괄 추가")]
        private void AddAllTestItems()
        {
            if (InventoryDataManager.Instance == null)
            {
                Debug.LogError("[InventoryUITester] InventoryDataManager가 씬에 없습니다! 테스트 실패.");
                return;
            }

            int count = 0;

            // 1. 소지템/재료를 모두 추가
            foreach (ItemData item in testItemsToAdd)
            {
                InventoryDataManager.Instance.AddItem(item);
                count++;
            }

            // 2. 기어를 모두 추가 (AddItem 마스터 메서드가 알아서 분류)
            foreach (GearData gear in testGearsToAdd)
            {
                InventoryDataManager.Instance.AddItem(gear);
                count++;
            }

            // 3. 랜턴 기능을 모두 추가 (AddItem 마스터 메서드가 알아서 분류)
            foreach (LanternFunctionData lantern in testLanternsToAdd)
            {
                InventoryDataManager.Instance.AddItem(lantern);
                count++;
            }

            foreach (RecordData record in testRecordsToAdd)
            {
                InventoryDataManager.Instance.AddItem(record);
                count++;
            }

            InventoryDataManager.Instance.AddMoney(testMoneyToAdd);
            Debug.Log($"[InventoryUITester] 총 {count}개의 아이템을 InventoryDataManager에 성공적으로 추가했습니다.");
        }
    }
}