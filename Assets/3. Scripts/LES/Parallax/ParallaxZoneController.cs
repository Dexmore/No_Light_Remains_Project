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

        // [핵심 수정 1] 오브젝트가 꺼질 때(스테이지 이동 등) 상태를 강제로 정리함
        private void OnDisable()
        {
            if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = null;
            
            // 꺼질 때는 배경도 같이 확실하게 꺼줌 (다시 켜질 때 꼬임 방지)
            // 단, isStartingZone 로직과 충돌하지 않게 주의 (보통 스테이지 꺼지면 배경도 꺼지는게 맞음)
            if (backgroundRoot != null)
            {
                // 페이드 없이 즉시 끔
                ForceState(false, 0f); 
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // [안전 장치] 내가 켜져있을 때만 로직 수행
            if (!isActiveAndEnabled) return;

            if (collision.CompareTag(targetTag) && backgroundRoot != null)
            {
                // 일단 켜고 본다
                backgroundRoot.SetActive(true);
                ToggleScripts(true);

                if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
                
                float duration = enableTransitions ? fadeInDuration : 0f;
                currentFadeRoutine = StartCoroutine(FadeRoutine(1f, duration));
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // [핵심 수정 2] 오류 해결 부분
            // 만약 나가는 순간 오브젝트가 꺼졌다면, 코루틴 시작하지 말고 그냥 즉시 꺼버림
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

        // 상태 강제 적용 함수 (코루틴 없이 즉시 적용)
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