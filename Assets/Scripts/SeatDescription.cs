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
                    return profile == null ? "Player" : profile.DisplayName;
                default:
                    return "Empty";
            }
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
