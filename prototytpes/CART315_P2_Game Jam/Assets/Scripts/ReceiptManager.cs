using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

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

    [Header("Receipt Panel")]
    public GameObject receiptPanel;
    public TMP_Text receiptBody;

    private readonly List<string> purchaseLines = new List<string>();

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
            purchaseLines.Add(
        productName + "    $" +
        (priceCents / 100f).ToString("0.00")
        );

        if (TotalCents > budgetCents)
        {
            IsGameOver = true;
        }

        UpdateUI();

        if (IsGameOver)
        {
            ShowReceipt();
        }

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
        ShowReceipt();
    }

    public void ResetReceipt()
    {
        TotalCents = 0;
        ItemCount = 0;

        IsGameOver = false;
        HasWon = false;

        purchaseLines.Clear();

        if (receiptBody != null)
        {
            receiptBody.text = "";
        }

        if (receiptPanel != null)
        {
            receiptPanel.SetActive(false);
        }

        UpdateUI();

        FoodSpawner spawner = FindFirstObjectByType<FoodSpawner>();

        if (spawner != null)
        {
            spawner.StartRun();
        }
    }

    private void RestartGame()
    {
        Snake snake = FindFirstObjectByType<Snake>();

        if (snake != null)
        {
            snake.ResetState();
        }
    }

    private void ShowReceipt()
    {
        StringBuilder receipt = new StringBuilder();

        receipt.AppendLine("Inspired by everyday shopping receipts");
        receipt.AppendLine("--------------------------------");
        receipt.AppendLine();

        for (int i = 0; i < purchaseLines.Count; i++)
        {
            receipt.AppendLine(
                (i + 1) + ". " + purchaseLines[i]
            );
        }

        receipt.AppendLine();
        receipt.AppendLine("--------------------------------");
        receipt.AppendLine("ITEMS: " + ItemCount);

        receipt.AppendLine(
            "TOTAL: $" + (TotalCents / 100f).ToString("0.00")
        );

        receipt.AppendLine(
            "BUDGET: $" + (budgetCents / 100f).ToString("0.00")
        );

        int remainingCents = budgetCents - TotalCents;

        if (remainingCents >= 0)
        {
            receipt.AppendLine(
                "REMAINING: $" +
                (remainingCents / 100f).ToString("0.00")
            );
        }
        else
        {
            receipt.AppendLine(
                "OVER BUDGET BY: $" +
                (-remainingCents / 100f).ToString("0.00")
            );
        }

        receipt.AppendLine();
        receipt.AppendLine(
            HasWon ? "THANK YOU FOR SHOPPING!" : "TOO MUCH SHOPPING!"
        );

        receipt.AppendLine("Press R to shop again.");

        if (receiptBody != null)
        {
            receiptBody.text = receipt.ToString();
        }

        if (receiptPanel != null)
        {
            receiptPanel.SetActive(true);
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
                    "Checkout is OPEN! Head to the cashier.";
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