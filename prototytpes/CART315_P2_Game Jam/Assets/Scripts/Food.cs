using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Food : MonoBehaviour
{
    public string ProductName { get; private set; }
    public int PriceCents { get; private set; }

    [Header("Price Label")]
    public TMP_FontAsset priceFont;

    private FoodSpawner spawner;

    public void Initialize(
        FoodSpawner owner,
        string productName,
        int priceCents,
        Sprite sprite)
    {
        spawner = owner;
        ProductName = productName;
        PriceCents = priceCents;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.sortingOrder = 2;

        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.offset = Vector2.zero;
        trigger.size = new Vector2(0.65f, 0.65f);

        CreateLabel();
    }

    private void CreateLabel()
    {
        GameObject labelObject = new GameObject("Product Label");
        labelObject.transform.SetParent(transform, false);

        TextMeshPro label = labelObject.AddComponent<TextMeshPro>();

        if (priceFont != null)
        {
            label.font = priceFont;
        }

        label.text = "$" + (PriceCents / 100f).ToString("0.00");

        label.fontSize = 3f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;

        label.rectTransform.sizeDelta = new Vector2(2f, 0.7f);

        labelObject.transform.localPosition =
            new Vector3(0f, 0.65f, -0.1f);

        labelObject.GetComponent<MeshRenderer>().sortingOrder = 5;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Snake>() == null)
        {
            return;
        }

        if (spawner != null)
        {
            spawner.TryCollect(this);
        }
    }
}
