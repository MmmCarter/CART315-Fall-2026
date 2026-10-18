using TMPro;
using UnityEngine;

public class ReceiptManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text totalText;
    public TMP_Text itemsText;

    public int TotalCents { get; private set; }
    public int ItemCount { get; private set; }

    private void Start()
    {
        UpdateUI();
    }

    public void AddPurchase(string productName, int priceCents)
    {
        TotalCents += priceCents;
        ItemCount++;

        UpdateUI();

        Debug.Log(
            "Added to receipt: " + productName +
            " | Total: $" + (TotalCents / 100f).ToString("0.00")
        );
    }

    public void ResetReceipt()
    {
        TotalCents = 0;
        ItemCount = 0;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (totalText != null)
        {
            totalText.text =
                "TOTAL: $" + (TotalCents / 100f).ToString("0.00");
        }

        if (itemsText != null)
        {
            itemsText.text = "ITEMS: " + ItemCount;
        }
    }
}