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
    public int requiredItems = 5;

    public int TotalCents { get; private set; }
    public int ItemCount { get; private set; }

    public bool IsGameOver { get; private set; }
    public bool HasWon { get; private set; }

    public bool CanCheckout =>
        !IsGameOver &&
        ItemCount >= requiredItems &&
        TotalCents <= budgetCents;

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

    public void TryCheckout()
    {
        if (!CanCheckout)
        {
            return;
        }

        HasWon = true;
        IsGameOver = true;

        UpdateUI();
    }

    public void ResetReceipt()
    {
        TotalCents = 0;
        ItemCount = 0;
        IsGameOver = false;
        HasWon = false;

        UpdateUI();
    }

    private void RestartGame()
    {
        ResetReceipt();

        Snake snake = FindFirstObjectByType<Snake>();

        if (snake != null)
        {
            snake.ResetState();
        }

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

            if (HasWon)
            {
                totalText.color = new Color(0.4f, 0.85f, 0.55f);
            }
            else if (IsGameOver)
            {
                totalText.color = new Color(1f, 0.4f, 0.4f);
            }
            else
            {
                totalText.color = new Color(0.96f, 0.93f, 0.86f);
            }
        }

        if (itemsText != null)
        {
            itemsText.text =
                "ITEMS: " + ItemCount + " / " + requiredItems;
        }

        if (budgetText != null)
        {
            budgetText.text =
                "BUDGET: $" + (budgetCents / 100f).ToString("0.00");
        }

        if (statusText != null)
        {
            if (HasWon)
            {
                statusText.text =
                    "CHECKOUT COMPLETE! Press R to shop again.";
            }
            else if (IsGameOver)
            {
                statusText.text =
                    "OVER BUDGET! Press R to restart.";
            }
            else if (CanCheckout)
            {
                statusText.text =
                    "Checkout is OPEN! Head to the green square.";
            }
            else
            {
                statusText.text =
                    "Buy " + requiredItems +
                    " items, then reach checkout.";
            }
         }
    }
}