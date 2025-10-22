using UnityEngine;
using UnityEngine.InputSystem; // 새로운 Input System을 사용하기 위해 추가

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
        private Key toggleKey = Key.I; // KeyCode에서 Key로 변경

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
            // Keyboard.current가 null이 아닌지 확인하여 키보드가 연결되어 있는지 체크합니다.
            if (Keyboard.current == null) return;
            
            // Input.GetKeyDown(toggleKey) 대신 새로운 Input System 방식으로 키 입력 확인
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
    }
}