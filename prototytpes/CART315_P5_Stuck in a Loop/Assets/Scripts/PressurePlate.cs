
using UnityEngine;

[DefaultExecutionOrder(1150)]
public class PressurePlate : MonoBehaviour
{
    [Header("Detection")]
    public BoxCollider detectionZone;
    public Collider plateSurface;

    [Header("Activation Settings")]
    [Min(1)]
    public int requiredObjects = 1;

    public float surfaceTolerance = 0.1f;

    [Header("Visual Settings")]
    public Renderer plateRenderer;
    public Material inactiveMaterial;
    public Material activeMaterial;

    private bool isPressed;
    private int detectedObjects;

    public bool IsPressed => isPressed;
    public int DetectedObjects => detectedObjects;

    private void Start()
    {
        if (detectionZone == null ||
            plateSurface == null)
        {
            Debug.LogError(
                "Pressure plate detection references are missing.",
                this
            );

            enabled = false;
            return;
        }

        RefreshState();
        UpdateAppearance();
    }

    private void LateUpdate()
    {
        RefreshState();
    }

    private void RefreshState()
    {
        detectedObjects = CountObjectsOnPlate();

        bool newPressedState =
            detectedObjects >= requiredObjects;

        if (newPressedState != isPressed)
        {
            isPressed = newPressedState;

            UpdateAppearance();

            Debug.Log(
                gameObject.name +
                (isPressed
                    ? " activated."
                    : " deactivated."),
                this
            );
        }
    }

    private int CountObjectsOnPlate()
    {
        Transform zoneTransform =
            detectionZone.transform;

        Vector3 center =
            zoneTransform.TransformPoint(
                detectionZone.center
            );

        Vector3 scale = zoneTransform.lossyScale;

        Vector3 halfExtents = Vector3.Scale(
            detectionZone.size * 0.5f,
            new Vector3(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z)
            )
        );

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            zoneTransform.rotation,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        var validObjects =
            new System.Collections.Generic
                .HashSet<PickupObject>();

        float surfaceHeight =
            plateSurface.bounds.max.y;

        foreach (Collider hit in hits)
        {
            // Only pickup objects can activate the plate
            PickupObject pickup =
                hit.GetComponentInParent<PickupObject>();

            if (pickup == null)
            {
                continue;
            }

            if (pickup.IsHeld)
            {
                continue;
            }

            float bottomHeight = hit.bounds.min.y;

            if (Mathf.Abs(
                bottomHeight - surfaceHeight
            ) > surfaceTolerance)
            {
                continue;
            }

            Vector3 localCenter =
                zoneTransform.InverseTransformPoint(
                    pickup.WorldCenter
                ) - detectionZone.center;

            if (Mathf.Abs(localCenter.x) >
                detectionZone.size.x * 0.5f)
            {
                continue;
            }

            if (Mathf.Abs(localCenter.z) >
                detectionZone.size.z * 0.5f)
            {
                continue;
            }

            // Prevent counting one object twice
            validObjects.Add(pickup);
        }

        return validObjects.Count;
    }

    private void UpdateAppearance()
    {
        if (plateRenderer == null)
        {
            return;
        }

        Material targetMaterial = isPressed
            ? activeMaterial
            : inactiveMaterial;

        if (targetMaterial != null)
        {
            plateRenderer.sharedMaterial =
                targetMaterial;
        }
    }
}
