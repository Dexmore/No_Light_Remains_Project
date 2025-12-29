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

        private void OnDisable()
        {
            if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = null;
            
            // 꺼질 때는 배경도 같이 끔
            if (backgroundRoot != null)
            {
                ForceState(false, 0f); 
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 들어올 때도 안전하게 검사
            if (!this.isActiveAndEnabled) return;

            if (collision.CompareTag(targetTag) && backgroundRoot != null)
            {
                backgroundRoot.SetActive(true);
                ToggleScripts(true);

                if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
                
                float duration = enableTransitions ? fadeInDuration : 0f;
                // 여기도 실행 직전 활성 상태라면 실행
                if (this.gameObject.activeInHierarchy)
                {
                    currentFadeRoutine = StartCoroutine(FadeRoutine(1f, duration));
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // 1차 방어: 들어오자마자 꺼져있는지 확인
            if (!this.isActiveAndEnabled) 
            {
                ForceState(false, 0f);
                return;
            }

            if (collision.CompareTag(targetTag) && backgroundRoot != null)
            {
                if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);

                float duration = enableTransitions ? fadeOutDuration : 0f;

                // [핵심 수정] 2차 방어: 코루틴 시작 직전에 "진짜 켜져있니?" 한 번 더 물어봄
                // 스테이지 이동 시, 위에서 검사 통과 후 여기까지 오는 사이에 꺼질 수 있음
                if (this.gameObject.activeInHierarchy)
                {
                    currentFadeRoutine = StartCoroutine(FadeRoutine(0f, duration, true));
                }
                else
                {
                    // 꺼져있다면 코루틴 대신 강제로 끔
                    ForceState(false, 0f);
                }
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