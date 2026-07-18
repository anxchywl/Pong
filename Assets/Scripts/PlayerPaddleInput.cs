using UnityEngine;
using UnityEngine.InputSystem;

namespace Pong
{
    /// Turns one seat's Move action into paddle intent. The seat's profile picks a control scheme;
    /// which keys or which stick that means is PongControls' business, not this component's.
    [RequireComponent(typeof(PaddleMovement))]
    public sealed class PlayerPaddleInput : MonoBehaviour
    {
        private PaddleMovement paddle;
        private InputActionAsset controls;
        private InputAction move;

        private void Awake()
        {
            paddle = GetComponent<PaddleMovement>();
        }

        /// Gives this paddle its own copy of the actions, masked to the profile's scheme. A copy per
        /// paddle is what lets two seats read one keyboard without sharing enabled state.
        public void Bind(InputActionAsset source, InputProfileDefinition profile, int deviceId)
        {
            Release();
            if (source == null || profile == null)
            {
                return;
            }

            controls = Instantiate(source);
            InputActionMap gameplay = controls.FindActionMap("Gameplay");
            gameplay.bindingMask = InputBinding.MaskByGroup(profile.ControlScheme);

            // a gamepad seat drives one pad and must ignore the others. A keyboard seat is already
            // told apart by its scheme, so leaving its devices open costs nothing
            if (profile.RequiresDevice)
            {
                Gamepad pad = FindGamepad(deviceId);
                if (pad == null)
                {
                    Release();
                    return;
                }

                controls.devices = new InputDevice[] { pad };
            }

            gameplay.Enable();
            move = gameplay.FindAction("Move");
        }

        private void OnDisable()
        {
            paddle.SetDirection(0f);
            Release();
        }

        private void Update()
        {
            paddle.SetDirection(move == null ? 0f : move.ReadValue<float>());
        }

        private void Release()
        {
            move = null;
            if (controls == null)
            {
                return;
            }

            controls.Disable();
            Destroy(controls);
            controls = null;
        }

        private static Gamepad FindGamepad(int deviceId)
        {
            foreach (Gamepad candidate in Gamepad.all)
            {
                if (candidate.deviceId == deviceId)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
