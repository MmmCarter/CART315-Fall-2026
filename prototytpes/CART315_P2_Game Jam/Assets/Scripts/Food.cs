using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Food : MonoBehaviour
{
    public Collider2D gridArea;

    // Information about the current item, used to generate a shopping list.
    public string ProductName { get; private set; }
    public int PriceCents { get; private set; }

    private Snake snake;
    private TextMesh productLabel;

    private readonly string[] productNames =
    {
        "MILK", "BREAD", "CHIPS", "APPLE", "COFFEE"
    };

    // Use whole numbers to represent cents (350 represents $3.50).
    private readonly int[] productPrices =
    {
        350, 250, 400, 150, 600
    };

    private readonly Color[] productColors =
    {
        new Color(0.65f, 0.82f, 0.92f),
        new Color(0.87f, 0.68f, 0.40f),
        new Color(0.92f, 0.50f, 0.30f),
        new Color(0.60f, 0.80f, 0.35f),
        new Color(0.65f, 0.46f, 0.32f)
    };

    private void Awake()
    {
        snake = FindFirstObjectByType<Snake>();
    }

    private void Start()
    {
        CreateLabel();
        RandomizePosition();
    }

    private void CreateLabel()
    {
        GameObject labelObject = new GameObject("Product Label");
        labelObject.transform.SetParent(transform, false);

        productLabel = labelObject.AddComponent<TextMesh>();
        productLabel.anchor = TextAnchor.MiddleCenter;
        productLabel.alignment = TextAlignment.Center;
        productLabel.fontSize = 48;
        productLabel.characterSize = 0.1f;
        productLabel.color = Color.white;

        labelObject.GetComponent<MeshRenderer>().sortingOrder = 10;
    }

    private void RandomizeProduct()
    {
        int index = Random.Range(0, productNames.Length);

        ProductName = productNames[index];
        PriceCents = productPrices[index];

        GetComponent<SpriteRenderer>().color = productColors[index];

        productLabel.text =
            ProductName + "\n$" + (PriceCents / 100f).ToString("0.00");

        // Place the text below the product to avoid obscuring the border.
        bool nearTop = transform.position.y > gridArea.bounds.max.y - 2f;

        productLabel.transform.localPosition =
            new Vector3(0f, nearTop ? -1.2f : 1.2f, -0.1f);
    }

    public void RandomizePosition()
    {
        Bounds bounds = gridArea.bounds;

        int minX = Mathf.CeilToInt(bounds.min.x);
        int maxX = Mathf.FloorToInt(bounds.max.x);
        int minY = Mathf.CeilToInt(bounds.min.y);
        int maxY = Mathf.FloorToInt(bounds.max.y);

        // Collect the squares not occupied by tickets, and make sure items don't spawn on the snake.
        var freeCells = new System.Collections.Generic.List<Vector2Int>();

        CheckoutPoint checkout = FindFirstObjectByType<CheckoutPoint>();

        Collider2D checkoutCollider = checkout != null
            ? checkout.GetComponent<Collider2D>()
            : null;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                bool insideCheckout = checkoutCollider != null &&
    checkoutCollider.OverlapPoint(new Vector2(x, y));

                if (!snake.Occupies(x, y) && !insideCheckout)
                {
                    freeCells.Add(new Vector2Int(x, y));
                }
            }
        }

        if (freeCells.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector2Int cell = freeCells[Random.Range(0, freeCells.Count)];
        transform.position = new Vector3(cell.x, cell.y, 0f);

        RandomizeProduct();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Items can only be picked up from the head of the receipt.
        if (other.GetComponent<Snake>() == null)
        {
            return;
        }

        ReceiptManager receipt = FindFirstObjectByType<ReceiptManager>();

        if (receipt != null)
        {
            if (receipt.IsGameOver)
            {
                return;
            }

            receipt.AddPurchase(ProductName, PriceCents);

            // Keep the current item in its position and do not generate the next one if the budget is exceeded.
            if (receipt.IsGameOver)
            {
                return;
            }
        }

        RandomizePosition();
    }

}
