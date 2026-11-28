
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerReset))]
[DefaultExecutionOrder(500)]
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Image crosshair;

    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public float holdDistance = 1.7f;
    public float minimumHoldDistance = 0.9f;
    public float collisionPadding = 0.03f;

    [Header("UI Colors")]
    public Color normalColor = Color.white;

    public Color interactColor =
        new Color(0.208f, 0.851f, 1f);

    public Color blockedColor =
        new Color(1f, 0.54f, 0.2f);

    private PlayerReset playerReset;

    private PickupObject focusedObject;
    private PickupObject heldObject;

    public PickupObject HeldObject => heldObject;
    public bool IsHolding => heldObject != null;

    private void Awake()
    {
        playerReset = GetComponent<PlayerReset>();
    }

    private void Start()
    {
        SetCrosshairColor(normalColor);

        if (playerCamera == null)
        {
            Debug.LogError(
                "Player Camera is not assigned!",
                this
            );

            enabled = false;
        }
    }

    private void LateUpdate()
    {
        // Check player control state
        if (!playerReset.HasControl)
        {
            ClearFocus();
            SetCrosshairColor(normalColor);
            return;
        }

        bool interactPressed =
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        // Carry the held object
        if (IsHolding)
        {
            bool hasClearSpace = UpdateHeldPosition();

            if (hasClearSpace)
            {
                SetCrosshairColor(interactColor);

                if (interactPressed)
                {
                    DropObject();
                }
            }
            else
            {
                SetCrosshairColor(blockedColor);
            }

            return;
        }

        // Detect objects
        UpdateFocus();

        if (focusedObject == null)
        {
            SetCrosshairColor(normalColor);
            return;
        }

        // Prevent anchored objects from being picked up
        if (focusedObject.IsAnchored)
        {
            focusedObject.SetHighlighted(false);
            SetCrosshairColor(blockedColor);
            return;
        }

        if (!focusedObject.CanPickup)
        {
            focusedObject.SetHighlighted(false);
            SetCrosshairColor(normalColor);
            return;
        }

        // Highlight the focused object
        focusedObject.SetHighlighted(true);
        SetCrosshairColor(interactColor);

        if (interactPressed)
        {
            PickUpObject(focusedObject);
        }
    }

    private void UpdateFocus()
    {
        PickupObject newFocus = null;

        if (TryGetClosestHit(
            interactionDistance,
            out RaycastHit hit))
        {
            newFocus =
                hit.collider.GetComponentInParent<PickupObject>();
        }

        if (newFocus == focusedObject)
        {
            return;
        }

        ClearFocus();

        focusedObject = newFocus;
    }

    private bool TryGetClosestHit(
        float distance,
        out RaycastHit closestHit)
    {
        RaycastHit[] hits = Physics.RaycastAll(
            playerCamera.position,
            playerCamera.forward,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        closestHit = default;

        float closestDistance = distance;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            if (IsPlayerCollider(hit.collider))
            {
                continue;
            }

            if (IsHeldCollider(hit.collider))
            {
                continue;
            }

            if (hit.distance <= closestDistance)
            {
                closestDistance = hit.distance;
                closestHit = hit;
                found = true;
            }
        }

        return found;
    }

    private void PickUpObject(PickupObject target)
    {
        if (target == null || !target.BeginHold())
        {
            return;
        }

        ClearFocus();

        heldObject = target;

        Physics.SyncTransforms();

        bool hasClearSpace = UpdateHeldPosition();

        SetCrosshairColor(
            hasClearSpace
                ? interactColor
                : blockedColor
        );
    }

    private bool UpdateHeldPosition()
    {
        if (heldObject == null)
        {
            return false;
        }

        float distance = holdDistance;

        float radius =
            heldObject.HalfExtents.magnitude;

        // Prevent the object from entering walls
        if (TryGetClosestHit(
            holdDistance + radius + collisionPadding,
            out RaycastHit hit))
        {
            distance = Mathf.Min(
                holdDistance,
                hit.distance - radius - collisionPadding
            );
        }

        if (distance < minimumHoldDistance)
        {
            return false;
        }

        Vector3 targetCenter =
            playerCamera.position +
            playerCamera.forward * distance;

        // Check the target position
        if (!CanOccupyPosition(targetCenter))
        {
            return false;
        }

        // Check the movement path
        if (!IsCarryPathClear(targetCenter))
        {
            return false;
        }

        Vector3 centerOffset =
            heldObject.WorldCenter -
            heldObject.transform.position;

        heldObject.MoveHeldTo(
            targetCenter - centerOffset
        );

        return true;
    }

    private bool CanOccupyPosition(Vector3 center)
    {
        Vector3 halfExtents =
            heldObject.HalfExtents +
            Vector3.one * collisionPadding;

        Collider[] overlaps = Physics.OverlapBox(
            center,
            halfExtents,
            heldObject.transform.rotation,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider other in overlaps)
        {
            if (IsHeldCollider(other))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private bool IsCarryPathClear(Vector3 targetCenter)
    {
        Vector3 startCenter = heldObject.WorldCenter;

        Vector3 movement =
            targetCenter - startCenter;

        float distance = movement.magnitude;

        if (distance < 0.001f)
        {
            return true;
        }

        Vector3 halfExtents =
            heldObject.HalfExtents * 0.98f;

        RaycastHit[] hits = Physics.BoxCastAll(
            startCenter,
            halfExtents,
            movement.normalized,
            heldObject.transform.rotation,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (RaycastHit hit in hits)
        {
            if (IsHeldCollider(hit.collider))
            {
                continue;
            }

            if (IsPlayerCollider(hit.collider))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private void DropObject()
    {
        if (heldObject == null)
        {
            return;
        }

        // Check the placement space
        if (!CanOccupyPosition(heldObject.WorldCenter))
        {
            SetCrosshairColor(blockedColor);
            return;
        }

        heldObject.EndHold();
        heldObject = null;

        SetCrosshairColor(normalColor);
    }

    public void ResetInteraction()
    {
        ClearFocus();

        if (heldObject != null)
        {
            heldObject.EndHold();
            heldObject = null;
        }

        SetCrosshairColor(normalColor);
    }

    private void ClearFocus()
    {
        if (focusedObject != null)
        {
            focusedObject.SetHighlighted(false);
        }

        focusedObject = null;
    }

    private bool IsPlayerCollider(Collider other)
    {
        return other.transform == transform ||
               other.transform.IsChildOf(transform);
    }

    private bool IsHeldCollider(Collider other)
    {
        if (heldObject == null)
        {
            return false;
        }

        return other.transform == heldObject.transform ||
               other.transform.IsChildOf(
                   heldObject.transform
               );
    }

    private void SetCrosshairColor(Color color)
    {
        if (crosshair != null)
        {
            crosshair.color = color;
        }
    }

    private void OnDisable()
    {
        ResetInteraction();
    }
}
