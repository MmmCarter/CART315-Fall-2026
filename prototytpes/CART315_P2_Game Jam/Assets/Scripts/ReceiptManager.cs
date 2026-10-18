using TMPro;
using UnityEngine;

public class ReceiptManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text totalText;
    public TMP_Text itemsText;
    public TMP_Text budgetText;
    public TMP_Text statusText;

    [Header("Game Rules")]
    public int budgetCents = 3000;

    public int TotalCents { get; private set; }
    public int ItemCount { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void AddPurchase(string productName, int priceCents)
    {
        if (IsGameOver)
        {
            return;
        }

        TotalCents += priceCents;
        ItemCount++;

        if (TotalCents > budgetCents)
        {
            IsGameOver = true;
        }

        UpdateUI();

        Debug.Log(
            "Purchased: " + productName +
            " | Total: $" + (TotalCents / 100f).ToString("0.00")
        );
    }

    public void ResetReceipt()
    {
        TotalCents = 0;
        ItemCount = 0;
        IsGameOver = false;

        UpdateUI();
    }

    private void RestartGame()
    {
        Snake snake = FindFirstObjectByType<Snake>();

        snake.ResetState();

        Food food = FindFirstObjectByType<Food>();

        if (food != null)
        {
            food.RandomizePosition();
        }
    }

    private void UpdateUI()
    {
        if (totalText != null)
        {
            totalText.text =
                "TOTAL: $" + (TotalCents / 100f).ToString("0.00");

            totalText.color = IsGameOver
                ? new Color(1f, 0.4f, 0.4f)
                : new Color(0.96f, 0.93f, 0.86f);
        }

        if (itemsText != null)
        {
            itemsText.text = "ITEMS: " + ItemCount;
        }

        if (budgetText != null)
        {
            budgetText.text =
                "BUDGET: $" + (budgetCents / 100f).ToString("0.00");
        }

        if (statusText != null)
        {
            statusText.text = IsGameOver
                ? "OVER BUDGET! Press R to restart."
                : "Collect products. Stay within budget.";
        }
    }
}