using Cart315.FirstPerson;
using UnityEngine;
using UnityEngine.InputSystem;

// Attach to OutdoorBench.
public class BenchRestInteraction : MonoBehaviour
{
    [SerializeField] private FPController player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform sitPoint;
    [SerializeField, Min(0.1f)] private float interactionDistance = 2.5f;
    [SerializeField, Min(0.1f)] private float seatedEyeHeight = 1.15f;
    [SerializeField, Min(0.001f)] private float lookSensitivity = 0.05f;

    private CharacterController characterController;
    private Transform cameraPivot;
    private Behaviour[] movementComponents;
    private bool[] previousEnabled;
    private bool previousControllerEnabled;
    private Vector3 standingPosition;
    private Quaternion standingRotation;
    private Vector3 standingPivotPosition;
    private Quaternion standingPivotRotation;
    private bool seated;
    private float yaw;
    private float pitch;
    private GUIStyle promptStyle;

    private void Start()
    {
        if (player == null || sitPoint == null)
        {
            Debug.LogError("Assign Player and Sit Point on Bench Rest Interaction.", this);
            enabled = false;
            return;
        }

        if (playerCamera == null) playerCamera = player.GetComponentInChildren<Camera>();
        cameraPivot = player.CameraHolder;
        characterController = player.GetComponent<CharacterController>();
        if (playerCamera == null || cameraPivot == null || characterController == null)
        {
            Debug.LogError("The assigned player needs its camera, camera pivot, and Character Controller.", this);
            enabled = false;
            return;
        }

        movementComponents = new Behaviour[]
        {
            player,
            player.GetComponent<FPJumping>(),
            player.GetComponent<FPSprinting>(),
            player.GetComponent<FPHeadBobbing>(),
            player.GetComponent<FPFootsteps>()
        };
        previousEnabled = new bool[movementComponents.Length];
    }

    private bool IsTargeted()
    {
        if (playerCamera == null || Cursor.lockState != CursorLockMode.Locked)
            return false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        return Physics.Raycast(ray, out RaycastHit hit, interactionDistance,
                   Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
               && hit.collider.GetComponentInParent<BenchRestInteraction>() == this;
    }

    private void Update()
    {
        if (seated)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                    CaptureCursor();
                return;
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                StandUp();
            return;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && IsTargeted())
            SitDown();
    }

    private void LateUpdate()
    {
        if (!seated || Mouse.current == null || Cursor.lockState != CursorLockMode.Locked)
            return;

        Vector2 delta = Mouse.current.delta.ReadValue();
        yaw = Mathf.Clamp(yaw + delta.x * lookSensitivity, -80f, 80f);
        pitch = Mathf.Clamp(pitch - delta.y * lookSensitivity, -65f, 65f);
        cameraPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void SitDown()
    {
        standingPosition = player.transform.position;
        standingRotation = player.transform.rotation;
        standingPivotPosition = cameraPivot.localPosition;
        standingPivotRotation = cameraPivot.localRotation;
        previousControllerEnabled = characterController.enabled;

        for (int i = 0; i < movementComponents.Length; i++)
        {
            Behaviour component = movementComponents[i];
            if (component == null) continue;
            previousEnabled[i] = component.enabled;
            component.enabled = false;
        }

        characterController.enabled = false;
        player.transform.SetPositionAndRotation(sitPoint.position, sitPoint.rotation);
        cameraPivot.localPosition = new Vector3(0f, seatedEyeHeight, 0f);
        cameraPivot.localRotation = Quaternion.identity;
        yaw = pitch = 0f;
        seated = true;
        CaptureCursor();
    }

    private void StandUp()
    {
        if (!seated) return;
        seated = false;

        if (player != null)
            player.transform.SetPositionAndRotation(standingPosition, standingRotation);
        if (cameraPivot != null)
        {
            cameraPivot.localPosition = standingPivotPosition;
            cameraPivot.localRotation = standingPivotRotation;
        }
        if (characterController != null)
            characterController.enabled = previousControllerEnabled;

        for (int i = 0; i < movementComponents.Length; i++)
            if (movementComponents[i] != null)
                movementComponents[i].enabled = previousEnabled[i];
    }

    private static void CaptureCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        if (seated) StandUp();
    }

    private void OnGUI()
    {
        if (!seated && !IsTargeted()) return;
        if (promptStyle == null)
        {
            promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 20;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = Color.white;
        }

        string message = seated
            ? (Cursor.lockState == CursorLockMode.Locked
                ? "E - Stand up    |    Mouse - Look around" : "Click to continue resting")
            : "E - Sit and rest";
        Rect rect = new Rect((Screen.width - 540f) / 2f, Screen.height - 90f, 540f, 44f);
        GUI.Box(rect, GUIContent.none);
        GUI.Label(rect, message, promptStyle);
    }
}