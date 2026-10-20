using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class CheckoutPoint : MonoBehaviour
{
    private ReceiptManager receipt;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        receipt = FindFirstObjectByType<ReceiptManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (receipt == null)
        {
            return;
        }

        spriteRenderer.color = receipt.CanCheckout
            ? Color.white
            : new Color(0.45f, 0.45f, 0.45f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Snake>() == null)
        {
            return;
        }

        if (receipt != null)
        {
            receipt.TryCheckout();
        }
    }
}