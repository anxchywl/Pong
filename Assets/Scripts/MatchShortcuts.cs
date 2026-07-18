using UnityEngine;
using UnityEngine.InputSystem;

namespace Pong
{
    /// Drives pause and restart from the Gameplay map. This lives outside the match so the match
    /// itself never has to know a keyboard exists; it only offers the two commands it always had.
    public sealed class MatchShortcuts : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField] private InputActionAsset controls;

        private InputAction pause;
        private InputAction restart;

        private void OnEnable()
        {
            InputActionMap gameplay = controls.FindActionMap("Gameplay");

            // no binding mask and no device list: pause and restart answer to whoever reaches them,
            // which is what a shared-screen match wants
            pause = gameplay.FindAction("Pause");
            restart = gameplay.FindAction("Restart");
            pause.performed += HandlePause;
            restart.performed += HandleRestart;
            pause.Enable();
            restart.Enable();
        }

        private void OnDisable()
        {
            pause.performed -= HandlePause;
            restart.performed -= HandleRestart;
            pause.Disable();
            restart.Disable();
        }

        private void HandlePause(InputAction.CallbackContext context)
        {
            match.TogglePause();
        }

        private void HandleRestart(InputAction.CallbackContext context)
        {
            match.RestartMatch();
        }
    }
}
