using System;

namespace Pong
{
    /// A seat's occupant plus, for a human, the input profile and physical device driving it.
    public readonly struct SeatAssignment : IEquatable<SeatAssignment>
    {
        public const int NoDevice = 0;

        private SeatAssignment(SeatOccupant occupant, string profileId, int deviceId, bool exclusive)
        {
            Occupant = occupant;
            ProfileId = profileId;
            DeviceId = deviceId;
            Exclusive = exclusive;
        }

        public static SeatAssignment Empty => new SeatAssignment(SeatOccupant.Empty, string.Empty, NoDevice, true);

        public static SeatAssignment Computer => new SeatAssignment(SeatOccupant.Computer, string.Empty, NoDevice, true);

        /// Seats a profile, which knows for itself whether it can be shared. Prefer this to Human:
        /// remembering to say a touchscreen is shareable at every call site is a rule waiting to be
        /// forgotten, and forgetting it silently evicts the player who was already sitting there.
        public static SeatAssignment For(InputProfileDefinition profile, int deviceId)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return Human(profile.Id, deviceId, profile.Kind != InputProfileKind.Touch);
        }

        /// A driver normally holds one paddle, so claiming it elsewhere vacates the seat it left.
        /// Some drivers are shared instead: a touchscreen is one device that several seats read,
        /// told apart by which part of the court a finger lands on rather than by hardware. Those
        /// are not exclusive, and the roster must let them sit in more than one seat at once.
        public static SeatAssignment Human(string profileId, int deviceId, bool exclusive = true)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                throw new ArgumentException("A human seat needs an input profile", nameof(profileId));
            }

            return new SeatAssignment(SeatOccupant.Human, profileId, deviceId, exclusive);
        }

        public SeatOccupant Occupant { get; }
        public string ProfileId { get; }
        public int DeviceId { get; }

        /// False when this driver may hold several seats at once, as a shared touchscreen does.
        public bool Exclusive { get; }

        public bool IsOccupied => Occupant != SeatOccupant.Empty;

        public bool Equals(SeatAssignment other)
        {
            return Occupant == other.Occupant && ProfileId == other.ProfileId &&
                DeviceId == other.DeviceId && Exclusive == other.Exclusive;
        }

        public override bool Equals(object obj)
        {
            return obj is SeatAssignment other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Occupant, ProfileId, DeviceId, Exclusive);
        }
    }
}
