using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    [Header("Assign TMP (TopBar/Currency_Display/Text)")]
    public TextMeshProUGUI amountText;

    public void SetAmount(int value)
    {
        if (!amountText) return;
        amountText.text = value.ToString();
    }
}
