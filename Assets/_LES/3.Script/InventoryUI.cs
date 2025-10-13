using UnityEngine;
using System.Collections;

namespace YourProject.UI
{
    [RequireComponent(typeof(CanvasGroup))] // CanvasGroup이 필수임을 명시
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField]
        [Range(0.1f, 2f)] // Inspector에서 슬라이더로 조절
        private float fadeDuration = 0.3f; 

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            // 초기 상태는 비활성화
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            gameObject.SetActive(false); 
        }

        // UI를 열 때 호출할 함수
        public void Open()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                StartCoroutine(FadeIn());
            }
        }

        // UI를 닫을 때 호출할 함수
        public void Close()
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(FadeOut());
            }
        }

        private IEnumerator FadeIn()
        {
            float timer = 0f;
            _canvasGroup.interactable = false; // 페이드 중에는 조작 불가

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime; // Time.timeScale에 영향받지 않음
                _canvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true; // 페이드 완료 후 조작 가능
        }

        private IEnumerator FadeOut()
        {
            float timer = 0f;
            _canvasGroup.interactable = false;

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false); // 페이드 완료 후 비활성화
        }
    }
}