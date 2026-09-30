using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Paddle paddle;

    // Key to control the game
    public enum ControlScheme
    {
        WS,
        ArrowKeys
    }

    public ControlScheme controls = ControlScheme.WS;

    // Update is called once per frame
    private void Update()
    {
        if (paddle == null) return;
        Vector2 direction = Vector2.zero;
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            bool upPressed;
            bool downPressed;

            if (controls == ControlScheme.WS)
            {
                upPressed = keyboard.wKey.isPressed;
                downPressed = keyboard.sKey.isPressed;
            }

            else
            {
                upPressed = keyboard.upArrowKey.isPressed;
                downPressed = keyboard.downArrowKey.isPressed;
            }

            // Pressing both directions will cancel movement input.
            float vertical =
                    (upPressed ? 1f : 0f) - (downPressed ? 1f : 0f);

            direction = new Vector2(0f, vertical);
        }

        paddle.direction = direction;
    }

    private void OnDisable()
    {
        if (paddle != null)
            paddle.direction = Vector2.zero;
    }
}