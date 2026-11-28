
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(ResettableObject))]
public class PickupObject : MonoBehaviour
{
    [Header("Visual Settings")]
    public Renderer objectRenderer;

    public Color highlightColor =
        new Color(0.65f, 0.9f, 0.95f);

    private Rigidbody rb;
    private BoxCollider boxCollider;
    private ResettableObject resettableObject;

    private bool isHeld;
    private bool isHighlighted;

    private bool previousGravity;
    private bool previousKinematic;
    private bool previousColliderEnabled;

    private RigidbodyInterpolation previousInterpolation;
    private CollisionDetectionMode previousCollisionMode;

    private MaterialPropertyBlock savedBlock;
    private MaterialPropertyBlock highlightBlock;

    public bool IsHeld => isHeld;
    public bool IsAnchored => resettableObject.IsAnchored;

    public bool CanPickup =>
        !isHeld && !IsAnchored && boxCollider.enabled;

    public Vector3 HalfExtents =>
        Vector3.Scale(
            boxCollider.size * 0.5f,
            new Vector3(
                Mathf.Abs(transform.lossyScale.x),
                Mathf.Abs(transform.lossyScale.y),
                Mathf.Abs(transform.lossyScale.z)
            )
        );

    public Vector3 WorldCenter =>
        transform.TransformPoint(boxCollider.center);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        resettableObject = GetComponent<ResettableObject>();

        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }

        savedBlock = new MaterialPropertyBlock();
        highlightBlock = new MaterialPropertyBlock();
    }

    public bool BeginHold()
    {
        if (!CanPickup)
        {
            return false;
        }

        SetHighlighted(false);

        // Store current settings
        previousGravity = rb.useGravity;
        previousKinematic = rb.isKinematic;
        previousColliderEnabled = boxCollider.enabled;
        previousInterpolation = rb.interpolation;
        previousCollisionMode = rb.collisionDetectionMode;

        // Stop all current motion
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Switch to carrying mode
        rb.collisionDetectionMode =
            CollisionDetectionMode.Discrete;

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        boxCollider.enabled = false;

        isHeld = true;

        return true;
    }

    public void MoveHeldTo(Vector3 position)
    {
        if (!isHeld)
        {
            return;
        }

        rb.position = position;
        transform.position = position;
    }

    public void EndHold()
    {
        if (!isHeld)
        {
            return;
        }

        // Keep current position
        rb.position = transform.position;
        rb.rotation = transform.rotation;

        // Restore original settings
        rb.isKinematic = previousKinematic;
        rb.useGravity = previousGravity;
        rb.interpolation = previousInterpolation;
        rb.collisionDetectionMode = previousCollisionMode;

        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        boxCollider.enabled = previousColliderEnabled;

        isHeld = false;

        if (!rb.isKinematic)
        {
            rb.WakeUp();
        }

        Physics.SyncTransforms();
    }

    public void SetHighlighted(bool highlighted)
    {
        if (objectRenderer == null)
        {
            return;
        }

        if (highlighted == isHighlighted)
        {
            return;
        }

        if (highlighted)
        {
            if (!CanPickup)
            {
                return;
            }

            Material material = objectRenderer.sharedMaterial;

            if (material == null)
            {
                return;
            }

            // Preserve existing material properties
            objectRenderer.GetPropertyBlock(savedBlock);
            objectRenderer.GetPropertyBlock(highlightBlock);

            if (material.HasProperty("_BaseColor"))
            {
                highlightBlock.SetColor(
                    "_BaseColor",
                    highlightColor
                );
            }
            else if (material.HasProperty("_Color"))
            {
                highlightBlock.SetColor(
                    "_Color",
                    highlightColor
                );
            }

            objectRenderer.SetPropertyBlock(highlightBlock);
        }
        else
        {
            // Restore original appearance
            objectRenderer.SetPropertyBlock(savedBlock);
        }

        isHighlighted = highlighted;
    }
}
