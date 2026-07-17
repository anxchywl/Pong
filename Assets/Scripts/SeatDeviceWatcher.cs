using UnityEngine;
using UnityEngine.InputSystem;

namespace Pong
{
    /// Watches pads arrive and leave, and keeps the court honest about it. A seat keeps its claim
    /// across a disconnect: a cable pulled mid-rally should cost a pause, not the seat.
    public sealed class SeatDeviceWatcher : MonoBehaviour
    {
        [SerializeField] private SeatDirector seats;
        [SerializeField] private MatchController match;

        private void OnEnable()
        {
            InputSystem.onDeviceChange += HandleDeviceChange;
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= HandleDeviceChange;
        }

        private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is not Gamepad || !seats.Roster.IsDeviceClaimed(device.deviceId))
            {
                return;
            }

            switch (change)
            {
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    // the paddle stops on its own once the device is gone, but a live ball would
                    // keep scoring against a player who is holding a dead pad
                    seats.Refresh();
                    match.PauseMatch();
                    break;
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    // the seat never let go, so the pad picks its paddle back up. Resuming is left
                    // to the player: they may not have both hands back yet
                    seats.Refresh();
                    break;
            }
        }
    }
}
