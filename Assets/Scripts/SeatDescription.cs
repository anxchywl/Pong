using UnityEngine.InputSystem;

namespace Pong
{
    /// Shared wording for seats, so the menu card and the players screen never drift apart.
    public static class SeatDescription
    {
        public static string Role(SeatRole role)
        {
            return role == SeatRole.Goalkeeper ? "GOALKEEPER" : "ATTACKER";
        }

        public static string Occupant(SeatAssignment assignment, InputProfileCatalog profiles)
        {
            switch (assignment.Occupant)
            {
                case SeatOccupant.Computer:
                    return "Computer";
                case SeatOccupant.Human:
                    InputProfileDefinition profile = profiles.Find(assignment.ProfileId);
                    if (profile == null)
                    {
                        return "Player";
                    }

                    return profile.RequiresDevice
                        ? $"{profile.DisplayName} {DeviceOrdinal(assignment.DeviceId)}"
                        : profile.DisplayName;
                default:
                    return "Empty";
            }
        }

        /// Gamepads are addressed by an arbitrary device id, which means nothing to a player.
        /// Number them by the order they are plugged in instead.
        private static string DeviceOrdinal(int deviceId)
        {
            for (int index = 0; index < Gamepad.all.Count; index++)
            {
                if (Gamepad.all[index].deviceId == deviceId)
                {
                    return (index + 1).ToString();
                }
            }

            return "—";
        }

        public static string Hint(SeatAssignment assignment, InputProfileCatalog profiles)
        {
            switch (assignment.Occupant)
            {
                case SeatOccupant.Computer:
                    return "Plays on its own";
                case SeatOccupant.Human:
                    InputProfileDefinition profile = profiles.Find(assignment.ProfileId);
                    return profile == null ? string.Empty : profile.Hint;
                default:
                    return "Select to add a player";
            }
        }
    }
}
