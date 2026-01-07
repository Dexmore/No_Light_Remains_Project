using UnityEngine;
using TMPro;
using System.Collections;
using System.Text;

public class BootTerminal : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI terminalText; // 녹색 글씨가 나올 텍스트
    [SerializeField] private GameObject bootPanel;         // 부팅 패널 (CRT 매테리얼 적용된 놈)

    [Header("타이핑 설정")]
    [SerializeField] private float typeSpeed = 0.03f;      // 글자당 속도 (낮을수록 빠름)
    [SerializeField] private float lineDelay = 0.15f;      // 줄바꿈 대기 시간
    [SerializeField] private int binaryLines = 8;          // 2진수 출력 줄 수

    private System.Action _onCompleteCallback;

    // 출력할 시스템 로그 (원하는 내용으로 수정하세요)
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
        
        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        StringBuilder sb = new StringBuilder();

        // 1. 2진수 데이터가 빠르게 쏟아지는 연출 (해킹 느낌)
        WaitForSeconds binarySpeed = new WaitForSeconds(0.01f);
        for (int i = 0; i < binaryLines; i++)
        {
            string binaryLine = GenerateRandomBinary(32); 
            sb.AppendLine(binaryLine);
            
            // 전체 텍스트 갱신 (빠르게)
            terminalText.text = sb.ToString();
            yield return binarySpeed;
        }
        
        sb.AppendLine("--------------------------------");
        terminalText.text = sb.ToString();
        yield return new WaitForSeconds(lineDelay);

        // 2. 로그 메시지 한 글자씩 타이핑 (핵심 연출)
        foreach (string log in systemLogs)
        {
            sb.Append("> "); // 프롬프트 문자
            
            // 한 글자씩 추가
            foreach (char c in log)
            {
                sb.Append(c);
                terminalText.text = sb.ToString() + "_"; // 끝에 커서(_) 깜빡임 효과 흉내
                
                // 특수 문자는 조금 더 천천히 (리얼함 추가)
                if (c == '.') yield return new WaitForSeconds(typeSpeed * 4f);
                else yield return new WaitForSeconds(typeSpeed);
            }
            
            sb.AppendLine(); // 줄바꿈
            terminalText.text = sb.ToString();
            
            // 특정 구간에서 뜸 들이기
            if(log.Contains("LOADING") || log.Contains("CONNECTING"))
                yield return new WaitForSeconds(lineDelay * 5f);
            else
                yield return new WaitForSeconds(lineDelay);
        }

        // 3. 완료 대기
        terminalText.text = sb.ToString(); // 커서 제거
        yield return new WaitForSeconds(0.5f);

        // 4. 종료
        bootPanel.SetActive(false);
        _onCompleteCallback?.Invoke();
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