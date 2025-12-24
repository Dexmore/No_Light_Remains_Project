using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Visuals
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ParallaxZoneController : MonoBehaviour
    {
        [Header("1. Target Group")]
        [Tooltip("제어할 배경 그룹의 부모 오브젝트")]
        public GameObject backgroundRoot;

        [Header("2. Settings")]
        [Tooltip("시작 구역 여부 (체크하면 처음부터 켜짐)")]
        public bool isStartingZone = false;

        [Tooltip("감지할 플레이어 태그")]
        public string targetTag = "Player";

        [Header("3. Transition Timing")]
        [Tooltip("체크 해제하면 페이드 효과 없이 즉시(0초) 전환됩니다.")]
        public bool enableTransitions = true; // [New] 트랜지션 ON/OFF 버튼

        [Tooltip("켜지는 시간 (Fade In)")]
        public float fadeInDuration = 0.5f;

        [Tooltip("꺼지는 시간 (Fade Out)")]
        public float fadeOutDuration = 1.0f;

        private List<SpriteRenderer> targetSprites = new List<SpriteRenderer>();
        private List<ParallaxMaster> targetScripts = new List<ParallaxMaster>();
        private Coroutine currentFadeRoutine;

        private void Start()
        {
            if (backgroundRoot == null) return;

            // 최적화: 제어 대상 미리 캐싱
            backgroundRoot.GetComponentsInChildren<SpriteRenderer>(true, targetSprites);
            backgroundRoot.GetComponentsInChildren<ParallaxMaster>(true, targetScripts);

            GetComponent<BoxCollider2D>().isTrigger = true;

            if (isStartingZone)
            {
                backgroundRoot.SetActive(true);
                ToggleScripts(true);
                SetAlpha(1f);
            }
            else
            {
                SetAlpha(0f);
                ToggleScripts(false);
                backgroundRoot.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(targetTag) && backgroundRoot != null)
            {
                backgroundRoot.SetActive(true);
                ToggleScripts(true);

                if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
                
                // [핵심 변경] 버튼이 꺼져있으면 시간 0초 적용
                float duration = enableTransitions ? fadeInDuration : 0f;
                currentFadeRoutine = StartCoroutine(FadeRoutine(1f, duration));
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag(targetTag) && backgroundRoot != null)
            {
                if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);

                // [핵심 변경] 버튼이 꺼져있으면 시간 0초 적용
                float duration = enableTransitions ? fadeOutDuration : 0f;
                currentFadeRoutine = StartCoroutine(FadeRoutine(0f, duration, true));
            }
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, bool disableAfter = false)
        {
            // 시간이 0 이하라면 즉시 처리 (깜빡임 방지)
            if (duration <= 0f)
            {
                SetAlpha(targetAlpha);
                if (disableAfter)
                {
                    ToggleScripts(false);
                    backgroundRoot.SetActive(false);
                }
                yield break; // 코루틴 즉시 종료
            }

            float timer = 0f;
            float startAlpha = targetSprites.Count > 0 ? targetSprites[0].color.a : 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
                SetAlpha(newAlpha);
                yield return null;
            }
            
            SetAlpha(targetAlpha);

            if (disableAfter)
            {
                ToggleScripts(false);
                backgroundRoot.SetActive(false);
            }
        }

        private void SetAlpha(float alpha)
        {
            foreach (var sprite in targetSprites)
            {
                if(sprite != null)
                {
                    Color c = sprite.color;
                    c.a = alpha;
                    sprite.color = c;
                }
            }
        }

        private void ToggleScripts(bool enable)
        {
            foreach (var script in targetScripts)
            {
                if (script != null) script.enabled = enable;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position, transform.localScale);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}