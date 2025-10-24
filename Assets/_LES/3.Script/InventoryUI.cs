using UnityEngine;

namespace Project.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class InventoryUI : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            //초기 상태는 비활성화
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            gameObject.SetActive(false); 
        }

        //UI를 열 때 호출할 함수 (즉시 활성화)
        public void Open()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
        }

        //UI를 닫을 때 호출할 함수 (즉시 비활성화)
        public void Close()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }
    }
}