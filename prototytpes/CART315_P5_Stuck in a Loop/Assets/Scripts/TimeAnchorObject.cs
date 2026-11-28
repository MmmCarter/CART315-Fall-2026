
using UnityEngine;

[RequireComponent(typeof(ResettableObject))]
[RequireComponent(typeof(PickupObject))]
[RequireComponent(typeof(Rigidbody))]
public class TimeAnchorObject : MonoBehaviour
{
    [Header("Anchor Appearance")]
    public Renderer objectRenderer;
    public Material anchoredMaterial;

    [Header("Anchor Requirements")]
    public float maxLinearSpeed = 0.15f;
    public float maxAngularSpeed = 0.3f;
    public float groundCheckDistance = 0.12f;

    private Rigidbody rb;
    private BoxCollider boxCollider;
    private PickupObject pickupObject;
    private ResettableObject resettableObject;

    private Material previousMaterial;

    private bool previousGravity;
    private bool previousKinematic;

    private CollisionDetectionMode
        previousCollisionMode;

    private bool hasSavedState;

    public bool IsAnchored =>
        resettableObject.IsAnchored;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        pickupObject =
            GetComponent<PickupObject>();

        resettableObject =
            GetComponent<ResettableObject>();

        if (objectRenderer == null)
        {
            objectRenderer =
                GetComponent<Renderer>();
        }
    }

    public bool CanAnchor()
    {
        if (IsAnchored)
        {
            return false;
        }

        // Cannot anchor a held object
        if (pickupObject.IsHeld)
        {
            return false;
        }

        if (rb.isKinematic ||
            !boxCollider.enabled)
        {
            return false;
        }

        if (rb.linearVelocity.magnitude >
            maxLinearSpeed)
        {
            return false;
        }

        if (rb.angularVelocity.magnitude >
            maxAngularSpeed)
        {
            return false;
        }

        if (!IsSupported())
        {
            return false;
        }

        return true;
    }

    private bool IsSupported()
    {
        Bounds bounds = boxCollider.bounds;

        float distance =
            bounds.extents.y +
            groundCheckDistance;

        RaycastHit[] hits = Physics.RaycastAll(
            bounds.center,
            Vector3.down,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (RaycastHit hit in hits)
        {
            // Ignore the object's colliders
            if (hit.collider.attachedRigidbody == rb)
            {
                continue;
            }

            if (hit.collider
                .GetComponentInParent<PlayerReset>()
                != null)
            {
                continue;
            }

            // Detect a supporting surface
            if (hit.normal.y >= 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryAnchor()
    {
        if (!CanAnchor())
        {
            return false;
        }

        if (objectRenderer == null ||
            anchoredMaterial == null)
        {
            Debug.LogWarning(
                "Anchor material or renderer is missing!",
                this
            );

            return false;
        }

        pickupObject.SetHighlighted(false);

        // Save current appearance
        previousMaterial =
            objectRenderer.sharedMaterial;

        // Save current physics state
        previousGravity = rb.useGravity;
        previousKinematic = rb.isKinematic;

        previousCollisionMode =
            rb.collisionDetectionMode;

        hasSavedState = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Discrete;

        rb.useGravity = false;
        rb.isKinematic = true;

        // Apply the anchor material
        objectRenderer.sharedMaterial =
            anchoredMaterial;

        // Prevent time reset
        resettableObject.SetAnchored(true);

        Physics.SyncTransforms();

        Debug.Log(
            gameObject.name + " anchored.",
            this
        );

        return true;
    }

    public bool TryUnanchor()
    {
        if (!IsAnchored || !hasSavedState)
        {
            return false;
        }

        pickupObject.SetHighlighted(false);

        // Restore normal material
        objectRenderer.sharedMaterial =
            previousMaterial;

        // Restore settings
        rb.isKinematic = previousKinematic;
        rb.useGravity = previousGravity;

        rb.collisionDetectionMode =
            previousCollisionMode;

        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.WakeUp();
        }

        // Allow future time resets
        resettableObject.SetAnchored(false);

        hasSavedState = false;

        Physics.SyncTransforms();

        Debug.Log(
            gameObject.name + " unanchored.",
            this
        );

        return true;
    }
}
