using UnityEngine;
using UnityEngine.InputSystem;

namespace Pong
{
    [RequireComponent(typeof(PaddleMovement))]
    public sealed class PlayerPaddleInput : MonoBehaviour
    {
        private PaddleMovement paddle;

        private void Awake()
        {
            paddle = GetComponent<PaddleMovement>();
        }

        private void Update()
        {
            paddle.SetDirection(ReadDirection());
        }

        private static float ReadDirection()
        {
            float direction = 0f;

            if (Keyboard.current != null)
            {
                direction += Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f;
                direction -= Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? 1f : 0f;
            }

            if (Gamepad.current != null)
            {
                float gamepadDirection = Gamepad.current.leftStick.y.ReadValue();
                if (Mathf.Abs(gamepadDirection) < 0.15f)
                {
                    gamepadDirection = Gamepad.current.dpad.y.ReadValue();
                }

                if (Mathf.Abs(gamepadDirection) > Mathf.Abs(direction))
                {
                    direction = gamepadDirection;
                }
            }

            return Mathf.Clamp(direction, -1f, 1f);
        }
    }
}
