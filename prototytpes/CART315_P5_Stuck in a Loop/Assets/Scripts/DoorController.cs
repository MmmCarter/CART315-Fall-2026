
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[DefaultExecutionOrder(1200)]
public class DoorController : MonoBehaviour
{
    [Header("References")]
    public PressurePlate pressurePlate;
    public LoopManager loopManager;

    [Header("Door Movement")]
    public float openHeight = 3.15f;
    public float openSpeed = 4f;
    public float closeSpeed = 3f;

    [Header("Safety Settings")]
    public bool preventCrushing = true;
    public float safetyPadding = 0.05f;

    [Header("Indicator Settings")]
    public Renderer indicatorRenderer;
    public Material closedMaterial;
    public Material openMaterial;

    private BoxCollider doorCollider;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool openRequested;
    private int lastLoopCount;

    public bool IsOpen =>
        Vector3.Distance(
            transform.position,
            openPosition
        ) < 0.01f;

    private void Awake()
    {
        doorCollider =
            GetComponent<BoxCollider>();

        // Save original position (closed)
        closedPosition = transform.position;

        // Calculate fully open position
        openPosition = closedPosition +
            Vector3.up * openHeight;
    }

    private void Start()
    {
        if (pressurePlate == null)
        {
            Debug.LogError(
                "Pressure Plate is not assigned!",
                this
            );

            enabled = false;
            return;
        }

        if (loopManager != null)
        {
            lastLoopCount =
                loopManager.LoopCount;
        }

        openRequested = pressurePlate.IsPressed;

        UpdateIndicator();
    }

    private void LateUpdate()
    {
        // Read the current state.
        bool shouldOpen =
            pressurePlate.IsPressed;

        if (shouldOpen != openRequested)
        {
            openRequested = shouldOpen;

            UpdateIndicator();
        }

        if (loopManager != null &&
            loopManager.LoopCount != lastLoopCount)
        {
            lastLoopCount =
                loopManager.LoopCount;

            SynchronizeAfterReset();

            return;
        }

        MoveDoor();
    }

    private void MoveDoor()
    {
        Vector3 targetPosition =
            openRequested
                ? openPosition
                : closedPosition;

        float speed = openRequested
            ? openSpeed
            : closeSpeed;

        Vector3 nextPosition =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

        if ((nextPosition -
            transform.position).sqrMagnitude
            < 0.00000001f)
        {
            return;
        }

        if (!openRequested &&
            preventCrushing &&
            IsClosingPathBlocked(nextPosition))
        {
            return;
        }

        SetDoorPosition(nextPosition);
    }

    private void SynchronizeAfterReset()
    {
        Vector3 targetPosition =
            openRequested
                ? openPosition
                : closedPosition;

        if (!openRequested &&
            preventCrushing &&
            IsClosingPathBlocked(targetPosition))
        {
            return;
        }

        // state after a time reset
        SetDoorPosition(targetPosition);
    }

    private void SetDoorPosition(Vector3 position)
    {
        transform.position = position;

        Physics.SyncTransforms();
    }

    private bool IsClosingPathBlocked(
        Vector3 nextPosition)
    {
        Vector3 movement =
            nextPosition - transform.position;

        if (movement.sqrMagnitude < 0.00000001f)
        {
            return false;
        }

        Vector3 currentCenter =
            transform.TransformPoint(
                doorCollider.center
            );

        Vector3 scale = transform.lossyScale;

        Vector3 halfExtents = Vector3.Scale(
            doorCollider.size * 0.5f,
            new Vector3(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z)
            )
        );

        Vector3 checkCenter =
            currentCenter + movement * 0.5f;

        halfExtents.y +=
            Mathf.Abs(movement.y) * 0.5f;

        halfExtents +=
            Vector3.one * safetyPadding;

        Collider[] hits = Physics.OverlapBox(
            checkCenter,
            halfExtents,
            transform.rotation,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            // Ignore the door itself
            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                continue;
            }

            // Detect player
            if (hit.GetComponentInParent<PlayerReset>()
                != null)
            {
                return true;
            }

            // Detect solid pickup object
            if (hit.GetComponentInParent<PickupObject>()
                != null)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateIndicator()
    {
        if (indicatorRenderer == null)
        {
            return;
        }

        Material targetMaterial = openRequested
            ? openMaterial
            : closedMaterial;

        if (targetMaterial != null)
        {
            indicatorRenderer.sharedMaterial =
                targetMaterial;
        }
    }
}
