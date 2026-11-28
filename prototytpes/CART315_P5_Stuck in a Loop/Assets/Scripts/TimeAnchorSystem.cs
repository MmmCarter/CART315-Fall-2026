
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(PlayerReset))]
[RequireComponent(typeof(PlayerInteraction))]
[DefaultExecutionOrder(1100)]
public class TimeAnchorSystem : MonoBehaviour
{
    [Header("References")]
    public LoopManager loopManager;

    [Header("Anchor Settings")]
    [Min(1)]
    public int anchorsPerLoop = 1;

    public TimeAnchorObject[] anchorObjects;

    [Header("UI References")]
    public TMP_Text anchorBudgetText;
    public TMP_Text anchoredCountText;

    [Header("UI Colors")]
    public Color availableColor =
        new Color(0.208f, 0.851f, 1f);

    public Color exhaustedColor =
        new Color(1f, 0.54f, 0.2f);

    private PlayerReset playerReset;
    private PlayerInteraction playerInteraction;

    private int usedThisLoop;
    private int lastLoopCount;

    private readonly HashSet<TimeAnchorObject>
        registeredObjects =
        new HashSet<TimeAnchorObject>();

    public int RemainingAnchors =>
        Mathf.Max(0, anchorsPerLoop - usedThisLoop);

    public int AnchoredCount
    {
        get
        {
            int count = 0;

            foreach (TimeAnchorObject obj
                in registeredObjects)
            {
                if (obj != null && obj.IsAnchored)
                {
                    count++;
                }
            }

            return count;
        }
    }

    private void Awake()
    {
        playerReset =
            GetComponent<PlayerReset>();

        playerInteraction =
            GetComponent<PlayerInteraction>();
    }

    private void Start()
    {
        if (loopManager == null)
        {
            Debug.LogError(
                "Loop Manager is not assigned!",
                this
            );

            enabled = false;
            return;
        }

        if (playerInteraction.playerCamera == null)
        {
            Debug.LogError(
                "Player Camera is not assigned!",
                this
            );

            enabled = false;
            return;
        }

        // Register all anchorable objects
        if (anchorObjects != null)
        {
            foreach (TimeAnchorObject obj
                in anchorObjects)
            {
                if (obj != null)
                {
                    registeredObjects.Add(obj);
                }
            }
        }

        lastLoopCount = loopManager.LoopCount;

        usedThisLoop = 0;

        UpdateHUD();
    }

    private void LateUpdate()
    {
        // Detect beginning of a new loop
        if (loopManager.LoopCount != lastLoopCount)
        {
            lastLoopCount =
                loopManager.LoopCount;

            usedThisLoop = 0;

            UpdateHUD();

            return;
        }

        if (!playerReset.HasControl)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.qKey
            .wasPressedThisFrame)
        {
            return;
        }

        // Avoid pickup and anchor on the same frame
        if (Keyboard.current.eKey
            .wasPressedThisFrame)
        {
            return;
        }

        // Held objects must be dropped first
        if (playerInteraction.IsHolding)
        {
            Debug.Log(
                "Drop the object before anchoring."
            );

            return;
        }

        TryToggleAnchor();
    }

    private void TryToggleAnchor()
    {
        TimeAnchorObject target =
            GetAnchorTarget();

        if (target == null)
        {
            return;
        }

        // Only registered objects can be anchored
        if (!registeredObjects.Contains(target))
        {
            return;
        }

        if (target.IsAnchored)
        {
            if (target.TryUnanchor())
            {
                UpdateHUD();
            }

            return;
        }

        // Check remaining anchor budget
        if (RemainingAnchors <= 0)
        {
            Debug.Log(
                "No anchors remaining this loop."
            );

            return;
        }

        // Spend one anchor only if successful
        if (target.TryAnchor())
        {
            usedThisLoop++;

            UpdateHUD();
        }
        else
        {
            Debug.Log(
                "Anchor failed. Place the object " +
                "on a stable surface first."
            );
        }
    }

    private TimeAnchorObject GetAnchorTarget()
    {
        Transform playerCamera =
            playerInteraction.playerCamera;

        float distance =
            playerInteraction.interactionDistance;

        RaycastHit[] hits = Physics.RaycastAll(
            playerCamera.position,
            playerCamera.forward,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        RaycastHit closestHit = default;

        float closestDistance = distance;

        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform == transform ||
                hit.collider.transform
                    .IsChildOf(transform))
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

        if (!found)
        {
            return null;
        }

        return closestHit.collider
            .GetComponentInParent<TimeAnchorObject>();
    }

    private void UpdateHUD()
    {
        if (anchorBudgetText != null)
        {
            anchorBudgetText.text =
                $"ANCHOR: {RemainingAnchors}/{anchorsPerLoop}";

            anchorBudgetText.color =
                RemainingAnchors > 0
                ? availableColor
                : exhaustedColor;
        }

        if (anchoredCountText != null)
        {
            anchoredCountText.text =
                $"ANCHORED: {AnchoredCount}";
        }
    }
}
