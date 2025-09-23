using UnityEngine;
using UnityEngine.UI;

public class LighthouseBar : MonoBehaviour
{
    [Header("Assign Lighthouse Fill Image (TopBar/Lighthouse_Gauge/Fill)")]
    public Image fillImage;

    public void Set01(float normalized) // 0~1
    {
        if (!fillImage) return;
        fillImage.fillAmount = Mathf.Clamp01(normalized);
    }
}
