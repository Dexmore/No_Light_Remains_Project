using UnityEngine;
using System.Text; // StringBuilder를 사용하기 위해 추가
using Random = UnityEngine.Random; // Unity의 Random 함수 사용
using TMPro;
using NaughtyAttributes;
using System.Threading.Tasks;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] public TMP_Text tMP_Text;

    private const float GlitchDuration = 0.5f;
    private const int GlitchDelayMs = 30;

    // 글리치 문자 후보 (태그 기호 < > 제외)
    private readonly string[] GlitchChars =
        { "#", "$", "%", "@", "^", "&", "*", "!", "?", "█", "■", "░", "~", "_", "=" };

    [Button]
    public async void GlitchText()
    {
        if (tMP_Text == null)
        {
            Debug.LogError("TMP_Text가 할당되지 않았습니다.");
            return;
        }

        string originalText = tMP_Text.text;
        int totalIterations = (int)(GlitchDuration * 1000f / GlitchDelayMs);

        for (int i = 0; i < totalIterations; i++)
        {
            await Task.Delay(GlitchDelayMs);

            // 글리치 문자열 생성 (태그가 출력되지 않도록 보장)
            tMP_Text.text = GenerateGlitchString(originalText);
        }

        // 효과 완료 후 원본 텍스트로 복구
        tMP_Text.text = originalText;
    }


    


    private string GenerateGlitchString(string original)
    {
        if (string.IsNullOrEmpty(original))
        {
            return "";
        }
        StringBuilder sb = new StringBuilder(original.Length * 3);
        string[] InsertChars = new string[] { "*", "░", "█" };
        for (int i = 0; i < original.Length; i++)
        {
            char originalChar = original[i];
            if (Random.value < 0.1f)
            {
                int insertCount = Random.Range(1, 4);
                for (int k = 0; k < insertCount; k++)
                {
                    sb.Append(InsertChars[Random.Range(0, InsertChars.Length)]);
                }
            }
            if (Random.value < 0.6f)
            {
                if (Random.value < 0.3f)
                {
                    sb.Append(GlitchChars[Random.Range(0, GlitchChars.Length)]);
                }
                else
                {
                    Color randomColor = new Color(Random.value, Random.value, Random.value);
                    string colorHex = ColorUtility.ToHtmlStringRGB(randomColor);
                    sb.Append($"<color=#{colorHex}>{originalChar}</color>");
                }
            }
            else
            {
                sb.Append(originalChar);
            }
        }
        return sb.ToString();
    }







}