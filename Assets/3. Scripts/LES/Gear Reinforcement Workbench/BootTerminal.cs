using UnityEngine;
using TMPro;
using System.Collections;
using System.Text;

public class BootTerminal : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI terminalText;
    [SerializeField] private GameObject bootPanel;
    
    // [핵심] 사운드는 여기서 직접 안 내고, 부모 UI에게 시킵니다.
    [Header("메인 UI 연결 (사운드 제어용)")]
    [SerializeField] private WorkbenchUI workbenchUI;

    [Header("타이핑 설정")]
    [SerializeField] private float typeSpeed = 0.03f;
    [SerializeField] private float lineDelay = 0.15f;
    [SerializeField] private int binaryLines = 8;

    private System.Action _onCompleteCallback;
    private Coroutine _bootCoroutine;

    private string[] systemLogs = new string[]
    {
        "BIOS DATE 01/15/2098 14:22:51 VER 1.02",
        "CPU: QUANTUM-V20, SPEED: 10 THz",
        "CHECKING MEMORY... OK",
        "LOADING WORKBENCH OS...",
        "INITIALIZING HARDWARE...",
        "CONNECTING TO GEAR SERVER...",
        "ACCESS GRANTED."
    };

    public void PlayBootSequence(System.Action onComplete)
    {
        _onCompleteCallback = onComplete;
        bootPanel.SetActive(true);
        terminalText.text = "";

        // [Sound] 부팅 소리 요청
        if (workbenchUI != null) workbenchUI.PlayBootSound();

        if (_bootCoroutine != null) StopCoroutine(_bootCoroutine);
        _bootCoroutine = StartCoroutine(SequenceRoutine());
    }

    public void StopBootSequence()
    {
        if (_bootCoroutine != null) StopCoroutine(_bootCoroutine);
        
        // [Sound] 루프 소리 끄기 요청
        if (workbenchUI != null) workbenchUI.StopLoopSound();

        bootPanel.SetActive(false);
        terminalText.text = "";
    }

    private IEnumerator SequenceRoutine()
    {
        StringBuilder sb = new StringBuilder();

        // 1. [Sound] 데이터 스크롤 루프 시작 요청
        if (workbenchUI != null) workbenchUI.PlayDataScrollLoop();

        WaitForSeconds binarySpeed = new WaitForSeconds(0.01f);
        for (int i = 0; i < binaryLines; i++)
        {
            string binaryLine = GenerateRandomBinary(32);
            sb.AppendLine(binaryLine);
            terminalText.text = sb.ToString();
            yield return binarySpeed;
        }

        // [Sound] 루프 정지
        if (workbenchUI != null) workbenchUI.StopLoopSound();

        sb.AppendLine("--------------------------------");
        terminalText.text = sb.ToString();
        yield return new WaitForSeconds(lineDelay);

        // 3. 로그 메시지 타이핑
        foreach (string log in systemLogs)
        {
            sb.Append("> ");
            
            // [Sound] 타이핑 루프 시작 요청
            if (workbenchUI != null) workbenchUI.PlayTypingLoop();

            foreach (char c in log)
            {
                sb.Append(c);
                terminalText.text = sb.ToString() + "_";

                if (c == '.' || c == ',')
                {
                    // [Sound] 쉼표에서 소리 일시정지 요청
                    if (workbenchUI != null) workbenchUI.PauseLoopSound();
                    yield return new WaitForSeconds(typeSpeed * 4f);
                    if (workbenchUI != null) workbenchUI.UnPauseLoopSound();
                }
                else
                {
                    yield return new WaitForSeconds(typeSpeed);
                }
            }

            // [Sound] 줄 끝나면 루프 정지
            if (workbenchUI != null) workbenchUI.StopLoopSound();

            sb.AppendLine();
            terminalText.text = sb.ToString();

            if (log.Contains("LOADING") || log.Contains("CONNECTING"))
                yield return new WaitForSeconds(lineDelay * 5f);
            else
                yield return new WaitForSeconds(lineDelay);
        }

        terminalText.text = sb.ToString();
        yield return new WaitForSeconds(0.5f);

        bootPanel.SetActive(false);
        _onCompleteCallback?.Invoke();
        _bootCoroutine = null;
    }

    private string GenerateRandomBinary(int length)
    {
        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = Random.value > 0.5f ? '1' : '0';
            if (i % 8 == 7) chars[i] = ' ';
        }
        return new string(chars);
    }
}