using UnityEngine;

[RequireComponent(typeof(Snake))]
public class ReceiptAppearance : MonoBehaviour
{
    private Snake snake;

    private Sprite bodySprite;
    private Sprite headSprite;
    private Sprite tailSprite;

    private void Awake()
    {
        snake = GetComponent<Snake>();

        bodySprite = CreatePaperSprite(0);
        headSprite = CreatePaperSprite(1);
        tailSprite = CreatePaperSprite(2);
    }

    private void LateUpdate()
    {
        var segments = snake.Segments;

        for (int i = 0; i < segments.Count; i++)
        {
            Transform segment = segments[i];
            SpriteRenderer renderer =
                segment.GetComponent<SpriteRenderer>();

            if (renderer == null)
            {
                continue;
            }

            renderer.color = Color.white;
            renderer.sortingOrder = 3;

            if (i == 0)
            {
                renderer.sprite = headSprite;
            }
            else if (i == segments.Count - 1)
            {
                renderer.sprite = tailSprite;
            }
            else
            {
                renderer.sprite = bodySprite;
            }

            Vector2 facing;

            if (i == 0)
            {
                facing = new Vector2(
                    snake.direction.x,
                    snake.direction.y
                );
            }
            else
            {
                facing = segments[i - 1].position - segment.position;
            }

            if (facing.sqrMagnitude > 0.01f)
            {
                float angle =
                    Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

                segment.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }

    // Kind: 0 = body, 1 = head, 2 = tail
    private Sprite CreatePaperSprite(int kind)
    {
        const int size = 32;

        Texture2D texture =
            new Texture2D(size, size, TextureFormat.RGBA32, false);

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color paper = new Color(0.96f, 0.93f, 0.86f);
        Color ink = new Color(0.22f, 0.22f, 0.20f);
        Color edge = new Color(0.78f, 0.75f, 0.68f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Color color = paper;

                // edge of the receipt color are different
                if (y == 0 || y == size - 1)
                {
                    color = edge;
                }

                if (kind == 1)
                {
                    // head
                    if (x >= 8 && x <= 23 &&
                        y >= 6 && y <= 25 &&
                        (x % 4 == 0 || x % 4 == 1))
                    {
                        color = ink;
                    }

                    // front edge
                    if (x >= size - (y % 4))
                    {
                        color = Color.clear;
                    }
                }
                else
                {
                    // body
                    bool printedRow =
                        x == 7 || x == 14 || x == 21;

                    bool printedText =
                        y >= 6 && y <= 20 && y % 5 != 0;

                    bool printedPrice =
                        y >= 24 && y <= 27;

                    if (printedRow && (printedText || printedPrice))
                    {
                        color = ink;
                    }

                    // tail
                    if (kind == 2 && x < y % 4)
                    {
                        color = Color.clear;
                    }
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            32f,
            0,
            SpriteMeshType.FullRect
        );
    }

    private void OnDestroy()
    {
        ReleaseSprite(bodySprite);
        ReleaseSprite(headSprite);
        ReleaseSprite(tailSprite);
    }

    private void ReleaseSprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        Destroy(sprite.texture);
        Destroy(sprite);
    }
}