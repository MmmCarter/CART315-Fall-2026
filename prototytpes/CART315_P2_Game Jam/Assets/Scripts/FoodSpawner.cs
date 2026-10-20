using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ProductDefinition
    {
        public string productName;
        public int priceCents;
        public Sprite sprite;
    }

    [Header("Scene References")]
    public Food foodPrefab;
    public Collider2D gridArea;
    public CheckoutPoint checkout;

    [Header("Products")]
    public ProductDefinition[] products;

    [Header("Normal Shopping")]
    public int minimumProducts = 2;
    public int maximumProducts = 3;

    [Header("Checkout Rush")]
    [Range(0.05f, 0.6f)]
    public float rushDensity = 0.25f;

    public int headSafeRadius = 2;

    private readonly List<Food> activeFoods = new List<Food>();

    private Snake snake;
    private ReceiptManager receipt;
    private bool rushStarted;

    private void Awake()
    {
        snake = FindFirstObjectByType<Snake>();
        receipt = FindFirstObjectByType<ReceiptManager>();
    }

    public void StartRun()
    {
        ClearProducts();
        rushStarted = false;

        if (foodPrefab == null || gridArea == null ||
            products == null || products.Length == 0)
        {
            Debug.LogError(
                "FoodSpawner: assign the prefab, grid and products."
            );
            return;
        }

        SpawnNormalWave();
    }

    public void TryCollect(Food food)
    {
        if (receipt == null || receipt.IsGameOver)
        {
            return;
        }

        if (!activeFoods.Remove(food))
        {
            return;
        }

        food.gameObject.SetActive(false);
        Destroy(food.gameObject);

        snake.Grow();
        receipt.AddPurchase(food.ProductName, food.PriceCents);

        if (receipt.IsGameOver)
        {
            return;
        }

        if (!rushStarted && receipt.CanCheckout)
        {
            rushStarted = true;
            ClearProducts();
            SpawnRush();
        }
        else if (!rushStarted)
        {
            ClearProducts();
            SpawnNormalWave();
        }

    }

    private void SpawnNormalWave()
    {
        List<Vector2Int> cells = GetFreeCells(false);
        Shuffle(cells);

        int count = Random.Range(
            minimumProducts,
            maximumProducts + 1
        );

        SpawnProducts(cells, count);
    }

    private void SpawnRush()
    {
        List<Vector2Int> cells = GetFreeCells(true);
        Shuffle(cells);

        int count = Mathf.RoundToInt(cells.Count * rushDensity);
        SpawnProducts(cells, count);
    }

    private List<Vector2Int> GetFreeCells(bool protectHead)
    {
        var cells = new List<Vector2Int>();

        Bounds bounds = gridArea.bounds;

        Collider2D checkoutCollider = checkout != null
            ? checkout.GetComponent<Collider2D>()
            : null;

        Vector2Int head = new Vector2Int(
            Mathf.RoundToInt(snake.transform.position.x),
            Mathf.RoundToInt(snake.transform.position.y)
        );

        for (int y = Mathf.CeilToInt(bounds.min.y);
             y <= Mathf.FloorToInt(bounds.max.y); y++)
        {
            for (int x = Mathf.CeilToInt(bounds.min.x);
                 x <= Mathf.FloorToInt(bounds.max.x); x++)
            {
                if (snake.Occupies(x, y))
                {
                    continue;
                }

                if (checkoutCollider != null &&
                    checkoutCollider.OverlapPoint(new Vector2(x, y)))
                {
                    continue;
                }

                if (protectHead)
                {
                    int distance =
                        Mathf.Abs(x - head.x) +
                        Mathf.Abs(y - head.y);

                    if (distance <= headSafeRadius)
                    {
                        continue;
                    }
                }

                cells.Add(new Vector2Int(x, y));
            }
        }

        return cells;
    }

    private void SpawnProducts(List<Vector2Int> cells, int count)
    {
        count = Mathf.Min(count, cells.Count);

        for (int i = 0; i < count; i++)
        {
            ProductDefinition product =
                products[Random.Range(0, products.Length)];

            Food food = Instantiate(
                foodPrefab,
                new Vector3(cells[i].x, cells[i].y, 0f),
                Quaternion.identity,
                transform
            );

            food.Initialize(
                this,
                product.productName,
                product.priceCents,
                product.sprite
            );

            activeFoods.Add(food);
        }
    }

    private void ClearProducts()
    {
        foreach (Food food in activeFoods)
        {
            if (food == null)
            {
                continue;
            }

            food.gameObject.SetActive(false);
            Destroy(food.gameObject);
        }

        activeFoods.Clear();
    }

    private void Shuffle(List<Vector2Int> cells)
    {
        for (int i = cells.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            Vector2Int temporary = cells[i];
            cells[i] = cells[randomIndex];
            cells[randomIndex] = temporary;
        }
    }
}