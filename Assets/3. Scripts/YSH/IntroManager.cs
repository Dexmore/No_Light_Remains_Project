using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro를 사용하기 위해 필요
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 필요

public class IntroManager : MonoBehaviour
{
    [Header("UI Components - Text Lines")]
    // Inspector에서 연결할 개별 TextMeshPro 컴포넌트들의 리스트
    // 이 리스트의 순서대로 텍스트가 나타납니다.
    [SerializeField] private List<TextMeshProUGUI> introTexts = new List<TextMeshProUGUI>();

    [Header("Timing Settings")]
    [SerializeField] private float fadeDuration = 1.0f;       // 각 텍스트가 나타나는(Fade In) 시간
    [SerializeField] private float timeBetweenLines = 0.5f;   // 텍스트가 나타난 후 다음 텍스트를 기다리는 시간
    [SerializeField] private float finalWaitTime = 2.0f;      // 모든 텍스트 표시 후 다음 씬으로 넘어가기 전 대기 시간
    [SerializeField] private string nextSceneName = "Stage0"; // 다음으로 넘어갈 씬 이름

    void Start()
    {
        // 씬 시작 시 모든 텍스트를 투명하게 설정하여 숨깁니다.
        foreach (var textComponent in introTexts)
        {
            if (textComponent != null)
            {
                // 색상의 Alpha(투명도) 값을 0으로 설정하여 숨깁니다.
                Color initialColor = textComponent.color;
                initialColor.a = 0f;
                textComponent.color = initialColor;
            }
        }

        // 인트로 프로세스 시작
        StartCoroutine(IntroProcess());
    }

    private IEnumerator IntroProcess()
    {
        // 리스트에 있는 각 Text 컴포넌트를 순서대로 처리
        foreach (var textComponent in introTexts)
        {
            if (textComponent != null)
            {
                // 1. 텍스트를 부드럽게 나타나게 함 (Fade In)
                yield return StartCoroutine(FadeText(textComponent, 1f, fadeDuration));

                // 2. 다음 텍스트를 표시하기 전까지 잠시 대기
                yield return new WaitForSeconds(timeBetweenLines);
                // 이미 나타난 텍스트는 Alpha=1f 상태이므로 화면에 계속 남아 있습니다.
            }
        }

        // 3. 모든 텍스트가 다 표시된 후, 다음 씬으로 넘어가기 전 마지막 대기
        yield return new WaitForSeconds(finalWaitTime);
        
        // 4. 씬 전환
        DBManager.I.currData.sceneName = nextSceneName;
        yield return null;
        DBManager.I.Save();
        yield return null;
        GameManager.I.LoadSceneAsync(nextSceneName, true);
    }

    // 특정 TextMeshPro 컴포넌트의 투명도를 변경하는 Coroutine
    private IEnumerator FadeText(TextMeshProUGUI textComponent, float targetAlpha, float duration)
    {
        float startTime = Time.time;
        Color startColor = textComponent.color;
        Color endColor = startColor;
        endColor.a = targetAlpha; // 목표 투명도 (이 경우 1f)

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            // Lerp를 사용하여 시작 색상에서 끝 색상으로 부드럽게 보간
            textComponent.color = Color.Lerp(startColor, endColor, t);
            yield return null; // 다음 프레임까지 대기
        }

        // Coroutine 종료 시, 정확히 목표 투명도에 도달하도록 보장
        textComponent.color = endColor;
    }
}