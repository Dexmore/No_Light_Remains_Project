using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Assign HP Fill Image (TopBar/HP_Bar/Fill)")]
    public Image hpFillImage;

    public void Set01(float normalized) // 0~1
    {
        if (!hpFillImage) return;
        hpFillImage.fillAmount = Mathf.Clamp01(normalized);
    }
}
