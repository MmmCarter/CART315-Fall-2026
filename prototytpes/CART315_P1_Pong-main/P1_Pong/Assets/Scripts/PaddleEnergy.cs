using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PaddleEnergy : MonoBehaviour
{
    [Header("Controls")]
    public Key powerKey = Key.A;
    public string keyLabel = "A";

    [Header("Energy")]
    [Range(1f, 100f)]
    public float energyPerHit = 25f;

    [Header("Appearance")]
    public Color powerColor = Color.cyan;
    public Slider energyBar;
    public TMP_Text keyHint;

    private float _energy;
    private bool _armed;
    private Vector3 _hintScale;

    private void Awake()
    {
        if (energyBar != null)
        {
            energyBar.minValue = 0f;
            energyBar.maxValue = 100f;
            energyBar.interactable = false;

            if (energyBar.fillRect != null)
            {
                Image fill = energyBar.fillRect.GetComponent<Image>();

                if (fill != null)
                    fill.color = powerColor;
            }
        }

        if (keyHint != null)
        {
            _hintScale = keyHint.rectTransform.localScale;
            keyHint.color = powerColor;
        }

        RefreshUI();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (!_armed && _energy >= 100f &&
            keyboard != null &&
            keyboard[powerKey].wasPressedThisFrame)
        {
            _armed = true;
            RefreshUI();
        }

        // Pulse the hint only while full and waiting for a key press.
        if (keyHint != null)
        {
            float pulse = !_armed && _energy >= 100f
                ? 1f + Mathf.Sin(Time.unscaledTime * 7f) * 0.08f
                : 1f;

            keyHint.rectTransform.localScale = _hintScale * pulse;
        }
    }

    public void GainEnergy()
    {
        _energy = Mathf.Min(100f, _energy + energyPerHit);
        RefreshUI();
    }

    public bool TryConsumePower()
    {
        if (!_armed) return false;

        _armed = false;
        _energy = 0f;
        RefreshUI();

        return true;
    }

    private void RefreshUI()
    {
        if (energyBar != null)
            energyBar.value = _energy;

        if (keyHint == null) return;

        if (_armed)
            keyHint.text = "POWER READY";
        else if (_energy >= 100f)
            keyHint.text = $"[{keyLabel}] POWER SHOT";
        else
            keyHint.text = "";
    }
}