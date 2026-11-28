
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[DefaultExecutionOrder(1000)]
public class LoopManager : MonoBehaviour
{
    [Header("Loop Settings")]
    [Min(1f)] public float loopDuration = 15f;
    public float warningTime = 3f;

    [Header("Player")]
    public PlayerReset playerReset;
    public PlayerInteraction playerInteraction;

    [Header("Resettable Objects")]
    public ResettableObject[] resettableObjects;

    [Header("UI References")]
    public TMP_Text timerText;
    public TMP_Text loopText;

    [Header("UI Colors")]
    public Color normalColor = Color.white;

    public Color warningColor =
        new Color(1f, 0.255f, 0.255f);

    private float remainingTime;
    private int loopCount = 1;

    public float RemainingTime => remainingTime;
    public int LoopCount => loopCount;
    public float LoopDuration => loopDuration;

    private void Start()
    {
        if (playerReset == null)
        {
            Debug.LogError(
                "Player Reset is not assigned!",
                this
            );

            enabled = false;
            return;
        }

        // Automatically locate Player Interaction
        if (playerInteraction == null)
        {
            playerInteraction =
                playerReset.GetComponent<PlayerInteraction>();
        }

        remainingTime = loopDuration;

        UpdateUI();
    }

    private void LateUpdate()
    {
        // Pause when player has no control
        if (!playerReset.HasControl)
        {
            return;
        }

        // Manual reset
        if (Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetLoop();
            return;
        }

        // Countdown
        remainingTime -= Time.deltaTime;

        // Automatic reset
        if (remainingTime <= 0f)
        {
            ResetLoop();
            return;
        }

        UpdateUI();
    }

    public void ResetLoop()
    {
        // Release held objects before teleporting
        if (playerInteraction != null)
        {
            playerInteraction.ResetInteraction();
        }

        // Restore player
        playerReset.ResetPlayer();

        // Restore all non-anchored objects
        if (resettableObjects != null)
        {
            foreach (ResettableObject obj
                in resettableObjects)
            {
                if (obj != null)
                {
                    obj.ResetObject();
                }
            }
        }

        Physics.SyncTransforms();

        // Start next loop
        loopCount++;
        remainingTime = loopDuration;

        UpdateUI();

        Debug.Log(
            "Time reset. Starting Loop " + loopCount
        );
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(
                Mathf.Max(remainingTime, 0f)
            );

            int minutes = seconds / 60;
            int displaySeconds = seconds % 60;

            timerText.text =
                $"{minutes:00}:{displaySeconds:00}";

            timerText.color =
                remainingTime <= warningTime
                ? warningColor
                : normalColor;
        }

        if (loopText != null)
        {
            loopText.text =
                $"LOOP {loopCount:000}";
        }
    }
}
