
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

    [Header("Resettable Objects")]
    public ResettableObject[] resettableObjects;

    [Header("UI References")]
    public TMP_Text timerText;
    public TMP_Text loopText;

    [Header("UI Colors")]
    public Color normalColor = Color.white;
    public Color warningColor = new Color(1f, 0.255f, 0.255f);

    private float remainingTime;
    private int loopCount = 1;

    public float RemainingTime => remainingTime;
    public int LoopCount => loopCount;
    public float LoopDuration => loopDuration;

    private void Start()
    {
        if (playerReset == null)
        {
            Debug.LogError("Player Reset is not assigned!", this);
            enabled = false;
            return;
        }

        remainingTime = loopDuration;
        UpdateUI();
    }

    private void LateUpdate()
    {
        // Wait until the player captures the mouse
        // Pause the countdown when control is released
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
        playerReset.ResetPlayer();

        // Reset all non-anchored objects
        if (resettableObjects != null)
        {
            foreach (ResettableObject obj in resettableObjects)
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

        Debug.Log("Time reset. Starting Loop " + loopCount);
    }

    private void UpdateUI()
    {
        // Display remaining time
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

        // Display loop number
        if (loopText != null)
        {
            loopText.text = $"LOOP {loopCount:000}";
        }
    }
}
