using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Visuals
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ParallaxZoneController : MonoBehaviour
    {
        [Header("1. Target Group")]
        public GameObject backgroundRoot;

        [Header("2. Settings")]
        public bool isStartingZone = false;
        public string targetTag = "Player";

        [Header("3. Transition Timing")]
        public bool enableTransitions = true;
        public float fadeInDuration = 0.5f;
        public float fadeOutDuration = 1.0f;

        private List<SpriteRenderer> targetSprites = new List<SpriteRenderer>();
        private List<ParallaxMaster> targetScripts = new List<ParallaxMaster>();
        private Coroutine currentFadeRoutine;

        private void Awake()
        {
            if (backgroundRoot == null) return;
            // 최적화: 캐싱
            backgroundRoot.GetComponentsInChildren<SpriteRenderer>(true, targetSprites);
            backgroundRoot.GetComponentsInChildren<ParallaxMaster>(true, targetScripts);
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Start()
        {
            // 시작 구역 설정
            if (isStartingZone)
            {
                ForceState(true, 1f);
            }
            else
            {
                ForceState(false, 0f);
            }
        }

        // [핵심] 오브젝트가 꺼질 때 상태를 깔끔하게 정리 (오류 방지)
        private void OnDisable()
        {
            if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = null;
            
            // 꺼질 때는 배경도 같이 끔 (다시 켜질 때 초기화된 상태로 시작하기 위함)
            if (backgroundRoot != null)
            {
                ForceState(false, 0f); 
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isActiveAndEnabled) return;

            if (collision.CompareTag(targetTag) && backgroundRoot != null)
            {
                backgroundRoot.SetActive(true);
                ToggleScripts(true);

                if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
                
                float duration = enableTransitions ? fadeInDuration : 0f;
                currentFadeRoutine = StartCoroutine(FadeRoutine(1f, duration));
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // 나가는 순간 씬 이동 등으로 내가 꺼졌다면 에러 없이 종료
            if (!isActiveAndEnabled) 
            {
                ForceState(false, 0f);
                return;
            }

            if (collision.CompareTag(targetTag) && backgroundRoot != null)
            {
                if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);

                float duration = enableTransitions ? fadeOutDuration : 0f;
                currentFadeRoutine = StartCoroutine(FadeRoutine(0f, duration, true));
            }
        }

        private void ForceState(bool isActive, float alpha)
        {
            if (backgroundRoot == null) return;

            SetAlpha(alpha);
            ToggleScripts(isActive);
            backgroundRoot.SetActive(isActive);
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, bool disableAfter = false)
        {
            if (duration <= 0f)
            {
                SetAlpha(targetAlpha);
                if (disableAfter) ForceState(false, targetAlpha);
                yield break;
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
                ForceState(false, targetAlpha);
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