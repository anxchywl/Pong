using UnityEngine;
using UnityEngine.InputSystem;

namespace Pong
{
    /// Drives one paddle from the input profile its seat was assigned. Keyboard profiles carry their
    /// own key pair, so several players can share one keyboard; gamepad profiles bind to one device.
    [RequireComponent(typeof(PaddleMovement))]
    public sealed class PlayerPaddleInput : MonoBehaviour
    {
        private const float StickDeadZone = 0.15f;

        private PaddleMovement paddle;
        private InputProfileDefinition profile;
        private int deviceId;
        private Gamepad boundGamepad;

        private void Awake()
        {
            paddle = GetComponent<PaddleMovement>();
        }

        public void Bind(InputProfileDefinition value, int device)
        {
            profile = value;
            deviceId = device;
            boundGamepad = null;
        }

        private void OnDisable()
        {
            paddle.SetDirection(0f);
        }

        private void Update()
        {
            paddle.SetDirection(profile == null ? 0f : ReadDirection());
        }

        private float ReadDirection()
        {
            return profile.Kind == InputProfileKind.Gamepad ? ReadGamepad() : ReadKeyboard();
        }

        private float ReadKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return 0f;
            }

            float direction = 0f;
            direction += keyboard[profile.MoveUpKey].isPressed ? 1f : 0f;
            direction -= keyboard[profile.MoveDownKey].isPressed ? 1f : 0f;
            return direction;
        }

        private float ReadGamepad()
        {
            Gamepad gamepad = ResolveGamepad();
            if (gamepad == null)
            {
                return 0f;
            }

            float direction = gamepad.leftStick.y.ReadValue();
            if (Mathf.Abs(direction) < StickDeadZone)
            {
                direction = gamepad.dpad.y.ReadValue();
            }

            return Mathf.Clamp(direction, -1f, 1f);
        }

        private Gamepad ResolveGamepad()
        {
            if (boundGamepad != null && boundGamepad.added && boundGamepad.deviceId == deviceId)
            {
                return boundGamepad;
            }

            boundGamepad = null;
            foreach (Gamepad candidate in Gamepad.all)
            {
                if (candidate.deviceId == deviceId)
                {
                    boundGamepad = candidate;
                    break;
                }
            }

            return boundGamepad;
        }
    }
}
