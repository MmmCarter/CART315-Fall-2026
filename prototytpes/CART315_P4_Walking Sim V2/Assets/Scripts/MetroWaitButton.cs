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

    [Header("Button Audio")]
    [SerializeField] private AudioSource buttonAudio;
    [SerializeField] private AudioClip pressSound;
    [SerializeField, Range(0f, 1f)] private float pressVolume = 0.3f;
    [SerializeField, Min(0.05f)] private float pressCooldown = 0.25f;

    [Header("Display Colors")]
    [SerializeField] private Color idleColor = new Color(1f, 0.78f, 0.42f);
    [SerializeField] private Color waitingColor = new Color(1f, 0.9f, 0.65f);
    [SerializeField] private Color endedColor = new Color(1f, 0.58f, 0.4f);

    private bool requested;
    private bool waiting;
    private GUIStyle promptStyle;
    private float nextPressTime;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (buttonAudio == null) buttonAudio = GetComponent<AudioSource>();
        if (buttonAudio == null) buttonAudio = gameObject.AddComponent<AudioSource>();
        buttonAudio.playOnAwake = false;
        buttonAudio.loop = false;
        buttonAudio.spatialBlend = 1f;
        buttonAudio.rolloffMode = AudioRolloffMode.Linear;
        buttonAudio.minDistance = 1f;
        buttonAudio.maxDistance = 7f;
        buttonAudio.dopplerLevel = 0f;
        buttonAudio.volume = 1f;
        SetDisplay("SERVICE INFORMATION", idleColor);
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
        if (Keyboard.current == null || !IsTargeted()) return;
        if (!Keyboard.current.eKey.wasPressedThisFrame || Time.unscaledTime < nextPressTime)
            return;

        nextPressTime = Time.unscaledTime + pressCooldown;
        if (buttonAudio != null && pressSound != null)
            buttonAudio.PlayOneShot(pressSound, pressVolume);

        // Later presses still give audio feedback, but never restart the request.
        if (!requested) StartCoroutine(RequestService());
    }

    private void SetDisplay(string message, Color color)
    {
        if (statusText == null) return;
        statusText.text = message;
        statusText.color = color;
    }

    private IEnumerator RequestService()
    {
        requested = true;
        waiting = waitBeforeReply;

        if (waitBeforeReply)
        {
            SetDisplay("REQUEST RECEIVED\nPLEASE WAIT...", waitingColor);
            yield return new WaitForSeconds(replyDelay);
        }

        waiting = false;
        SetDisplay("SERVICE ENDED\nPLEASE USE THE EXIT", endedColor);
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