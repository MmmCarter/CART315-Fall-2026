
using UnityEngine;

public class ResettableObject : MonoBehaviour
{
    [Header("Time Anchor")]
    [SerializeField] private bool isAnchored = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private Rigidbody rb;
    private bool initialIsKinematic;
    private bool initialUseGravity;

    public bool IsAnchored => isAnchored;

    private void Awake()
    {
        // Store object's original transform
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;

        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            initialIsKinematic = rb.isKinematic;
            initialUseGravity = rb.useGravity;
        }
    }

    public void ResetObject()
    {
        // Anchored objects will not reset
        if (isAnchored)
        {
            return;
        }

        transform.localScale = initialScale;

        if (rb != null)
        {
            rb.isKinematic = initialIsKinematic;
            rb.useGravity = initialUseGravity;

            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Restore Rigidbody position and rotation
            rb.position = initialPosition;
            rb.rotation = initialRotation;

            if (!rb.isKinematic)
            {
                rb.WakeUp();
            }
        }
        else
        {
            // Reset objects without a Rigidbody
            transform.SetPositionAndRotation(
                initialPosition,
                initialRotation
            );
        }
    }

    public void SetAnchored(bool anchored)
    {
        isAnchored = anchored;
    }
}
