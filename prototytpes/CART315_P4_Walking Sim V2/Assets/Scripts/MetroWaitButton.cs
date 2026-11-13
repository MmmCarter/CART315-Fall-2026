using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Attach to the button Cube
public class MetroWaitButton : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private bool waitBeforeReply = true;
    [SerializeField, Min(0f)] private float replyDelay = 3f;
    [SerializeField, Min(0.1f)] private float interactionDistance = 2.5f;

    private bool requested;
    private bool waiting;
    private GUIStyle promptStyle;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (statusText != null) statusText.text = "METRO SERVICE\nPRESS E";
    }

    private bool IsTargeted()
    {
        if (playerCamera == null || Cursor.lockState != CursorLockMode.Locked)
            return false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        return Physics.Raycast(ray, out RaycastHit hit, interactionDistance,
                   Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
               && hit.collider.GetComponentInParent<MetroWaitButton>() == this;
    }

    private void Update()
    {
        if (requested || Keyboard.current == null || !IsTargeted()) return;
        if (Keyboard.current.eKey.wasPressedThisFrame)
            StartCoroutine(RequestService());
    }

    private IEnumerator RequestService()
    {
        requested = true;
        waiting = waitBeforeReply;

        if (waitBeforeReply)
        {
            if (statusText != null) statusText.text = "PLEASE WAIT...";
            yield return new WaitForSeconds(replyDelay);
        }

        waiting = false;
        if (statusText != null)
            statusText.text = "SERVICE ENDED\nPLEASE USE THE EXIT";
    }

    private void OnGUI()
    {
        if (!IsTargeted()) return;
        if (promptStyle == null)
        {
            promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 20;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = Color.white;
        }

        string message = !requested ? "E - Check metro service"
            : waiting ? "Waiting for a response..." : "No further metro service";
        Rect rect = new Rect((Screen.width - 480f) / 2f, Screen.height - 90f, 480f, 44f);
        GUI.Box(rect, GUIContent.none);
        GUI.Label(rect, message, promptStyle);
    }
}